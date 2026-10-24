using BattleTech.Save.Test;
using JwTweaks.Features;

namespace JwTweaks.Patches;

[HarmonyPatch(typeof(SerializableReferenceContainer.SerializedReference), "Size")]
public static class SerializedReference_Size
{
        
    public static bool Prepare() => JTCore.settings.ImprovedSaveSerialization || JTCore.settings.CustomSaveBlocks;

    public static void Prefix(ref bool __runOriginal, SerializableReferenceContainer.SerializedReference __instance, ref int __result)
    {
        if (!__runOriginal)
        {
            return;
        }
        
        __runOriginal = false;

        __result = SaveSerializationManager.StorageSpaceImprovedByteArray(__instance.serializedData.Length) + 
                   HBS.Util.Serialization.StorageSpaceString(__instance.serializedType);
    }
}