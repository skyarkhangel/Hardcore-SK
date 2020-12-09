using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Analyzer.Profiling
{

    [Entry("entry.tick.world", Category.Tick)]
    public static class H_WorldPawns
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            foreach (var meth in typeof(WorldComponent).AllSubclasses().Select(wc => wc.GetMethod("WorldComponentTick")))
                yield return meth;

            yield return AccessTools.Method(typeof(WorldPawns), nameof(WorldPawns.WorldPawnsTick));
            yield return AccessTools.Method(typeof(FactionManager), nameof(FactionManager.FactionManagerTick));
            yield return AccessTools.Method(typeof(WorldObjectsHolder), nameof(WorldObjectsHolder.WorldObjectsHolderTick));
            yield return AccessTools.Method(typeof(WorldPathGrid), nameof(WorldPathGrid.WorldPathGridTick));
            yield return AccessTools.Method(typeof(Caravan), nameof(Caravan.Tick));
        }
    }
}