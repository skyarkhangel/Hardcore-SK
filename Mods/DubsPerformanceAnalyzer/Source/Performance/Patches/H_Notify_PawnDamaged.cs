using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Analyzer.Performance
{
    internal class H_Notify_PawnDamaged : PerfPatch
    {
        public static bool Enabled = false;
        public override string Name => "performance.lordnotifydamage";
        public override PerformanceCategory Category => PerformanceCategory.Overrides;


        public override void OnEnabled(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(Lord), nameof(Lord.Notify_PawnDamaged)),
                prefix: new HarmonyMethod(typeof(H_Notify_PawnDamaged), nameof(Prefix)));
        }


        public static bool Prefix(Pawn victim)
        {
            if (!Enabled) return true;

            var L = victim.GetLord().CurLordToil;
            if (L is LordToil_AssaultColony || L is LordToil_AssaultColonySappers) return false;

            return true;
        }
    }
}