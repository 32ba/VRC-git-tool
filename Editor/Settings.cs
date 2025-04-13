using UnityEditor;
using UnityEngine;

public static class GitToolSettings
{
  // EditorPrefs Keys
  private const string GitRepositoryPathKey = "GitTool_RepositoryPath";
  private const string AutoPushEnabledKey = "GitTool_AutoPushEnabled";
  private const string RemoteNameKey = "GitTool_RemoteName";
  private const string TriggerActionEnabledKeyPrefix = "GitTool_TriggerActionEnabled_";
  private const string TriggerActionTemplateKeyPrefix = "GitTool_TriggerActionTemplate_";

  // Public static properties to access settings from other classes
  public static string RepositoryPath { get => EditorPrefs.GetString(GitRepositoryPathKey, ""); set => EditorPrefs.SetString(GitRepositoryPathKey, value); }
  public static bool IsAutoPushEnabled { get => EditorPrefs.GetBool(AutoPushEnabledKey, false); set => EditorPrefs.SetBool(AutoPushEnabledKey, value); }
  public static string PushRemoteName { get => EditorPrefs.GetString(RemoteNameKey, "origin"); set => EditorPrefs.SetString(RemoteNameKey, value); }

  public static bool IsTriggerActionEnabled(string actionPrefKey)
  {
    // Ensure actionPrefKey is not null or empty before creating the full key
    if (string.IsNullOrEmpty(actionPrefKey)) return false;
    return EditorPrefs.GetBool(TriggerActionEnabledKeyPrefix + actionPrefKey, false); // Default to false if not set
  }

  public static string GetTriggerActionTemplate(string actionTemplateKey, string defaultTemplate)
  {
    // Ensure actionTemplateKey is not null or empty
    if (string.IsNullOrEmpty(actionTemplateKey)) return defaultTemplate;
    return EditorPrefs.GetString(TriggerActionTemplateKeyPrefix + actionTemplateKey, defaultTemplate);
  }


  public static string GetTriggerActionEnabledKey(string actionPrefKey)
  {
    return TriggerActionEnabledKeyPrefix + actionPrefKey;
  }
  public static void SetTriggerActionEnabled(string actionPrefKey, bool isEnabled)
  {
    // Ensure actionPrefKey is not null or empty
    if (string.IsNullOrEmpty(actionPrefKey)) return;
    EditorPrefs.SetBool(TriggerActionEnabledKeyPrefix + actionPrefKey, isEnabled);
  }
  public static string GetTriggerActionTemplateKey(string actionTemplateKey)
  {
    return TriggerActionTemplateKeyPrefix + actionTemplateKey;
  }
  public static void SetTriggerActionTemplate(string actionTemplateKey, string template)
  {
    // Ensure actionTemplateKey is not null or empty
    if (string.IsNullOrEmpty(actionTemplateKey)) return;
    EditorPrefs.SetString(TriggerActionTemplateKeyPrefix + actionTemplateKey, template);
  }
}
