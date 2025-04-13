using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Linq; // Add Linq
using GitTool.Editor.Action; // Add this line

[InitializeOnLoad]
public class OnSceneSavedHandler
{
  // Find the specific action for scene save
  private static TriggerActionDefinition SceneSaveAction = TriggerAction.Actions.FirstOrDefault(a => a.ActionName == "On Scene Save");

  static OnSceneSavedHandler()
  {
    EditorApplication.delayCall += () =>
    {
      // Use the PreferenceKey from the found action
      if (SceneSaveAction != null && EditorPrefs.GetBool(SceneSaveAction.GetEnabledEditorPrefsKey(), false))
      {
        EditorSceneManager.sceneSaved += OnSceneSaved;
      }
      else if (SceneSaveAction == null)
      {
        Debug.LogWarning("Git Tool: 'On Scene Save' TriggerAction not found.");
      }
    };
  }

  private static async void OnSceneSaved(Scene scene)
  {
    // Ensure the action is defined before proceeding
    if (SceneSaveAction == null)
    {
      Debug.LogError("Git Tool: SceneSaveAction is not defined. Cannot proceed.");
      return;
    }
    // Check if this specific action is enabled (redundant check if subscription logic is correct, but safe)
    if (!EditorPrefs.GetBool(SceneSaveAction.GetEnabledEditorPrefsKey(), false))
    {
      // This should ideally not happen if the event subscription is managed correctly
      Debug.LogWarning("Git Tool: OnSceneSaved called but the action is disabled in settings.");
      return;
    }

    string repositoryPath = GitToolSettings.RepositoryPath;
    // Pass the SceneSaveAction object to get the commit message
    string commitMessage = GetCommitMessageForAction(SceneSaveAction, scene);

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

    // --- Check Status ---
    var (statusOutput, statusError, statusSuccess) = await GitOperations.StatusAsync(repositoryPath);

    if (!statusSuccess)
    {
      string errorMessage = $"Git Tool: Status check failed: {statusError}";
      Debug.LogError(errorMessage);
      //EditorUtility.DisplayDialog("Git Status Error", errorMessage, "OK");
      return;
    }

    // Check if there are changes to commit
    // Note: Checking for "nothing to commit" might not be robust enough for all git versions/languages.
    // Consider `git diff --quiet --exit-code` or `git diff-index --quiet HEAD --` for a more reliable check.
    if (statusOutput != null && statusOutput.Contains("nothing to commit"))
    {
      Debug.Log("Git Tool: No changes detected. Skipping commit.");
      return;
    }

    // --- Add ---
    var (addOutput, addError, addSuccess) = await GitOperations.AddAsync(repositoryPath);

    if (!addSuccess)
    {
      string errorMessage = $"Git Tool: Add failed: {addError}\nOutput:\n{addOutput}";
      Debug.LogError(errorMessage);
      //EditorUtility.DisplayDialog("Git Add Error", errorMessage, "OK");
      return;
    }

    // --- Commit ---
    var (commitOutput, commitError, commitSuccess) = await GitOperations.CommitAsync(repositoryPath, commitMessage);

    if (!commitSuccess)
    {
      string errorMessage = $"Git Tool: Commit failed: {commitError}\nOutput:\n{commitOutput}";
      Debug.LogError(errorMessage);
      //EditorUtility.DisplayDialog("Git Commit Error", errorMessage, "OK");
      return;
    }
    else
    {
      Debug.Log($"Git Tool: Commit completed successfully.\nOutput:\n{commitOutput}");
    }

    // --- Push (if enabled) ---
    if (GitToolSettings.IsAutoPushEnabled)
    {
      string remoteName = GitToolSettings.PushRemoteName;
      if (string.IsNullOrEmpty(remoteName))
      {
        Debug.LogWarning("Git Tool: Auto Push is enabled but Remote Name is not set. Skipping push.");
        return;
      }

      Debug.Log($"Git Tool: Attempting to push to remote '{remoteName}'...");
      var (pushOutput, pushError, pushSuccess) = await GitOperations.PushAsync(repositoryPath, remoteName);

      if (!pushSuccess)
      {
        string errorMessage = $"Git Tool: Push failed: {pushError}\nOutput:\n{pushOutput}";
        Debug.LogError(errorMessage);
        //EditorUtility.DisplayDialog("Git Push Error", errorMessage, "OK");
      }
      else
      {
        Debug.Log($"Git Tool: Push completed successfully.\nOutput:\n{pushOutput}");
      }
    }
  }

  // Updated method to accept TriggerAction object
  private static string GetCommitMessageForAction(TriggerActionDefinition action, Scene scene)
  {
    if (action == null)
    {
      Debug.LogError("Git Tool: TriggerAction object is null in GetCommitMessageForAction.");
      return ""; // Return empty string or handle error appropriately
    }

    // Use properties from the TriggerAction object
    string template = GitToolSettings.GetTriggerActionTemplate(action.TemplatePreferenceKey, action.DefaultTemplate);

    // Replace placeholders
    template = template.Replace("{action}", action.ActionName); // Use action.Name
    if (scene != null)
    {
      template = template.Replace("{sceneName}", scene.name);
    }
    return template;
  }
}
