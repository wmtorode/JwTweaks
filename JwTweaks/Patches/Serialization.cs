using System;
using BattleTech.Save;
using HBS.Util;
using JwTweaks.Features;

namespace JwTweaks.Patches;


[HarmonyPatch(typeof(Serialization), "PutByteArray")]
public static class Serialization_PutByteArray
{
    
    public static bool Prepare() => JTCore.settings.ImprovedSaveSerialization;
    
    public static void Prefix(ref bool __runOriginal, byte[] value, byte[] bytes, ref int offset, int size)
    {
        if (!__runOriginal)
        {
            return;
        }
        
        __runOriginal = false;
        
        if (size == 0)
            size = bytes.Length - offset;
        if (SaveSerializationManager.StorageSpaceImprovedByteArray(value.Length) > size)
            throw new SerializationException("Not enough space" + (object) size);
        bytes[offset] = (byte) 100;
        ++offset;
        Serialization.PutInt(value.Length, bytes, ref offset);
        Array.Copy(value, 0, bytes, offset, value.Length);
        offset += value.Length;
    }
}

[HarmonyPatch(typeof(Serialization), "GetByteArray")]
public static class Serialization_GetByteArray
{
    
    public static bool Prepare() => JTCore.settings.ImprovedSaveSerialization;
    
    public static void Prefix(ref bool __runOriginal, byte[] bytes, ref int offset, int size, ref byte[] __result)
    {
        if (!__runOriginal)
        {
            return;
        }
        
        __runOriginal = false;
        var originalSize = size;
        var originalOffset = offset;
        if (size == 0)
            size = bytes.Length - offset;
        if (Serialization.STORAGE_SPACE_INT + 1 > size)
            throw new SerializationException("Not enough space");
        bool improvedSerializer = bytes[offset] == 100;
        var dataType = bytes[offset];
        if (dataType != 14 && dataType != 100)
            throw new SerializationException($"Unexpected type: {dataType}");
        ++offset;
        int length = Serialization.GetInt(bytes, ref offset);
        if (length < 0)
            throw new SerializationException("invalid byte array length");
        if (Serialization.STORAGE_SPACE_INT + length > size)
            throw new SerializationException("Not enough space");
        __result = new byte[length];
        if (length == 0)
            return ;
        lock (Serialization.lck)
        {
            if (improvedSerializer)
            {
                Array.Copy(bytes, offset, __result, 0, length);
                offset += length;
            }
            else
            {
                for (int index = 0; index < length; ++index)
                {
                    byte num = Serialization.GetByte(bytes, ref offset);
                    __result[index] = num;
                }
            }
        }
    }
}

[HarmonyPatch(typeof(Serialization), "PeekByteArrayLength")]
public static class Serialization_PeekByteArrayLength
{
    
    public static bool Prepare() => JTCore.settings.ImprovedSaveSerialization;
    
    public static void Prefix(ref bool __runOriginal, byte[] bytes, ref int offset, int size, ref int __result)
    {
        if (!__runOriginal)
        {
            return;
        }
        
        __runOriginal = false;
        
        if (size == 0)
            size = bytes.Length - offset;
        if (Serialization.STORAGE_SPACE_INT + 1 > size)
            throw new SerializationException("Not enough space");
        var dataType = bytes[offset];
        if (dataType != 14 && dataType != 100)
            throw new SerializationException($"Unexpected type: {dataType}");
        ++offset;
        int num = Serialization.GetInt(bytes, ref offset);
        __result = num >= 0 ? num : throw new SerializationException("invalid byte array length");
    }
}
