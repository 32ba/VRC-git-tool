using UnityEngine;

namespace GitTool.Editor.Action
{
  public class TriggerActionDefinition
  {
    public string ActionId; // Unique identifier for the action, can be used for serialization or reference
    public string ActionName; // Display name for the action, shown in the UI
    public string PreferenceKey; // Key used to store the enabled state in PlayerPrefs
    public string TemplatePreferenceKey; // Key for the commit message template in PlayerPrefs
    public string DefaultTemplate; // Default template for commit messages

    // Helper methods to get the full PlayerPrefs keys (optional but convenient)
    public string GetEnabledPlayerPrefsKey() => PreferenceKey;
    public string GetTemplatePlayerPrefsKey() => TemplatePreferenceKey;
  }
}
