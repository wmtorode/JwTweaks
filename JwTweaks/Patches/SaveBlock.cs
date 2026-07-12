using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BattleTech;
using BattleTech.Save;
using BattleTech.Save.Core;
using BattleTech.Serialization;
using HBS.Util;
using JwTweaks.Features;

namespace JwTweaks.Patches
{
    [HarmonyPatch(typeof(SaveBlock<GameInstanceSave>), "_ToBlockBytes")]
    [HarmonyPriority(Priority.First)]
    public static class SaveBlockGameInstanceSave__ToBlockBytes
    {
        
        public static bool Prepare() => JTCore.settings.ImprovedSaveSerialization || JTCore.settings.CustomSaveBlocks;

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
        
        
        public static void Prefix(SaveBlock<GameInstanceSave> __instance, ref bool __runOriginal, byte[] metaDataBytes, byte[] targetBytes, bool needsCompression, ref byte[] __result)
        {
            if (!__runOriginal)
            {
                return;
            }
        
            __runOriginal = false;

            if (needsCompression)
            {
                metaDataBytes = __instance.CompressBytes(metaDataBytes);
                targetBytes = __instance.CompressBytes(targetBytes);
            }

            byte[] customBlock = Array.Empty<byte>();
            if (JTCore.settings.CustomSaveBlocks)
            {
                customBlock = SaveSerializationManager.WriteCustomSaveBlocks();
            }
            var saveVersion = SaveSerializationManager.SaveVersion();
            
            byte[] md5Hash1 = CryptoUtil.GetMD5Hash(__instance.GetTypeString());
            byte[] md5Hash2 = CryptoUtil.GetMD5Hash(metaDataBytes);
            byte[] md5Hash3 = CryptoUtil.GetMD5Hash(targetBytes);
            int finalBlockLength = SaveSerializationManager.FinalBlockLength(saveVersion, metaDataBytes.Length, targetBytes.Length, customBlock.Length);
            byte[] bytes = new byte[finalBlockLength];
            bytes[0] = (byte) 98;
            bytes[1] = (byte) 115;
            bytes[2] = (byte) 116;
            bytes[3] = (byte) 98;
            try
            {
                __instance.serializationStream.SetBuffer(bytes, 4, finalBlockLength - 4);
                __instance.serializationStream.PutInt(saveVersion);
                __instance.serializationStream.PutByteArray(md5Hash1);
                __instance.serializationStream.PutByteArray(md5Hash2);
                __instance.serializationStream.PutByteArray(md5Hash3);
                __instance.serializationStream.PutByteArray(metaDataBytes);
                __instance.serializationStream.PutInt((int) __instance.target);
                __instance.serializationStream.PutByteArray(targetBytes);
                if (JTCore.settings.CustomSaveBlocks)
                {
                    byte[] customblockHash = CryptoUtil.GetMD5Hash(customBlock);
                    __instance.serializationStream.PutByteArray(customblockHash);
                    __instance.serializationStream.PutByteArray(customBlock);
                }
            }
            catch (SerializationException ex)
            {
                throw new SaveBlockException(SaveBlockError.INTERNAL_ERROR, "Failed writing data to SerializationStream.", ex);
            }
            __result = bytes;
        }
        
    }
    
    [HarmonyPatch(typeof(SaveBlock<GameInstanceSave>), "GetFinalBlockLength")]
    public static class SaveBlockGameInstanceSave_GetFinalBlockLength
    {
        
        public static bool Prepare() => JTCore.settings.ImprovedSaveSerialization && !JTCore.settings.CustomSaveBlocks;

        public static void Prefix(ref bool __runOriginal, byte[] metaDataBytes, byte[] targetBytes, ref int __result)
        {
            if (!__runOriginal)
            {
                return;
            }
        
            __runOriginal = false;

            __result = SaveSerializationManager.FinalBlockLength(2, metaDataBytes.Length, targetBytes.Length, 0);
        }
    }
    
    [HarmonyPatch(typeof(SaveBlock<GameInstanceSave>), "GetSaveData")]
    public static class SaveBlockGameInstanceSave_GetSaveData
    {
        public static bool Prepare() => JTCore.settings.CustomSaveBlocks;
        
        public static void Prefix(SaveBlock<GameInstanceSave> __instance, ref bool __runOriginal, byte[] blockBytes, ref GameInstanceSave __result)
        {
            if (!__runOriginal)
            {
                return;
            }
        
            __runOriginal = false;
            
            GameInstanceSave obj = default (GameInstanceSave);
            try
            {
                __instance.ProcessFileSignature(blockBytes);
                __instance.ProcessVersion(blockBytes);
                __instance.ProcessType(blockBytes);
                byte[] metaDataHash = __instance.serializationStream.GetByteArray();
                byte[] gameDataHash = __instance.serializationStream.GetByteArray();
                byte[] metaData = __instance.serializationStream.GetByteArray();
                byte[] md5Hash1 = CryptoUtil.GetMD5Hash(metaData);
                if (!__instance.CompareHash(metaDataHash, md5Hash1))
                    throw new SaveBlockException(SaveBlockError.CORRUPT_FILE, "Invalid MetaData hash.");
                GameInstanceSave existingObject = __instance.Deserialize(metaData, SerializationTarget.MetaData, TargetMaskOperation.HAS_ANY, decompress: true);
                
                if (__instance.target == SerializationTarget.MetaData)
                {
                    __result = existingObject;
                    return;
                }

                int target1 = (int) __instance.target;
                int num = __instance.serializationStream.GetInt();
                if ((num & target1) != target1)
                    throw new SaveBlockException(SaveBlockError.NO_DATA_FOR_TARGET, string.Format("Block was serialized with target {0}. Tried deserializing it with target {1}", (object) num, (object) __instance.target));
                byte[] gameData = __instance.serializationStream.GetByteArray();
                byte[] md5Hash2 = CryptoUtil.GetMD5Hash(gameData);
                if (!__instance.CompareHash(gameDataHash, md5Hash2))
                    throw new SaveBlockException(SaveBlockError.CORRUPT_FILE, "Bad target hash.");
                SerializationTarget target2 = ~SerializationTarget.MetaData & __instance.target;

                // guard if old save format was being loaded
                if (__instance.version == 3)
                {
                    byte[] customBlockHash = __instance.serializationStream.GetByteArray();
                    byte[] customBlockData = __instance.serializationStream.GetByteArray();
                    byte[] md5Hash3 = CryptoUtil.GetMD5Hash(customBlockData);
                    if (!__instance.CompareHash(customBlockHash, md5Hash3))
                        throw new SaveBlockException(SaveBlockError.CORRUPT_FILE, "Bad custom block hash.");
                    SaveSerializationManager.LoadCustomSaveBlocks(customBlockData);
                }

                __result = __instance.Deserialize(gameData, target2, TargetMaskOperation.HAS_ANY, existingObject, true);
                
                
                
                
                return;
            }
            catch (SerializationException ex)
            {
                throw new SaveBlockException(SaveBlockError.CORRUPT_FILE, "GetBlock failed reading from SerializationStream.", (Exception) ex);
            }
        }
    }
}