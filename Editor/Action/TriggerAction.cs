using UnityEngine;
using System.Collections.Generic;
using GitTool.Editor.Action;
using UnityEditor;
using System.Linq;

public class TriggerAction
{
    public static List<TriggerActionDefinition> Actions = new()
    {
        CreateDefaultAction("On Scene Save", "GitTool_TriggerOnSceneSave", "GitTool_SceneSaveMessageTemplate", "Auto-commit: Scene saved - {sceneName}"),
        CreateDefaultAction("On Build Complete", "GitTool_TriggerOnBuildComplete", "GitTool_BuildCompleteMessageTemplate", "Auto-commit: Build completed")
    };

    private static TriggerActionDefinition CreateDefaultAction(string name, string prefKey, string templateKey, string defaultTemplate)
    {
        TriggerActionDefinition action = ScriptableObject.CreateInstance<TriggerActionDefinition>();
        action.ActionName = name;
        action.PreferenceKey = prefKey;
        action.TemplatePreferenceKey = templateKey;
        action.DefaultTemplate = defaultTemplate;
        return action;
    }
}
