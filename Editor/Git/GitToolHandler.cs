using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

[InitializeOnLoad]
public static class GitToolHandler
{
  static GitToolHandler()
  {
    EditorApplication.delayCall += () =>
    {
      if (EditorPrefs.GetBool("GitTool_TriggerOnSceneSave", true))
      {
        EditorSceneManager.sceneSaved += OnSceneSaved;
      }
    };
  }

  private static async void OnSceneSaved(Scene scene)
  {
    string repositoryPath = EditorPrefs.GetString("GitTool_RepositoryPath", "");
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

    // Check if there are any changes to commit
    (string statusOutput, string statusError, bool statusSuccess) = await GitToolOperations.StatusAsync(repositoryPath);

    if (!statusSuccess)
    {
      Debug.LogError($"Git Tool: Status check failed: {statusError}");
      return;
    }

    // If there are no changes, skip the commit
    if (statusOutput.Contains("nothing to commit, working tree clean"))
    {
      Debug.Log("Git Tool: No changes detected. Skipping commit.");
      return;
    }

    (string addOutput, string addError, bool addSuccess) = await GitToolOperations.AddAsync(repositoryPath);

    if (!addSuccess)
    {
      Debug.LogError($"Git Tool: Add failed: {addError}");
      return;
    }

    (string commitOutput, string commitError, bool commitSuccess) = await GitToolOperations.CommitAsync(repositoryPath, commitMessage);

    if (!commitSuccess)
    {
      Debug.LogError($"Git Tool: Commit failed: {commitError}");
    }

    if (EditorPrefs.GetBool("GitTool_AutoPushEnabled", false))
    {
      string remoteName = EditorPrefs.GetString("GitTool_RemoteName", "origin");
      (string pushOutput, string pushError, bool pushSuccess) = await GitToolOperations.PushAsync(repositoryPath, remoteName);

      if (!pushSuccess)
      {
        Debug.LogError($"Git Tool: Push failed: {pushError}");
      }

      if (commitSuccess && pushSuccess)
      {
        Debug.Log("Git Tool: Commit and Push completed successfully.");
      }
      else if (commitSuccess)
      {
        Debug.Log("Git Tool: Commit completed successfully.");
      }
      else
      {
        Debug.LogError($"Git Tool: Commit failed: {commitError}");
      }
    }
    else
    {
      if (commitSuccess)
      {
        Debug.Log("Git Tool: Commit completed successfully.");
      }
      else
      {
        Debug.LogError($"Git Tool: Commit failed: {commitError}");
      }
    }
  }

  private static string GetCommitMessageForAction(string actionName, Scene scene = default)
  {
    TriggerAction action = TriggerAction.Actions.Find(a => a.Name == actionName);
    if (action == null)
    {
      Debug.LogError($"Trigger action '{actionName}' not found.");
      return null;
    }

    string template;
    if (EditorPrefs.GetBool(action.OverrideTemplateKey, false))
    {
      template = EditorPrefs.GetString(action.TemplateKey, action.DefaultTemplate);
    }
    else
    {
      template = EditorPrefs.GetString("GitTool_CommitMessageTemplate", action.DefaultTemplate);
    }

    string actionDescription = "";
    if (actionName == "On Scene Save" && scene != default)
    {
      actionDescription = $"Scene saved - \"{scene.name}\"";
    }
    // Add other action descriptions here (e.g., for build complete)
    // else if (actionName == "On Build Complete" && report != null) { ... }

    return template.Replace("{action}", actionDescription);
  }
}
