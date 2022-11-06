using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Gagarin
{
    public static class LoadedModManager_Profiler
    {
        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.LoadModXML))]
        public static class LoadModXML_Profiler
        {
            private static Stopwatch stopwatch = new Stopwatch();

            [HarmonyPriority(1000)]
            public static void Prefix()
            {
                stopwatch.Reset();
                stopwatch.Start();
            }

            [HarmonyPriority(Priority.Last)]
            public static void Postfix()
            {
                stopwatch.Stop();
                Log.Warning($"GAGARIN: <color=white>LoadModXML_Profiler</color> took " +
                    $"<color=red>{Math.Round((float)stopwatch.ElapsedMilliseconds / 1000f, 4)}</color> seconds");
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.CombineIntoUnifiedXML))]
        public static class CombineIntoUnifiedXML_Profiler
        {
            private static Stopwatch stopwatch = new Stopwatch();

            [HarmonyPriority(1000)]
            public static void Prefix()
            {
                stopwatch.Reset();
                stopwatch.Start();
            }

            [HarmonyPriority(Priority.Last)]
            public static void Postfix()
            {
                stopwatch.Stop();
                Log.Warning($"GAGARIN: <color=white>CombineIntoUnifiedXML_Profiler</color> took " +
                    $"<color=red>{Math.Round((float)stopwatch.ElapsedMilliseconds / 1000f, 4)}</color> seconds");
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ApplyPatches))]
        public static class ApplyPatches_Profiler
        {
            private static Stopwatch stopwatch = new Stopwatch();

            [HarmonyPriority(1000)]
            public static void Prefix()
            {
                stopwatch.Reset();
                stopwatch.Start();
            }

            [HarmonyPriority(Priority.Last)]
            public static void Postfix()
            {
                stopwatch.Stop();
                Log.Warning($"GAGARIN: <color=white>ApplyPatches_Profiler</color> took " +
                    $"<color=red>{Math.Round((float)stopwatch.ElapsedMilliseconds / 1000f, 4)}</color> seconds");
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ParseAndProcessXML))]
        public static class ParseAndProcessXML_Profiler
        {
            private static Stopwatch stopwatch = new Stopwatch();

            [HarmonyPriority(1000)]
            public static void Prefix()
            {
                stopwatch.Reset();
                stopwatch.Start();
            }

            [HarmonyPriority(Priority.Last)]
            public static void Postfix()
            {
                stopwatch.Stop();
                Log.Warning($"GAGARIN: <color=white>ParseAndProcessXML_Profiler</color> took " +
                    $"<color=red>{Math.Round((float)stopwatch.ElapsedMilliseconds / 1000f, 4)}</color> seconds");
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ClearCachedPatches))]
        public static class ClearCachedPatches_Profiler
        {
            private static Stopwatch stopwatch = new Stopwatch();

            [HarmonyPriority(1000)]
            public static void Prefix()
            {
                stopwatch.Reset();
                stopwatch.Start();
            }

            [HarmonyPriority(Priority.Last)]
            public static void Postfix()
            {
                stopwatch.Stop();
                Log.Warning($"GAGARIN: <color=white>ClearCachedPatches_Profiler</color> took " +
                    $"<color=red>{Math.Round((float)stopwatch.ElapsedMilliseconds / 1000f, 4)}</color> seconds");
            }
        }
    }
}
