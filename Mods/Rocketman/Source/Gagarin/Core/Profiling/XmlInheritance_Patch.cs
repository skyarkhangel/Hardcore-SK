using System;
using System.Diagnostics;
using HarmonyLib;
using Verse;

namespace Gagarin
{
    public static class XmlInheritance_Patch
    {
        [GagarinPatch(typeof(XmlInheritance), nameof(XmlInheritance.Resolve))]
        public static class XmlInheritance_Resolve_Patch
        {
            private static Stopwatch stopwatch = new Stopwatch();

            [HarmonyPriority(1000)]
            public static void Prefix()
            {
                stopwatch.Start();
            }

            [HarmonyPriority(Priority.Last)]
            public static void Postfix()
            {
                stopwatch.Stop();
                Log.Warning($"GAGARIN: <color=white>XmlInheritance.Resolve</color> took <color=red>{Math.Round((float)stopwatch.ElapsedMilliseconds / 1000f, 4)}</color> seconds");
            }
        }
    }
}
