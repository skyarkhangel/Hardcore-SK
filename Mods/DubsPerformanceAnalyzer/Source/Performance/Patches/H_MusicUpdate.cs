using HarmonyLib;
using RimWorld;
using Verse;

namespace Analyzer.Performance
{
    internal class H_MusicUpdate : PerfPatch
    {
        public static bool Enabled = false;
        public override PerformanceCategory Category => PerformanceCategory.Removes;

        public override string Name => "performance.killaudio";

        public override void OnEnabled(Harmony harmony)
        {
            var skiff = AccessTools.Method(typeof(MusicManagerPlay), nameof(MusicManagerPlay.MusicUpdate));
            harmony.Patch(skiff, new HarmonyMethod(typeof(H_MusicUpdate), nameof(Prefix)));
        }

        public static bool Prefix()
        {
            return !Enabled;
        }
    }
}