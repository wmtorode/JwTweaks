using System;
using System.Collections.Generic;
using BattleTech.UI.Tooltips;

namespace JwTweaks.Patches;

[HarmonyPatch(typeof(TooltipManager), "SetActiveTooltip")]
public static class TooltipManager_SetActiveTooltip
{
    private static List<string> components = new List<string>()
    {
        "UpgradeDef",
        "HeatSinkDef",
        "JumpJetDef",
        "AmmunitionBoxDef"
    };

    static void Prefix(ref bool __runOriginal, TooltipManager __instance, object data, string prefabName)
    {
        if (!__runOriginal)
        {
            return;
        }
            
        try
        {
            string name = data.GetType().Name;
            if (components.Contains(name))
            {
                name = "MechComponentDef";
            }
            foreach (TooltipManager.TooltipObject tooltipObject in __instance.TooltipPool)
            {
                if (tooltipObject.dataType.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    __instance.activeTooltip = tooltipObject.script;
                    __instance.activeTooltip.SetData(data);
                    return;
                }
            }
            JTCore.modLog.Info?.Write($"Tooltip Data of type: {name} was unable to be set");
        }
        catch (Exception e)
        {
            JTCore.modLog.Error?.Write(e);
        }
    }
}