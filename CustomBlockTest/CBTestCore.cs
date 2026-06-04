using System.Reflection;
using CustomBlockTest.Data;
using HarmonyLib;
using IRBTModUtils.Logging;
using JwTweaks.Data;
using JwTweaks.Features;

namespace CustomBlockTest
{
    public class CBTestCore
    {
        
        internal static DeferringLogger modLog;
        internal static string modDir;
        internal static TestData testData = new TestData();

        public static void Init(string modDirectory, string settingsJSON)
        {
            
            modDir = modDirectory;
            
            modLog = new DeferringLogger(modDirectory, "CBTest", "CBT", true);
        
            modLog.Info?.Write("CBTest starting");

            JsonSaveBlock<TestData> testDataSaveBlock = new JsonSaveBlock<TestData>();
            testDataSaveBlock.Data = testData;
            SaveSerializationManager.RegisterCustomSaveBlock(testDataSaveBlock, "TestDataBlock");
            
            testData.RandomizeData();
            
            modLog.Info?.Write("Randomized Test Data:");
            testData.Log(modLog);
            
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "ca.jwolf.CBTest");

        }
        
        
    }
}