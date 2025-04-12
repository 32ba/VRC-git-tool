using UnityEngine;
using System.Collections.Generic;

public class TriggerAction
{
  public string Name { get; set; }
  public string PrefKey { get; set; }
  public string OverrideTemplateKey { get; set; }
  public string TemplateKey { get; set; }
  public string DefaultTemplate { get; set; }

  public static readonly List<TriggerAction> Actions = new List<TriggerAction>
    {
        new TriggerAction
        {
            Name = "On Scene Save",
            PrefKey = "GitTool_TriggerOnSceneSave",
            OverrideTemplateKey = "GitTool_OverrideSceneSaveTemplate",
            TemplateKey = "GitTool_SceneSaveMessageTemplate",
            DefaultTemplate = "Auto-commit: {action}"
        },
        new TriggerAction
        {
            Name = "On Build Complete",
            PrefKey = "GitTool_TriggerOnBuildComplete",
            OverrideTemplateKey = "GitTool_OverrideBuildCompleteTemplate",
            TemplateKey = "GitTool_BuildCompleteMessageTemplate",
            DefaultTemplate = "Auto-commit: {action}"
        }
    };
}
