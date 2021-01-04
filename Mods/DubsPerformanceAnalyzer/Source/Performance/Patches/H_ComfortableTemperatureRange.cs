using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace Analyzer.Performance
{
    internal class H_ComfortableTemperatureRange : PerfPatch
    {
        public static bool Enabled = false;
        public override PerformanceCategory Category => PerformanceCategory.Optimizes;

        public static Dictionary<int, FloatRange> tempCache = new Dictionary<int, FloatRange>();
        public static int LastTick = 0;

        public override string Name => "performance.tempcache";

        public override void OnEnabled(Harmony harmony)
        {
            var jiff = AccessTools.Method(typeof(GenTemperature), nameof(GenTemperature.ComfortableTemperatureRange), new[] { typeof(Pawn) });
            var pre = new HarmonyMethod(typeof(H_ComfortableTemperatureRange), nameof(Prefix));
            var post = new HarmonyMethod(typeof(H_ComfortableTemperatureRange), nameof(Postfix));
            harmony.Patch(jiff, pre, post);
        }


        public static bool Prefix(Pawn p, ref FloatRange __result)
        {
            if (!Enabled) return true;

            if (LastTick != Find.TickManager.TicksGame)
            {
                LastTick = Find.TickManager.TicksGame;
                tempCache.Clear();
            }

            if (tempCache.TryGetValue(p.thingIDNumber, out var result))
            {
                __result = result;
                return false;
            }

            return true;
        }

        public static void Postfix(Pawn p, FloatRange __result)
        {
            if (!Enabled) return;

            if (!tempCache.ContainsKey(p.thingIDNumber))
            {
                tempCache.Add(p.thingIDNumber, __result);
            }
        }
    }
}