using UnityEngine;
using UnityEditor;
using System;
using System.Diagnostics;
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

  public static async Task<(string output, string error, bool success)> ExecuteGitCommandAsync(string repositoryPath, string arguments)
  {
    string output = string.Empty;
    string error = string.Empty;
    bool success = false;

    try
    {
      ProcessStartInfo startInfo = new ProcessStartInfo
      {
        FileName = "git",
        Arguments = $"-C \"{repositoryPath}\" {arguments}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      using (Process process = new Process())
      {
        process.StartInfo = startInfo;
        process.Start();

        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        process.EnableRaisingEvents = true;
        process.Exited += (sender, e) =>
        {
          output = process.StandardOutput.ReadToEnd();
          error = process.StandardError.ReadToEnd();
          success = process.ExitCode == 0;
          tcs.SetResult(true);
        };

        await tcs.Task;
      }
    }
    catch (Exception ex)
    {
      error = $"Error executing Git command: {ex.Message}";
      success = false;
    }

    return (output, error, success);
  }

  public static bool ExecuteGitCommand(string repositoryPath, string arguments, out string output, out string error)
  {
    var result = ExecuteGitCommandAsync(repositoryPath, arguments).Result;
    output = result.output;
    error = result.error;
    return result.success;
  }
}
