using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BattleTech;
using BattleTech.Save;
using BattleTech.Save.Core;
using JwTweaks.Features;

namespace JwTweaks.Patches
{
    [HarmonyPatch(typeof(SaveBlock<GameInstanceSave>), "_ToBlockBytes")]
    public static class SaveBlockGameInstanceSave__ToBlockBytes
    {
        
        public static bool Prepare() => JTCore.settings.ImprovedSaveSeriallization;

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_1 && codes[i -1].opcode == OpCodes.Ldfld)
                {
                    codes[i].opcode = OpCodes.Ldc_I4_2;
                    break;
                }
            }

            return codes.AsEnumerable();
        }
    }
    
    [HarmonyPatch(typeof(SaveBlock<GameInstanceSave>), "GetFinalBlockLength")]
    public static class SaveBlockGameInstanceSave_GetFinalBlockLength
    {
        
        public static bool Prepare() => JTCore.settings.ImprovedSaveSeriallization;

        public static void Prefix(ref bool __runOriginal, byte[] metaDataBytes, byte[] targetBytes, ref int __result)
        {
            if (!__runOriginal)
            {
                return;
            }
        
            __runOriginal = false;

            __result = SaveSerializationManager.FinalBlockLength(2, metaDataBytes.Length, targetBytes.Length);
        }
    }
}