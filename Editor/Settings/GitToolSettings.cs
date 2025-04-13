using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class GitToolSettings : EditorWindow
{
  private string gitRepositoryPath = "";
  private string commitMessageTemplate = "Auto-commit: {action}";
  private bool autoPushEnabled = false;
  private string remoteName = "origin";
  private Dictionary<string, bool> actionFoldouts = new Dictionary<string, bool>();

  [MenuItem("Tools/Git Tool Settings")]
  public static void ShowWindow()
  {
    GetWindow<GitToolSettings>("Git Tool Settings");
  }

  private void OnGUI()
  {
    GUILayout.Label("Git Tool Settings", EditorStyles.boldLabel);

    // Git repository path
    GUILayout.Space(10);
    GUILayout.Label("Git Repository Path", EditorStyles.boldLabel);
    if (string.IsNullOrEmpty(gitRepositoryPath))
    {
      gitRepositoryPath = System.IO.Directory.GetCurrentDirectory();
    }
    EditorGUILayout.LabelField("Repository Path", gitRepositoryPath);

    // Button to manually set repository path
    if (GUILayout.Button("Set Repository Path"))
    {
      string selectedPath = EditorUtility.OpenFolderPanel("Select Git Repository Path", gitRepositoryPath, "");
      if (!string.IsNullOrEmpty(selectedPath))
      {
        gitRepositoryPath = selectedPath;
      }
    }

    // Git Init button
    if (!System.IO.Directory.Exists(System.IO.Path.Combine(gitRepositoryPath, ".git")))
    {
      if (!GitCommandHandler.IsGitInstalled())
      {
        GUILayout.Label("Git is not installed. Please install Git to use this tool.", EditorStyles.boldLabel);
        if (GUILayout.Button("Download Git"))
        {
          Application.OpenURL("https://git-scm.com/downloads");
        }
      }
      else if (GUILayout.Button("Initialize Git Repository"))
      {
        GitToolOperations.Init(gitRepositoryPath, out string initOutput, out string initError);

        if (string.IsNullOrEmpty(initError))
        {
          Debug.Log("Git repository initialized successfully.");
        }
        else
        {
          Debug.LogError($"Failed to initialize Git repository: {initError}");
        }
      }
    }

    // Commit message templates
    GUILayout.Space(10);
    GUILayout.Label("Commit Message Templates", EditorStyles.boldLabel);

    // Default template
    commitMessageTemplate = EditorGUILayout.TextField("Default Template", commitMessageTemplate);


    // Auto Push settings
    GUILayout.Space(10);
    GUILayout.Label("Auto Push Settings", EditorStyles.boldLabel);
    autoPushEnabled = EditorGUILayout.Toggle("Auto Push", autoPushEnabled);
    if (autoPushEnabled)
    {
      remoteName = EditorGUILayout.TextField("Remote Name", remoteName);
    }

    // Trigger options
    GUILayout.Space(10);
    GUILayout.Label("Trigger Actions", EditorStyles.boldLabel);

    foreach (var action in TriggerAction.Actions)
    {
      bool isEnabled = EditorPrefs.GetBool(action.PrefKey, true);
      isEnabled = EditorGUILayout.Toggle(action.Name, isEnabled);
      EditorPrefs.SetBool(action.PrefKey, isEnabled);

      if (isEnabled)
      {
        // Initialize foldout state if needed
        if (!actionFoldouts.ContainsKey(action.PrefKey))
        {
          actionFoldouts[action.PrefKey] = false;
        }

        // Draw foldout header
        bool foldoutState = actionFoldouts[action.PrefKey];
        foldoutState = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutState, $"{action.Name} Settings");
        actionFoldouts[action.PrefKey] = foldoutState;

        if (foldoutState)
        {
          EditorGUI.indentLevel++;

          bool overrideTemplate = EditorPrefs.GetBool(action.OverrideTemplateKey, false);
          overrideTemplate = EditorGUILayout.Toggle("Override Template", overrideTemplate);
          EditorPrefs.SetBool(action.OverrideTemplateKey, overrideTemplate);

          if (overrideTemplate)
          {
            string template = EditorPrefs.GetString(action.TemplateKey, action.DefaultTemplate);
            template = EditorGUILayout.TextField("Template", template);
            EditorPrefs.SetString(action.TemplateKey, template);
          }

          EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
      }
    }

    // Save button
    if (GUILayout.Button("Save Settings"))
    {
      SaveSettings();
    }
  }

  private void SaveSettings()
  {
    EditorPrefs.SetString("GitTool_RepositoryPath", gitRepositoryPath);
    EditorPrefs.SetString("GitTool_CommitMessageTemplate", commitMessageTemplate);
    EditorPrefs.SetBool("GitTool_AutoPushEnabled", autoPushEnabled);
    EditorPrefs.SetString("GitTool_RemoteName", remoteName);

    // Save trigger action settings
    foreach (var action in TriggerAction.Actions)
    {
      // Note: The enabled state and override settings are saved directly in OnGUI
      // Only save the template string if override is enabled
      if (EditorPrefs.GetBool(action.OverrideTemplateKey, false))
      {
        // The template string is also saved directly in OnGUI,
        // but we could potentially move that logic here if needed.
      }
    }

    Debug.Log("Git Tool settings saved.");
  }

  private void OnEnable()
  {
    // Load settings
    gitRepositoryPath = EditorPrefs.GetString("GitTool_RepositoryPath", "");
    commitMessageTemplate = EditorPrefs.GetString("GitTool_CommitMessageTemplate", "Auto-commit: {action}");
    autoPushEnabled = EditorPrefs.GetBool("GitTool_AutoPushEnabled", false);
    remoteName = EditorPrefs.GetString("GitTool_RemoteName", "origin");

    // Initialize foldout dictionary
    actionFoldouts = new Dictionary<string, bool>();
  }
}
