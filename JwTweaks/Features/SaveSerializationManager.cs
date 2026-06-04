using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HBS.Util;
using Ionic.Zlib;
using JwTweaks.Data;
using Newtonsoft.Json;

namespace JwTweaks.Features;

public class SaveSerializationManager
{
    
    private static SaveSerializationManager _instance;
    
    private static Dictionary<String, ICustomSave> _customSaveBlocks = new Dictionary<String, ICustomSave>();
    private static Dictionary<String, String> _missingBlockData = new Dictionary<String, String>();
    

    public static SaveSerializationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SaveSerializationManager();
            }
            return _instance;
        }
    }

    public static int SaveVersion()
    {
        if (JTCore.settings.CustomSaveBlocks)
        {
            return 3;
        }
        if (JTCore.settings.ImprovedSaveSerialization)
        {
            return 2;
        }
        return 1;
    }

    public static void RegisterCustomSaveBlock(ICustomSave customSave, string blockName = null)
    {
        var name = blockName ?? customSave.BlockName;
        
        if (_customSaveBlocks.ContainsKey(name))
        {
            throw new Exception($"Custom save block {name} already registered");
        }
        _customSaveBlocks.Add(name, customSave);
    }

    public static bool UnregisterCustomSaveBlock(string blockName)
    {
        return _customSaveBlocks.Remove(blockName);
    }

    internal static byte[] WriteCustomSaveBlocks()
    {
        var saveData = new Dictionary<String, String>();
        foreach (var customSave in _customSaveBlocks)
        {
            saveData.Add(customSave.Key, customSave.Value.SaveData());
        }

        foreach (var block in _missingBlockData)
        {
            if (!saveData.ContainsKey(block.Key))
            {
                saveData.Add(block.Key, block.Value);
            }
        }

        var serialized = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(saveData, Formatting.None));
        using (MemoryStream ms = new MemoryStream())
        {
            using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress, JTCore.settings.SaveBlockSettings.CompressionLevel))
            {
                gzip.Write(serialized, 0, serialized.Length);
            }
            serialized = ms.ToArray();
        }
        return serialized;
    }

    internal static void LoadCustomSaveBlocks(byte[] serialized)
    {
        String uncompressed;
        using (MemoryStream ms = new MemoryStream(serialized))
            {
                using (GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        gzip.CopyTo(output);
                        uncompressed = Encoding.UTF8.GetString(output.ToArray());
                    }
                }
            }
        var saveData = JsonConvert.DeserializeObject<Dictionary<String, String>>(uncompressed);
        var throwOnMissing = JTCore.settings.SaveBlockSettings.throwOnMissingSaveBlock;
        var persistMissing = JTCore.settings.SaveBlockSettings.persistMissingSaveBlocks;
        foreach (var block in saveData)
        {
            if (_customSaveBlocks.ContainsKey(block.Key))
            {
                _customSaveBlocks[block.Key].LoadData(block.Value);
            }
            else if (persistMissing)
            {
                _missingBlockData.Add(block.Key, block.Value);
                JTCore.modLog.Warn?.Write($"Missing save block {block.Key}");
            }
            else if (throwOnMissing)
            {
                throw new Exception($"Missing save block {block.Key}");
            }
        }
    }

    public static int StorageSpaceImprovedByteArray(int arrayLength)
    {
        return 1 + Serialization.STORAGE_SPACE_INT + arrayLength;
    }

    public static int FinalBlockLength(int saveVersion, int saveMetaDataLength, int saveDataLength, int customSaveBlockLength)
    {
        switch (saveVersion)
        {
            case 3:
                return 4 + Serialization.STORAGE_SPACE_INT + (4 * StorageSpaceImprovedByteArray(16)) + StorageSpaceImprovedByteArray(saveMetaDataLength) + Serialization.STORAGE_SPACE_INT + StorageSpaceImprovedByteArray(saveDataLength) + StorageSpaceImprovedByteArray(customSaveBlockLength);
            case 2:
                return 4 + Serialization.STORAGE_SPACE_INT + (3 * StorageSpaceImprovedByteArray(16)) + StorageSpaceImprovedByteArray(saveMetaDataLength) + Serialization.STORAGE_SPACE_INT + StorageSpaceImprovedByteArray(saveDataLength);
            default:
                return 4 + Serialization.STORAGE_SPACE_INT + (3 * Serialization.StorageSpaceByteArray(16)) + Serialization.StorageSpaceByteArray(saveMetaDataLength) + Serialization.STORAGE_SPACE_INT + Serialization.StorageSpaceByteArray(saveDataLength);
        }
    }

    public static int SaveMetaDataReadOffset(int saveVersion)
    {
        switch (saveVersion)
        {
            case 3:
            case 2:
                return 4 + Serialization.STORAGE_SPACE_INT + (3 * StorageSpaceImprovedByteArray(16)) + StorageSpaceImprovedByteArray(0);
            default:
                return 4 + Serialization.STORAGE_SPACE_INT + (3 * Serialization.StorageSpaceByteArray(16)) + Serialization.StorageSpaceByteArray(0);
        }
    }
    
    
    
}