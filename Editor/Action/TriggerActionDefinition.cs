using UnityEngine;

namespace GitTool.Editor.Action
{
  public class TriggerActionDefinition : ScriptableObject
  {
    [Tooltip("The display name of the action shown in the settings UI.")]
    public string ActionName = "New Action";

    [Tooltip("The key used to store the enabled state in EditorPrefs. Should be unique.")]
    public string PreferenceKey = "GitTool_TriggerActionEnabled_NewAction";

    [Tooltip("The key used to store the commit message template in EditorPrefs. Should be unique.")]
    public string TemplatePreferenceKey = "GitTool_TriggerActionTemplate_NewAction";

    [Tooltip("The default commit message template used if none is set.")]
    [TextArea]
    public string DefaultTemplate = "Auto-commit: {action}";

    // Helper methods to get the full EditorPrefs keys (optional but convenient)
    public string GetEnabledEditorPrefsKey() => PreferenceKey;
    public string GetTemplateEditorPrefsKey() => TemplatePreferenceKey;
  }
}
