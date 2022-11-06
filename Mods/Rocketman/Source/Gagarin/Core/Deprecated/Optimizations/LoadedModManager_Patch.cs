using System;
using System.Xml;
using Verse;
using RocketMan;
using System.Reflection;
using HarmonyLib;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using RimWorld;

namespace Gagarin
{
    public static class LoadedModManager_Patch
    {
        public static bool cacheExists = false;

        public static bool cacheUsed = false;

        public static string cachePath = Path.Combine(GenFilePaths.ConfigFolderPath, "Cache");

        public static string cachedModsListPath = Path.Combine(GenFilePaths.ConfigFolderPath, "Cache/mods.xml");

        public static string cachedUnifiedXmlPath = Path.Combine(GenFilePaths.ConfigFolderPath, "Cache/unified.xml");

        public static Dictionary<string, LoadableXmlAsset> loadablelookup = new Dictionary<string, LoadableXmlAsset>();

        public static Dictionary<XmlNode, string> packageIdlookup = new Dictionary<XmlNode, string>();

        public static GagarinPatchInfo[] patches = new GagarinPatchInfo[] {
            new GagarinPatchInfo(typeof(LoadModXML_Patch)),
            new GagarinPatchInfo(typeof(ApplyPatches_Patch)),
            new GagarinPatchInfo(typeof(ShortHashGiver_GiveAllShortHashes_Patch)),
        };

        public static Stopwatch stopwatch = new Stopwatch();

        [Main.OnInitialization]
        public static void Start()
        {
            stopwatch.Start();
            int loadIndex = 0;
            HashSet<string> currentMods = new HashSet<string>();
            foreach (ModContentPack modContent in LoadedModManager.RunningMods)
            {
                if (Context.core == null && modContent.IsCoreMod)
                    Context.core = modContent;

                currentMods.Add(modContent.PackageId);
                Context.runningMods.Add(modContent);
                Context.packageIdLoadIndexlookup[modContent.PackageId] = loadIndex++;
            }
            LoadedModManager_Patch.PatchAll();
            // TODO reactivate this
            cacheExists = true;
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
                Log.Message($"GAGARIN: Created cache folder at {cachedUnifiedXmlPath}");
            }
            if (!File.Exists(cachedUnifiedXmlPath))
            {
                Log.Warning($"GAGARIN: Cache not found starting the caching process!");
                cacheExists = false;
            }
            if (RunningModsSetUtility.Changed(currentMods, cachedModsListPath))
            {
                Log.Warning("GAGARIN: Mod list changed!");
                if (File.Exists(cachedUnifiedXmlPath))
                    File.Delete(cachedUnifiedXmlPath);
                cacheExists = false;
            }
            RunningModsSetUtility.Dump(Context.runningMods, cachedModsListPath);
            Log.Warning($"GAGARIN: Cache <color=green>FOUND</color>!");
        }

        public static void PatchAll()
        {
            for (int i = 0; i < patches.Length; i++)
            {
                try { patches[i].Patch(GagarinPatcher.harmony); }
                catch (Exception er)
                {
                    Log.Error($"GAGARIN: LoadedLanguage_Patch PATCHING FAILED! {patches[i].DeclaringType}:{er}");
                    break;
                }
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.LoadModXML))]
        public static class LoadModXML_Patch
        {
            public static void Postfix(List<LoadableXmlAsset> __result)
            {
                foreach (LoadableXmlAsset loadable in __result)
                {
                    string packageId = loadable.mod.PackageId;
                    if (Context.assetPackageIdlookup.TryGetValue(packageId, out List<LoadableXmlAsset> assests))
                    {
                        assests.Add(loadable);
                    }
                    else
                    {
                        assests = new List<LoadableXmlAsset>();
                        assests.Add(loadable);
                        Context.assetPackageIdlookup[packageId] = assests;
                    }
                    loadablelookup[loadable.GetLoadableId()] = loadable;
                    foreach (XmlNode node in loadable.xmlDoc.DocumentElement.ChildNodes)
                        packageIdlookup[node] = packageId;
                }
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ApplyPatches))]
        public static class ApplyPatches_Patch
        {
            public static Stopwatch stopwatch = new Stopwatch();

            public static bool Prefix(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
            {
                stopwatch.Start();
                return !cacheExists;
            }

            public static void Postfix(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
            {
                stopwatch.Stop();
                Log.Message($"GAGARIN: <color=green>LoadedModManager.ApplyPatches</color> took " +
                    $"<color=orange>{Math.Round((float)stopwatch.ElapsedTicks / Stopwatch.Frequency, 2)} seconds</color>");
                stopwatch.Restart();
                if (!cacheExists)
                {
                    try
                    {
                        LoadableXmlAssetUtility.Dump(assetlookup, xmlDoc, cachedUnifiedXmlPath);
                        Log.Message($"GAGARIN: Created patched cache at {cachedUnifiedXmlPath}");
                    }
                    catch (Exception er)
                    {
                        Log.Error($"GAGARIN: creating cache failed {er}");
                        cacheUsed = false;
                    }
                }
                else if (cacheExists)
                {
                    try
                    {
                        LoadableXmlAssetUtility.Load(loadablelookup, assetlookup, xmlDoc, cachedUnifiedXmlPath);
                        TKeySystem.Clear();
                        TKeySystem.Parse(xmlDoc);
                        Log.Message($"GAGARIN: Loaded xml assests from cache");
                        cacheUsed = true;
                        foreach (ModContentPack runningMod in LoadedModManager.runningMods)
                        {
                            foreach (PatchOperation patch in runningMod.Patches)
                                patch.neverSucceeded = false;
                        }
                    }
                    catch (Exception er)
                    {
                        Log.Error($"GAGARIN: loading cache failed {er}");
                        cacheUsed = false;
                    }
                }
                Log.Message($"GAGARIN: cache operations took " +
                    $"<color=orange>{Math.Round((float)stopwatch.ElapsedTicks / Stopwatch.Frequency, 2)} seconds</color>");
                stopwatch.Stop();
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ClearCachedPatches))]
        public static class ClearCachedPatches_Patch
        {
            public static bool Prefix()
            {
                return !cacheUsed;
            }
        }

        [GagarinPatch(typeof(ShortHashGiver), nameof(ShortHashGiver.GiveAllShortHashes))]
        public static class ShortHashGiver_GiveAllShortHashes_Patch
        {
            private static bool initialized = false;

            public static void Postfix()
            {
                if (initialized)
                    return;
                initialized = true;
                stopwatch.Stop();
                Log.Message($"GAGARIN: Loading took <color=red>more</color> than " +
                     $"<color=orange>{Math.Round((float)stopwatch.ElapsedTicks / Stopwatch.Frequency, 2)} seconds</color>");
            }
        }
    }
}
