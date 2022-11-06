using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Gagarin
{
    public static class GagarinTargets
    {
        public static readonly MethodBase LoadedModManager_LoadModXML = AccessTools.Method(typeof(LoadedModManager), nameof(LoadedModManager.LoadModXML));

        public static readonly MethodBase LoadedModManager_CombineIntoUnifiedXML = AccessTools.Method(typeof(LoadedModManager), nameof(LoadedModManager.CombineIntoUnifiedXML));

        public static readonly MethodBase TKeySystem_Parse = AccessTools.Method(typeof(TKeySystem), nameof(TKeySystem.Parse));

        public static readonly MethodBase LoadedModManager_ApplyPatches = AccessTools.Method(typeof(LoadedModManager), nameof(LoadedModManager.ApplyPatches));

        public static readonly MethodBase LoadedModManager_ParseAndProcessXML = AccessTools.Method(typeof(LoadedModManager), nameof(LoadedModManager.ParseAndProcessXML));

        public static readonly MethodBase LoadedModManager_ClearCachedPatches = AccessTools.Method(typeof(LoadedModManager), nameof(LoadedModManager.ClearCachedPatches));
    }
}
