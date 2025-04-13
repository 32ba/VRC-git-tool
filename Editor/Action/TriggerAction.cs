using UnityEngine;
using System.Collections.Generic;
using GitTool.Editor.Action;
using UnityEditor;
using System.Linq;

public class TriggerAction
{
    public static List<TriggerActionDefinition> Actions { get; private set; }

    static TriggerAction()
    {
        LoadActions();
    }

    private static void LoadActions()
    {
        // Load all TriggerActionDefinitions from Resources/TriggerActions
        TriggerActionDefinition[] loadedActions = Resources.LoadAll<TriggerActionDefinition>("TriggerActions");

        if (loadedActions != null && loadedActions.Length > 0)
        {
            Actions = loadedActions.ToList();
        }
        else
        {
            Debug.LogWarning("Git Tool: No TriggerActionDefinitions found in Resources/TriggerActions. Using default actions.");
            // Fallback to hardcoded defaults if no ScriptableObjects are found
            Actions = new List<TriggerActionDefinition>
            {
                CreateDefaultAction("On Scene Save", "GitTool_TriggerOnSceneSave", "GitTool_SceneSaveMessageTemplate", "Auto-commit: Scene saved - {sceneName}"),
                CreateDefaultAction("On Build Complete", "GitTool_TriggerOnBuildComplete", "GitTool_BuildCompleteMessageTemplate", "Auto-commit: Build completed")
            };
        }
    }

    private static TriggerActionDefinition CreateDefaultAction(string name, string prefKey, string templateKey, string defaultTemplate)
    {
        TriggerActionDefinition action = ScriptableObject.CreateInstance<TriggerActionDefinition>();
        action.ActionName = name;
        action.PreferenceKey = prefKey;
        action.TemplatePreferenceKey = templateKey;
        action.DefaultTemplate = defaultTemplate;
        return action;
    }

    [MenuItem("Tools/Git Tool/Reload Trigger Actions")]
    public static void ReloadActions()
    {
        LoadActions();
    }
}
