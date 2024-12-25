using HBS.Util;

namespace JwTweaks.Features;

public class SaveSerializationManager
{
    
    private static SaveSerializationManager _instance;

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

    public static int StorageSpaceImprovedByteArray(int arrayLength)
    {
        return 1 + Serialization.STORAGE_SPACE_INT + arrayLength;
    }

    public static int FinalBlockLength(int saveVersion, int saveMetaDataLength, int saveDataLength)
    {
        switch (saveVersion)
        {
            case 2:
                return 4 + Serialization.STORAGE_SPACE_INT + (3 * StorageSpaceImprovedByteArray(16)) + StorageSpaceImprovedByteArray(saveMetaDataLength) + Serialization.STORAGE_SPACE_INT + StorageSpaceImprovedByteArray(saveDataLength);;
            default:
                return 4 + Serialization.STORAGE_SPACE_INT + (3 * Serialization.StorageSpaceByteArray(16)) + Serialization.StorageSpaceByteArray(saveMetaDataLength) + Serialization.STORAGE_SPACE_INT + Serialization.StorageSpaceByteArray(saveDataLength);
        }
    }

    public static int SaveMetaDataReadOffset(int saveVersion)
    {
        switch (saveVersion)
        {
            case 2:
                return 4 + Serialization.STORAGE_SPACE_INT + (3 * StorageSpaceImprovedByteArray(16)) + StorageSpaceImprovedByteArray(0);
            default:
                return 4 + Serialization.STORAGE_SPACE_INT + (3 * Serialization.StorageSpaceByteArray(16)) + Serialization.StorageSpaceByteArray(0);
        }
    }
    
    
    
}