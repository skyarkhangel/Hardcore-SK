using HarmonyLib;
using RimWorld;

namespace Analyzer.Performance
{
    internal class H_CompDeepDrill : PerfPatch
    {
        public static bool Enabled = false;
        public override PerformanceCategory Category => PerformanceCategory.Overrides;

        public override string Name => "performance.compdeepdrill";

        public override void OnEnabled(Harmony harmony)
        {
            var skiff = AccessTools.Method(typeof(CompDeepDrill), nameof(CompDeepDrill.CanDrillNow));
            harmony.Patch(skiff, new HarmonyMethod(typeof(H_CompDeepDrill), nameof(Prefix)));
        }

        public static bool Prefix(CompDeepDrill __instance, ref bool __result)
        {
            if (!Enabled) return true;
            

            __result = (__instance.powerComp == null || __instance.powerComp.PowerOn) && (__instance.parent.Map.Biome.hasBedrock || __instance.ValuableResourcesPresent());
            return false;
        }
    }
}