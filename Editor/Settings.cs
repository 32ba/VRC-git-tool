using UnityEditor;
using UnityEngine;

public static class GitToolSettings
{
  // PlayerPrefs Keys
  private const string GitRepositoryPathKey = "GitTool_RepositoryPath";
  private const string AutoPushEnabledKey = "GitTool_AutoPushEnabled";
  private const string RemoteNameKey = "GitTool_RemoteName";
  private const string TriggerActionEnabledKeyPrefix = "GitTool_TriggerActionEnabled_";
  private const string TriggerActionTemplateKeyPrefix = "GitTool_TriggerActionTemplate_";

  // Public static properties to access settings from other classes
  public static string RepositoryPath { get => PlayerPrefs.GetString(GitRepositoryPathKey, ""); set => PlayerPrefs.SetString(GitRepositoryPathKey, value); }
  public static bool IsAutoPushEnabled { get => PlayerPrefs.GetInt(AutoPushEnabledKey, 0) == 1; set => PlayerPrefs.SetInt(AutoPushEnabledKey, value ? 1 : 0); }
  public static string PushRemoteName { get => PlayerPrefs.GetString(RemoteNameKey, "origin"); set => PlayerPrefs.SetString(RemoteNameKey, value); }

  public static bool IsTriggerActionEnabled(string actionPrefKey)
  {
    // Ensure actionPrefKey is not null or empty before creating the full key
    if (string.IsNullOrEmpty(actionPrefKey)) return false;
    return PlayerPrefs.GetInt(TriggerActionEnabledKeyPrefix + actionPrefKey, 0) == 1;
  }

  public static string GetTriggerActionTemplate(string actionTemplateKey, string defaultTemplate)
  {
    // Ensure actionTemplateKey is not null or empty
    if (string.IsNullOrEmpty(actionTemplateKey)) return defaultTemplate;
    return PlayerPrefs.GetString(TriggerActionTemplateKeyPrefix + actionTemplateKey, defaultTemplate);
  }


  public static string GetTriggerActionEnabledKey(string actionPrefKey)
  {
    return TriggerActionEnabledKeyPrefix + actionPrefKey;
  }
  public static void SetTriggerActionEnabled(string actionPrefKey, bool isEnabled)
  {
    // Ensure actionPrefKey is not null or empty
    if (string.IsNullOrEmpty(actionPrefKey)) return;
    PlayerPrefs.SetInt(TriggerActionEnabledKeyPrefix + actionPrefKey, isEnabled ? 1 : 0);
  }
  public static string GetTriggerActionTemplateKey(string actionTemplateKey)
  {
    return TriggerActionTemplateKeyPrefix + actionTemplateKey;
  }
  public static void SetTriggerActionTemplate(string actionTemplateKey, string template)
  {
    // Ensure actionTemplateKey is not null or empty
    if (string.IsNullOrEmpty(actionTemplateKey)) return;
    PlayerPrefs.SetString(TriggerActionTemplateKeyPrefix + actionTemplateKey, template);
  }
}
