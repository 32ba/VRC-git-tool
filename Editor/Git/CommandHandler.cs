using UnityEngine;
using UnityEditor;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

public static class GitCommandHandler
{
  public static bool IsGitInstalled()
  {
    try
    {
      ProcessStartInfo startInfo = new ProcessStartInfo
      {
        FileName = "git",
        Arguments = "--version",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      using (Process process = Process.Start(startInfo))
      {
        process.WaitForExit();
        return process.ExitCode == 0;
      }
    }
    catch
    {
      return false;
    }
  }

  public static async Task<(string output, string error, int exitCode)> ExecuteGitCommandAsync(string repositoryPath, string arguments)
  {
    StringBuilder outputBuilder = new StringBuilder();
    StringBuilder errorBuilder = new StringBuilder();
    int exitCode = -1;

    try
    {
      ProcessStartInfo startInfo = new ProcessStartInfo
      {
        FileName = "git",
        Arguments = $"-C \"{repositoryPath}\" {arguments}", // Consider more robust argument escaping if necessary
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
        StandardOutputEncoding = Encoding.UTF8, // Specify encoding if needed
        StandardErrorEncoding = Encoding.UTF8
      };

      using (Process process = new Process())
      {
        process.StartInfo = startInfo;

        // Use TaskCompletionSource to signal completion of output/error streams and process exit
        var processExitTcs = new TaskCompletionSource<int>();
        var outputTcs = new TaskCompletionSource<bool>();
        var errorTcs = new TaskCompletionSource<bool>();

        process.EnableRaisingEvents = true;

        process.OutputDataReceived += (sender, e) =>
        {
          if (e.Data == null)
          {
            outputTcs.TrySetResult(true);
          }
          else
          {
            outputBuilder.AppendLine(e.Data);
          }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
          if (e.Data == null)
          {
            errorTcs.TrySetResult(true);
          }
          else
          {
            errorBuilder.AppendLine(e.Data);
          }
        };

        process.Exited += (sender, e) =>
        {
          // Ensure output/error streams are closed before setting process exit result
          Task.WhenAll(outputTcs.Task, errorTcs.Task).ContinueWith(t =>
          {
            // Guard against cases where Exited event fires before streams are fully processed
            // It's generally safer to check exit code after waiting for streams.
            // However, in some edge cases, Exited might fire early.
            // We prioritize capturing the exit code here.
            try
            {
              exitCode = process.ExitCode;
            }
            catch (InvalidOperationException)
            {
              // Process might have already exited and disposed internally
              // We might rely on the TCS status or a default error code
              exitCode = -1; // Indicate potential issue
            }
            processExitTcs.TrySetResult(exitCode);
          });
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Wait for the process to exit and streams to be processed
        exitCode = await processExitTcs.Task;

        // Fallback check in case Exited event had issues getting the code
        if (exitCode == -1)
        {
          try
          {
            if (process.HasExited) exitCode = process.ExitCode;
          }
          catch { } // Ignore if process is inaccessible
        }
      }
    }
    catch (Exception ex)
    {
      errorBuilder.AppendLine($"Error executing Git command: {ex.Message}");
      exitCode = -1; // Indicate failure
    }

    return (outputBuilder.ToString(), errorBuilder.ToString(), exitCode);
  }

  // Keep the synchronous wrapper for now, but mark as obsolete or update its logic
  // Or remove it if all callers are updated to use the async version.
  [Obsolete("Use ExecuteGitCommandAsync instead.")]
  public static bool ExecuteGitCommand(string repositoryPath, string arguments, out string output, out string error)
  {
    // This synchronous wrapper will block the main thread.
    // Consider removing it entirely in the future.
    var task = ExecuteGitCommandAsync(repositoryPath, arguments);
    // Using .Result or .Wait() on the main thread can cause deadlocks in some contexts (like UI).
    // It's generally unsafe in Unity Editor scripts unless carefully managed.
    // For simplicity here, we use .Result, but acknowledge the risk.
    var result = task.Result;
    output = result.output;
    error = result.error;
    return result.exitCode == 0;
  }
}
