using System;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;

using IRBTModUtils.Logging;

namespace JwTweaks;

public class JTCore
{
    internal static DeferringLogger modLog;
    internal static string modDir;

    public static void Init(string modDirectory, string settingsJSON)
    {
            
        modDir = modDirectory;
        
        modLog = new DeferringLogger(modDirectory, "JwTweaks", "JTC");
        
        modLog.Info?.Write("JwTweaks starting");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "ca.jwolf.JwTweaks");

    }
}