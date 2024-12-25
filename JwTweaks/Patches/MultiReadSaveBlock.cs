using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BattleTech;
using BattleTech.Save;
using BattleTech.Save.Core;
using JwTweaks.Features;


namespace JwTweaks.Patches;

[HarmonyPatch(typeof(MultiReadSaveBlock<GameInstanceSave>), "get_MetaDataArraySizeReadAmount")]
public static class MultiReadSaveBlockGameInstanceSave_MetaDataArraySizeReadAmount
{
        
    public static bool Prepare() => JTCore.settings.ImprovedSaveSeriallization;

    public static void Prefix(ref bool __runOriginal, MultiReadSaveBlock<GameInstanceSave> __instance,  ref int __result)
    {
        if (!__runOriginal)
        {
            return;
        }
        
        __runOriginal = false;

        __result = SaveSerializationManager.SaveMetaDataReadOffset(__instance.version);
    }
}

[HarmonyPatch(typeof(MultiReadSaveBlock<GameInstanceSave>), "Reset")]
public static class MultiReadSaveBlockGameInstanceSave_Reset
{
        
    public static bool Prepare() => JTCore.settings.ImprovedSaveSeriallization;

    public static void Prefix(ref bool __runOriginal, MultiReadSaveBlock<GameInstanceSave> __instance)
    {
        if (!__runOriginal)
        {
            return;
        }

        __instance.version = -1;
    }
    
}