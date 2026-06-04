using System.Linq;
using BattleTech;
using BattleTech.Save;
using HarmonyLib;

namespace CustomBlockTest.Patches
{

    [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
    class SimGameState_RehydratePatch
    {
        public static void Postfix(SimGameState __instance)
        {
            CBTestCore.modLog.Info?.Write("SimGameState Rehydrated, test Data Values:");
            CBTestCore.testData.Log(CBTestCore.modLog);
        }
    }

    [HarmonyPatch(typeof(SimGameState), "Dehydrate")]
    class SimGameState_Dehydrate
    {
        public static void Prefix(ref bool __runOriginal, SimGameState __instance)
        {
            CBTestCore.modLog.Info?.Write("SimGameState Dehyrating, test Data Values:");
            CBTestCore.testData.RandomizeData();
            CBTestCore.testData.Log(CBTestCore.modLog);
        }
    }
}