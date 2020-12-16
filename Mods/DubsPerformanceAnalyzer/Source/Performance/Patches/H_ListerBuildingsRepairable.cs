using HarmonyLib;
using RimWorld;
using Verse;

namespace Analyzer.Performance
{
    internal class H_ListerBuildingsRepairable : PerfPatch
    {
        public static bool Enabled = false;
        public override string Name => "performance.buildingrepair";
        public override PerformanceCategory Category => PerformanceCategory.Optimizes;


        public override void OnEnabled(Harmony harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(ListerBuildingsRepairable), nameof(ListerBuildingsRepairable.UpdateBuilding)),
                new HarmonyMethod(typeof(H_ListerBuildingsRepairable), nameof(Prefix)));
        }

        public static bool Prefix(Building b)
        {
            if (!Enabled) return true;
            return b.def.building.repairable && b.def.useHitPoints;
        }
    }
}