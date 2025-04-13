using UnityEngine;
using System.Threading.Tasks;

public static class GitOperations
{
  public static async Task<(string output, string error, bool success)> StatusAsync(string repositoryPath)
  {
    string statusArgs = "status";
    return await GitCommandHandler.ExecuteGitCommandAsync(repositoryPath, statusArgs);
  }

  public static async Task<(string output, string error, bool success)> AddAsync(string repositoryPath)
  {
    string addArgs = "add .";
    return await GitCommandHandler.ExecuteGitCommandAsync(repositoryPath, addArgs);
  }

  public static async Task<(string output, string error, bool success)> CommitAsync(string repositoryPath, string commitMessage)
  {
    string commitArgs = $"commit -m \"{commitMessage}\"";
    return await GitCommandHandler.ExecuteGitCommandAsync(repositoryPath, commitArgs);
  }

  public static async Task<(string output, string error, bool success)> PushAsync(string repositoryPath, string remoteName)
  {
    string pushArgs = $"push {remoteName}";
    return await GitCommandHandler.ExecuteGitCommandAsync(repositoryPath, pushArgs);
  }

  public static bool Init(string repositoryPath, out string output, out string error)
  {
    string initArgs = "init";
    return GitCommandHandler.ExecuteGitCommand(repositoryPath, initArgs, out output, out error);
  }

  public static bool Add(string repositoryPath, out string output, out string error)
  {
    var result = AddAsync(repositoryPath).Result;
    output = result.output;
    error = result.error;
    return result.success;
  }

  public static bool Commit(string repositoryPath, string commitMessage, out string output, out string error)
  {
    var result = CommitAsync(repositoryPath, commitMessage).Result;
    output = result.output;
    error = result.error;
    return result.success;
  }

  public static bool Push(string repositoryPath, string remoteName, out string output, out string error)
  {
    var result = PushAsync(repositoryPath, remoteName).Result;
    output = result.output;
    error = result.error;
    return result.success;
  }
}
