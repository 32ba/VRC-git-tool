using System.Threading.Tasks;

public static class GitOperations
{
  public static async Task<(string output, string error, bool success)> StatusAsync(string repositoryPath)
  {
    string statusArgs = "status";
    var result = await GitCommandHandler.ExecuteGitCommandAsync(repositoryPath, statusArgs);
    return (result.output, result.error, result.exitCode == 0);
  }

  public static async Task<(string output, string error, bool success)> AddAsync(string repositoryPath)
  {
    string addArgs = "add ."; // Consider adding specific files instead of '.' for more control
    var result = await GitCommandHandler.ExecuteGitCommandAsync(repositoryPath, addArgs);
    return (result.output, result.error, result.exitCode == 0);
  }

  public static async Task<(string output, string error, bool success)> CommitAsync(string repositoryPath, string commitMessage)
  {
    // Ensure commit message is properly escaped for the command line
    // Simple escaping for quotes, might need more robust solution for complex messages
    string escapedMessage = commitMessage.Replace("\"", "\\\"");
    string commitArgs = $"commit -m \"{escapedMessage}\"";
    var result = await GitCommandHandler.ExecuteGitCommandAsync(repositoryPath, commitArgs);
    return (result.output, result.error, result.exitCode == 0);
  }

  public static async Task<(string output, string error, bool success)> PushAsync(string repositoryPath, string remoteName)
  {
    // Ensure remote name is valid before using
    string pushArgs = $"push {remoteName}"; // Consider adding branch name: `push {remoteName} current-branch`
    var result = await GitCommandHandler.ExecuteGitCommandAsync(repositoryPath, pushArgs);
    return (result.output, result.error, result.exitCode == 0);
  }

  public static async Task<(string output, string error, bool success)> InitAsync(string repositoryPath)
  {
    string initArgs = "init";
    var result = await GitCommandHandler.ExecuteGitCommandAsync(repositoryPath, initArgs);
    return (result.output, result.error, result.exitCode == 0);
  }

  // Synchronous methods removed as they block the main thread and are unsafe in Unity Editor context.
  // Callers should be updated to use the Async versions.
}
