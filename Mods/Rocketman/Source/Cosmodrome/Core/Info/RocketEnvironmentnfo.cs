using System;
using System.IO;
using HarmonyLib;
using Verse;

namespace RocketMan
{
    public static class RocketEnvironmentInfo
    {
        // private static bool isDevEnvInitialized = false;
        // private static bool isDevEnv = false;

        private const string LogFolderName = "Logs";

        private const string DependenciesSubPath = "1.4/Plugins/Dependencies";

        private const string PluginFolderSubPath = "1.4/Plugins/Stable";

        private const string ExperimentalPluginFolderSubPath = "1.4/Plugins/Experimental";

        public static bool IsDevEnv
        {
            get
            {
                //if (!isDevEnvInitialized)
                //{
                //    string path = Path.GetFullPath(Path.Combine(GenFilePaths.ConfigFolderPath, "rocketeer.0102.txt"));
                //    Log.Message($"ROCKETMAN: config path {path}");
                //    isDevEnvInitialized = true;
                //    isDevEnv = File.Exists(path);
                //    if (isDevEnv)
                //    {
                //        Log.Warning($"ROCKETMAN: dev environment detected!");
                //    }
                //}
                //return isDevEnv;
                return File.Exists(DevKeyFilePath);
            }
        }

        public static string DevKeyFilePath
        {
            get => Path.GetFullPath(Path.Combine(GenFilePaths.ConfigFolderPath, "rocketeer.0102.txt"));
        }

        public static string PluginsFolderPath
        {
            get => Path.Combine(Finder.ModContentPack.RootDir, PluginFolderSubPath);
        }

        public static string DependenciesFolderPath
        {
            get => Path.Combine(Finder.ModContentPack.RootDir, DependenciesSubPath);
        }

        public static string ExperimentalPluginsFolderPath
        {
            get => IsDevEnv ? Path.Combine(Finder.ModContentPack.RootDir, ExperimentalPluginFolderSubPath) : null;
        }

        public static string ConfigFolderPath
        {
            get => GenFilePaths.ConfigFolderPath;
        }

        public static string CustomConfigFolderPath
        {
            get => Path.Combine(Directory.GetParent(GenFilePaths.ConfigFolderPath).FullName, "RocketMan");
        }

        public static string RocketSettingsFilePath
        {
            get => Path.Combine(CustomConfigFolderPath, GenText.SanitizeFilename($"Mod_{RocketMod.Instance.Content.FolderName}_{RocketMod.Instance.GetType().Name}.xml"));
        }

        public static string LogsFolderPath
        {
            get => Path.Combine(CustomConfigFolderPath, LogFolderName);
        }

        public static bool IncompatibilityUnresolved = false;

        public static bool SoyuzLoaded = false;

        public static bool ProtonLoaded = false;

        public static bool RocketeerLoaded = false;

        public static bool GagarinLoaded = false;
    }
}
