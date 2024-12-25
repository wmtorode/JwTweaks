using System;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;

using IRBTModUtils.Logging;
using JwTweaks.Data;

namespace JwTweaks;

public class JTCore
{
    internal static DeferringLogger modLog;
    internal static string modDir;
    internal static Settings settings;

    public static void Init(string modDirectory, string settingsJSON)
    {
            
        modDir = modDirectory;
        
        // if this fails, we are in a bad state so just fail and let modtek worry about it
        settings = JsonConvert.DeserializeObject<Settings>(settingsJSON);
        
        modLog = new DeferringLogger(modDirectory, "JwTweaks", "JTC", settings.Debug);
        
        modLog.Info?.Write("JwTweaks starting");
        

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "ca.jwolf.JwTweaks");

    }
}