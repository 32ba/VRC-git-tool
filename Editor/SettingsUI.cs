
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

public class GitToolSettingsUI : EditorWindow
{
  private string _guiGitRepositoryPath = "";
  private bool _guiAutoPushEnabled = false;
  private string _guiRemoteName = "origin";
  private bool _isRepositoryPathValid = false; // Track repository path validity

  // Trigger action states are read/written directly in OnGUI

  [MenuItem("Tools/Git Tool Settings")]
  public static void ShowWindow()
  {
    EditorWindow.GetWindow<GitToolSettingsUI>("Git Tool Settings");
  }
  private void OnEnable()
  {
    LoadSettingsForGUI();
    ValidateRepositoryPath(); // Validate on enable
  }

  private void LoadSettingsForGUI()
  {
    _guiGitRepositoryPath = GitToolSettings.RepositoryPath;
    _guiAutoPushEnabled = GitToolSettings.IsAutoPushEnabled;
    _guiRemoteName = GitToolSettings.PushRemoteName;
  }

  public void OnGUI()
  {
    GUILayout.Label("Git Tool Settings", EditorStyles.boldLabel);

    // --- Git Repository Path ---
    GUILayout.Space(10);
    GUILayout.Label("Git Repository Path", EditorStyles.boldLabel);
    EditorGUI.BeginChangeCheck();
    _guiGitRepositoryPath = EditorGUILayout.TextField("Repository Path", _guiGitRepositoryPath);
    if (EditorGUI.EndChangeCheck())
    {
      GitToolSettings.RepositoryPath = _guiGitRepositoryPath;
      ValidateRepositoryPath(); // Validate on text field change
    }

    if (GUILayout.Button("Set Repository Path"))
    {
      string selectedPath = EditorUtility.OpenFolderPanel("Select Git Repository Path", _guiGitRepositoryPath, "");
      if (!string.IsNullOrEmpty(selectedPath))
      {
        _guiGitRepositoryPath = selectedPath;
        GitToolSettings.RepositoryPath = _guiGitRepositoryPath;
        ValidateRepositoryPath(); // Validate on folder selection
      }
    }

    // Display repository status based on validation
    GUILayout.Space(5);
    if (string.IsNullOrEmpty(_guiGitRepositoryPath))
    {
      EditorGUILayout.HelpBox("Repository path is not set.", MessageType.Info);
    }
    else if (!_isRepositoryPathValid)
    {
      EditorGUILayout.HelpBox("Repository path is invalid or not a Git repository.", MessageType.Warning);
    }

    // Disable "Initialize Git Repository" button if the path is invalid
    EditorGUI.BeginDisabledGroup(_isRepositoryPathValid);
    if (GUILayout.Button("Initialize Git Repository"))
    {
      // Execute async operation without awaiting in OnGUI
      InitializeGitRepositoryAsync(_guiGitRepositoryPath);
    }
    EditorGUI.EndDisabledGroup();

    // --- Auto Push Settings ---
    GUILayout.Space(10);
    GUILayout.Label("Auto Push Settings", EditorStyles.boldLabel);
    EditorGUI.BeginChangeCheck();
    _guiAutoPushEnabled = EditorGUILayout.Toggle("Auto Push", _guiAutoPushEnabled);
    if (EditorGUI.EndChangeCheck())
    {
      GitToolSettings.IsAutoPushEnabled = _guiAutoPushEnabled;
    }

    if (_guiAutoPushEnabled)
    {
      EditorGUI.BeginChangeCheck();
      _guiRemoteName = EditorGUILayout.TextField("Remote Name", _guiRemoteName);
      if (EditorGUI.EndChangeCheck())
      {
        GitToolSettings.PushRemoteName = _guiRemoteName;
      }
    }

    // --- Trigger Actions ---
    GUILayout.Space(10);
    GUILayout.Label("Trigger Actions", EditorStyles.boldLabel);

    // Load and display trigger actions from ScriptableObjects
    if (TriggerAction.Actions != null)
    {
      foreach (var action in TriggerAction.Actions)
      {
        // Ensure action is not null
        if (action == null)
        {
          EditorGUILayout.HelpBox("Invalid TriggerAction detected (null).", MessageType.Warning);
          continue;
        }

        EditorGUI.BeginChangeCheck();
        // Use the action's properties directly
        bool isEnabled = PlayerPrefs.GetInt(action.GetEnabledPlayerPrefsKey(), 0) == 1;
        isEnabled = EditorGUILayout.Toggle(action.ActionName, isEnabled);
        if (EditorGUI.EndChangeCheck())
        {
          // Save change directly to PlayerPrefs using the action's key
          PlayerPrefs.SetInt(action.GetEnabledPlayerPrefsKey(), isEnabled ? 1 : 0);
        }

        // Show template field only if the action is enabled
        if (PlayerPrefs.GetInt(action.GetEnabledPlayerPrefsKey(), 0) == 1)
        {
          EditorGUI.BeginChangeCheck();
          // Get the template from PlayerPrefs
          string template = PlayerPrefs.GetString(action.GetTemplatePlayerPrefsKey(), action.DefaultTemplate);
          template = EditorGUILayout.TextField("Commit message template", template);
          if (EditorGUI.EndChangeCheck())
          {
            // Save the template to PlayerPrefs
            PlayerPrefs.SetString(action.GetTemplatePlayerPrefsKey(), template);
          }
        }
        GUILayout.Space(5);
      }
    }
    else
    {
      EditorGUILayout.HelpBox("TriggerAction.Actions list is not available.", MessageType.Warning);
    }
  }

  private async void InitializeGitRepositoryAsync(string path)
  {
    if (string.IsNullOrEmpty(path))
    {
      Debug.LogError("Repository path is empty.");
      return;
    }

    Debug.Log($"Attempting to initialize Git repository at: {path}");
    var (output, error, success) = await GitOperations.InitAsync(path);

    if (success)
    {
      Debug.Log($"Git repository initialized successfully.\nOutput:\n{output}");
      // Optionally force UI repaint or update state if needed
      Repaint();
    }
    else
    {
      Debug.LogError($"Failed to initialize Git repository: {error}\nOutput:\n{output}");
      //EditorUtility.DisplayDialog("Git Init Error", $"Failed to initialize Git repository.\n\nError:\n{error}\n\nOutput:\n{output}", "OK");
    }
  }

  // Validate repository path and update the _isRepositoryPathValid flag
  private void ValidateRepositoryPath()
  {
    _isRepositoryPathValid = false; // Assume invalid until proven otherwise

    if (!string.IsNullOrEmpty(_guiGitRepositoryPath))
    {
      try
      {
        _isRepositoryPathValid = Directory.Exists(Path.Combine(_guiGitRepositoryPath, ".git"));
      }
      catch (System.Exception)
      {
        // Handle exceptions during path validation (e.g., invalid characters)
        _isRepositoryPathValid = false;
      }
    }
  }
}
