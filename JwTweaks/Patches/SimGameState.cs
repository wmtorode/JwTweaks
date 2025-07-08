using System;
using System.Collections.Generic;
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