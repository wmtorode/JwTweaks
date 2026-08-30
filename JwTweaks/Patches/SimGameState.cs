using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Framework;
using BattleTech.Save;
using JwTweaks.Features;
using UnityEngine;

namespace JwTweaks.Patches;

[HarmonyPatch(typeof(SimGameState), "Rehydrate")]
    public static class SimGameState_Rehydrate {
        
        
        public static bool Prepare() => JTCore.settings.FixTravelContracts;
        
        static void Postfix(SimGameState __instance, GameInstanceSave gameInstanceSave) {
            try {
                
                foreach (Contract contract in __instance.GlobalContracts)
                {
                    TravelContractFixer.Instance.FixIsTravelContract(contract, __instance, false);
                    TravelContractFixer.LogContract(contract);
                }
                foreach (Contract contract in __instance.CurSystem.SystemContracts) {
                    TravelContractFixer.Instance.FixIsTravelContract(contract, __instance, false);
                    TravelContractFixer.LogContract(contract);
                }
                foreach (Contract contract in __instance.CurSystem.SystemBreadcrumbs) {
                    TravelContractFixer.Instance.FixIsTravelContract(contract, __instance, false);
                    TravelContractFixer.LogContract(contract);
                }

                if (__instance.activeBreadcrumb != null)
                {
                    JTCore.modLog.Info?.Write("Active breadcrumb found");
                    // by definition this is a travel contract, as only they can be a breadcrumb
                    TravelContractFixer.Instance.FixIsTravelContract(__instance.activeBreadcrumb, __instance, true);
                    TravelContractFixer.LogContract(__instance.activeBreadcrumb);
                }

                if (__instance.pendingBreadcrumb != null)
                {
                    JTCore.modLog.Info?.Write("Pending breadcrumb found");
                    TravelContractFixer.LogContract(__instance.pendingBreadcrumb);
                }

            }
            catch (Exception e) {
                JTCore.modLog.Error?.Write(e);
            }
        }
    }
    
    [HarmonyPatch(typeof(SimGameState), "GetFilteredComparisons")]
    public static class SimGameState_GetFilteredComparisons {
        
        
        public static bool Prepare() => JTCore.settings.EnhancedContractRequirements;
        
        static void Prefix(ref bool __runOriginal, SimGameState __instance, IEnumerable<ComparisonDef> comparisons,
            Func<ComparisonDef, bool> ComparisonFunc, ref SimGameState.FilteredComparisonResults __result) {

            if (!__runOriginal)
            {
                return;
            }

            __runOriginal = false;
            __result = ContractRequirementsEnhancer.Instance.GetFilteredComparisons(comparisons, ComparisonFunc, __instance);

        }
    }
    
    [HarmonyPatch(typeof(SimGameState), "GetPotentialFactions")]
    public static class SimGameState_GetPotentialFactions {
        
        
        public static bool Prepare() => JTCore.settings.EnhancedContractRequirements;
        
        static void Prefix(ref bool __runOriginal, SimGameState __instance, StarSystem system,
            IEnumerable<string> sourceFactions,
            IEnumerable<ComparisonDef> comparisons,
            ref List<string> __result) {

            if (!__runOriginal)
            {
                return;
            }

            __runOriginal = false;
            __result = ContractRequirementsEnhancer.Instance.GetPotentialFactions(system, sourceFactions.ToList(), 
                comparisons.ToList(), __instance);
        }
    }