
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

public class GitToolSettingsUI : EditorWindow
{
  private string _guiGitRepositoryPath = "";
  private bool _guiAutoPushEnabled = false;
  private string _guiRemoteName = "origin";

  // Trigger action states are read/written directly in OnGUI

  [MenuItem("Tools/Git Tool Settings")]
  public static void ShowWindow()
  {
    EditorWindow.GetWindow<GitToolSettingsUI>("Git Tool Settings");
  }
  private void OnEnable()
  {
    LoadSettingsForGUI();
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
    }

    if (GUILayout.Button("Set Repository Path"))
    {
      string selectedPath = EditorUtility.OpenFolderPanel("Select Git Repository Path", _guiGitRepositoryPath, "");
      if (!string.IsNullOrEmpty(selectedPath))
      {
        _guiGitRepositoryPath = selectedPath;
        GitToolSettings.RepositoryPath = _guiGitRepositoryPath;
      }
    }

    if (!string.IsNullOrEmpty(_guiGitRepositoryPath))
    {
      try
      {
        if (!Directory.Exists(Path.Combine(_guiGitRepositoryPath, ".git")))
        {
          GUILayout.Space(5);
          if (!GitCommandHandler.IsGitInstalled())
          {
            EditorGUILayout.HelpBox("Git is not installed. Please install Git to use this tool.", MessageType.Warning);
            if (GUILayout.Button("Download Git"))
            {
              Application.OpenURL("https://git-scm.com/downloads");
            }
          }
          else if (GUILayout.Button("Initialize Git Repository"))
          {
            GitOperations.Init(_guiGitRepositoryPath, out string initOutput, out string initError);
            if (string.IsNullOrEmpty(initError)) { Debug.Log("Git repository initialized successfully."); }
            else { Debug.LogError($"Failed to initialize Git repository: {initError}"); }
          }
        }
      }
      catch (System.Exception ex)
      {
        EditorGUILayout.HelpBox($"Error checking repository path: {ex.Message}", MessageType.Error);
      }
    }
    else
    {
      EditorGUILayout.HelpBox("Repository path is not set.", MessageType.Info);
    }

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

    // Assuming TriggerAction class and Actions list exist and are accessible
    if (TriggerAction.Actions != null)
    {
      foreach (var action in TriggerAction.Actions)
      {
        // Check for null action or keys before proceeding
        if (action == null || string.IsNullOrEmpty(action.PrefKey) || string.IsNullOrEmpty(action.TemplateKey))
        {
          EditorGUILayout.HelpBox("Invalid TriggerAction detected.", MessageType.Warning);
          continue;
        }

        string isEnabledKey = GitToolSettings.GetTriggerActionEnabledKey(action.PrefKey);
        string templateKey = GitToolSettings.GetTriggerActionTemplateKey(action.TemplateKey);

        EditorGUI.BeginChangeCheck();
        // Read directly from EditorPrefs for the toggle's current state
        bool isEnabled = GitToolSettings.IsTriggerActionEnabled(isEnabledKey); // Default to true
        isEnabled = EditorGUILayout.Toggle(action.Name, isEnabled);
        if (EditorGUI.EndChangeCheck())
        {
          // Save change directly to EditorPrefs
          GitToolSettings.SetTriggerActionEnabled(isEnabledKey, isEnabled);
        }

        // Use the *just updated* value from EditorPrefs to decide whether to show the template field
        if (GitToolSettings.IsTriggerActionEnabled(isEnabledKey))
        {
          EditorGUI.BeginChangeCheck();
          // Read directly from EditorPrefs for the template field's current state
          string template = GitToolSettings.GetTriggerActionTemplate(templateKey, action.DefaultTemplate);
          template = EditorGUILayout.TextField("Commit message template", template);
          if (EditorGUI.EndChangeCheck())
          {
            // Save change directly to EditorPrefs
            GitToolSettings.SetTriggerActionTemplate(templateKey, template);
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
}
