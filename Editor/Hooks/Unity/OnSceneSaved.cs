using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

[InitializeOnLoad]
public class OnSceneSavedHandler
{
  static OnSceneSavedHandler()
  {
    EditorApplication.delayCall += () =>
    {
      if (GitToolSettings.IsTriggerActionEnabled("OnSceneSave"))
      {
        EditorSceneManager.sceneSaved += OnSceneSaved;
      }
    };
  }

  private static async void OnSceneSaved(Scene scene)
  {
    string repositoryPath = GitToolSettings.RepositoryPath;
    string commitMessage = GetCommitMessageForAction("On Scene Save", scene);

    if (string.IsNullOrEmpty(repositoryPath))
    {
      Debug.LogWarning("Git repository path is not set. Please configure it in the Git Tool Settings.");
      return;
    }

    if (string.IsNullOrEmpty(commitMessage))
    {
      Debug.LogWarning("Commit message is empty. Skipping commit.");
      return;
    }

    (string statusOutput, string statusError, bool statusSuccess) = await GitOperations.StatusAsync(repositoryPath);

    if (!statusSuccess)
    {
      Debug.LogError($"Git Tool: Status check failed: {statusError}");
      return;
    }

    if (statusOutput != null && statusOutput.Contains("nothing to commit"))
    {
      Debug.Log("Git Tool: No changes detected. Skipping commit.");
      return;
    }


    (string addOutput, string addError, bool addSuccess) = await GitOperations.AddAsync(repositoryPath);

    if (!addSuccess)
    {
      Debug.LogError($"Git Tool: Add failed: {addError}");
      return;
    }

    (string commitOutput, string commitError, bool commitSuccess) = await GitOperations.CommitAsync(repositoryPath, commitMessage);

    if (!commitSuccess)
    {
      Debug.LogError($"Git Tool: Commit failed: {commitError}");
      return;
    }
    else
    {
      Debug.Log("Git Tool: Commit completed successfully.");
    }

    if (GitToolSettings.IsAutoPushEnabled)
    {
      string remoteName = GitToolSettings.PushRemoteName;
      (string pushOutput, string pushError, bool pushSuccess) = await GitOperations.PushAsync(repositoryPath, remoteName);

      if (!pushSuccess)
      {
        Debug.LogError($"Git Tool: Push failed: {pushError}");
      }
      else
      {
        Debug.Log("Git Tool: Push completed successfully.");
      }
    }
  }

  private static string GetCommitMessageForAction(string actionName, Scene scene)
  {
    string templateKey = $"GitTool_{actionName}MessageTemplate";
    string defaultTemplate = "Auto-commit: {action}";
    string template = GitToolSettings.GetTriggerActionTemplate(templateKey, defaultTemplate);

    template = template.Replace("{action}", actionName);
    if (scene != null)
    {
      template = template.Replace("{sceneName}", scene.name);
    }
    return template;
  }
}
