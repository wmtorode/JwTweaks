using Ionic.Zlib;

namespace JwTweaks.Data;

public class SaveBlockSettings
{
    public CompressionLevel CompressionLevel = CompressionLevel.BestSpeed;
    public bool throwOnMissingSaveBlock = true;
    public bool persistMissingSaveBlocks = false;
}