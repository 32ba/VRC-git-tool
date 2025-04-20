using UnityEngine;
using System.Collections.Generic;
using GitTool.Editor.Action;
using UnityEditor;
using System.Linq;

public class TriggerAction
{
    public static List<TriggerActionDefinition> Actions = new()
    {
        new TriggerActionDefinition
        {
            ActionId = "on_scene_save",
            ActionName = "On Scene Save",
            PreferenceKey = "GitTool_TriggerOnSceneSave",
            TemplatePreferenceKey = "GitTool_SceneSaveMessageTemplate",
            DefaultTemplate = "Auto-commit: Scene saved - {sceneName}"
        },
        new TriggerActionDefinition
        {
            ActionId = "on_build_complete",
            ActionName = "On Build Complete",
            PreferenceKey = "GitTool_TriggerOnBuildComplete",
            TemplatePreferenceKey = "GitTool_BuildCompleteMessageTemplate",
            DefaultTemplate = "Auto-commit: Build completed at {time}"
        },
    };
}
