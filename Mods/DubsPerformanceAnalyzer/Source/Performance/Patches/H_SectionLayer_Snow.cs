using Analyzer.Profiling;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Analyzer.Performance
{
    internal class H_SectionLayer_Snow : PerfPatch
    {
        public override string Name => "performance.sectionlayersnow";
        public override PerformanceCategory Category => PerformanceCategory.Optimizes;

        public static bool Enabled = false;

        public override void OnEnabled(Harmony harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(SectionLayer_Snow), nameof(SectionLayer_Snow.SnowDepthColor)),
                prefix: new HarmonyMethod(typeof(H_SectionLayer_Snow), nameof(Prefix)));
        }

        public static bool Prefix(float snowDepth, ref Color32 __result)
        {
            if (!Enabled) return true;

            __result = new Color32(255, 255, 255, (byte)(byte.MaxValue * snowDepth));
            return false;
        }
    }
}