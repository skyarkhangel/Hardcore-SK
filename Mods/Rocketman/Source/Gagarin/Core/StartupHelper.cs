using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using RocketMan;
using Verse;

namespace Gagarin
{
    public static class StartupHelper
    {
        [Main.OnInitialization]
        public static void StartUpStarted()
        {
            Context.RunningMods = LoadedModManager.RunningMods.ToList();
            Context.Core = LoadedModManager.RunningMods.First(m => m.IsCoreMod);

            if (!Directory.Exists(GagarinEnvironmentInfo.CacheFolderPath))
            {
                Directory.CreateDirectory(GagarinEnvironmentInfo.CacheFolderPath);
            }
            if (!Directory.Exists(GagarinEnvironmentInfo.TexturesFolderPath))
            {
                Directory.CreateDirectory(GagarinEnvironmentInfo.TexturesFolderPath);
            }
            Log.Message("GAGARIN: <color=green>StartUpStarted called!</color>");
            if (GagarinEnvironmentInfo.CacheExists)
            {
                Log.Warning("GAGARIN: <color=green>Cache found</color>");

                Context.IsUsingCache = true;

                if (GagarinEnvironmentInfo.ModListChanged)
                {
                    Context.IsUsingCache = false;

                    Log.Warning("GAGARIN: Mod list changed! Deleting cache");
                }
            }
            if (!Context.IsUsingCache)
            {
                Log.Warning("GAGARIN: <color=green>Cache not found or got purged!</color>");
            }
            Log.Message("GAGARIN: <color=green>Loading cache settings!</color>");
            RunningModsSetUtility.Dump(Context.RunningMods, GagarinEnvironmentInfo.ModListFilePath);

            GagarinSettings.LoadSettings();
            if (DateTime.Now.Subtract(GagarinPrefs.CacheCreationTime).Days >= 3)
            {
                Context.IsUsingCache = false;
            }
            if (GagarinPrefs.Enabled)
            {
                GagarinPatcher.PatchAll();
            }
            else
            {
                Log.Message("GAGARIN: <color=red>Gagarin is disabled!</color>");
            }
        }

        private static Assembly ResolveHandler(object sender, ResolveEventArgs e)
        {
            Log.Error($"ROCKETMAN: Trying to resolve {e.Name}");

            Logger.Debug($"ROCKETMAN: Trying to resolve {e.Name}", file: "ResolveHandler.log");

            return null;
        }

        [Main.OnStaticConstructor]
        public static void StartUpFinished()
        {
            Log.Message("GAGARIN: <color=green>StartUpFinished called!</color>");

            Context.AssetsHashes.Clear();
            Context.DefsXmlAssets.Clear();
            Context.XmlAssets.Clear();
            Context.CurrentLoadingMod = null;

            CachedDefHelper.Clean();
        }
    }
}
