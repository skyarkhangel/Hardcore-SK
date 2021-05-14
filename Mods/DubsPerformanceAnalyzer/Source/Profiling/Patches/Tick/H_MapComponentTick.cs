using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI.Group;

namespace Analyzer.Profiling
{
    [Entry("entry.tick.mapcomponent", Category.Tick)]
    internal class H_MapComponentTick
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            foreach (var meth in Utility.SubClassNonAbstractImplementationsOf(typeof(MapComponent), t => t.Name == "MapComponentTick"))
                yield return meth;

            yield return AccessTools.Method(typeof(WildAnimalSpawner), nameof(WildAnimalSpawner.WildAnimalSpawnerTick));
            yield return AccessTools.Method(typeof(WildPlantSpawner), nameof(WildPlantSpawner.WildPlantSpawnerTick));
            yield return AccessTools.Method(typeof(PowerNetManager), nameof(PowerNetManager.PowerNetsTick));
            yield return AccessTools.Method(typeof(SteadyEnvironmentEffects), nameof(SteadyEnvironmentEffects.SteadyEnvironmentEffectsTick));
            yield return AccessTools.Method(typeof(LordManager), nameof(LordManager.LordManagerTick));
            yield return AccessTools.Method(typeof(PassingShipManager), nameof(PassingShipManager.PassingShipManagerTick));
            yield return AccessTools.Method(typeof(VoluntarilyJoinableLordsStarter), nameof(VoluntarilyJoinableLordsStarter.VoluntarilyJoinableLordsStarterTick));
            yield return AccessTools.Method(typeof(GameConditionManager), nameof(GameConditionManager.GameConditionManagerTick));
            yield return AccessTools.Method(typeof(WeatherManager), nameof(WeatherManager.WeatherManagerTick));
            yield return AccessTools.Method(typeof(ResourceCounter), nameof(ResourceCounter.ResourceCounterTick));
            yield return AccessTools.Method(typeof(WeatherDecider), nameof(WeatherDecider.WeatherDeciderTick));
            yield return AccessTools.Method(typeof(FireWatcher), nameof(FireWatcher.FireWatcherTick));

        }

        public static string GetLabel(MapComponent __instance) => __instance.GetType().FullName;
    }
}