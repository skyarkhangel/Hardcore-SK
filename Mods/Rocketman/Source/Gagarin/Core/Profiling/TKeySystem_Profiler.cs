using System;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Gagarin
{
    [GagarinPatch(typeof(TKeySystem), nameof(TKeySystem.Parse))]
    public static class TKeySystem_Profiler
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
            Log.Warning($"GAGARIN: <color=white>TKeySystem.Parse</color> took <color=red>{Math.Round((float)stopwatch.ElapsedMilliseconds / 1000f, 4)}</color> seconds");
        }
    }
}
