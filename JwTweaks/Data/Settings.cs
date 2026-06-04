using Ionic.Zlib;

namespace JwTweaks.Data;

public class Settings
{
    public bool Debug = false;
    public bool ShowTooltips = false;
    public bool ImprovedSaveSerialization = false;
    public bool FixTravelContracts = false;
    public bool CustomSaveBlocks = false;
    public SaveBlockSettings SaveBlockSettings = new SaveBlockSettings();
}