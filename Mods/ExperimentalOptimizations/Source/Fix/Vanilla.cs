using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ExperimentalOptimizations.Fix
{
    [FixOn(InitStage.ModInit)]
    public class Vanilla_JobDriver_Wait_CheckForAutoAttack
    {
        public static void Patch()
        {
            typeof(JobDriver_Wait).Method(nameof(JobDriver_Wait.CheckForAutoAttack))
                .Patch(prefix: typeof(Vanilla_JobDriver_Wait_CheckForAutoAttack).HarmonyMethod(nameof(CheckForAutoAttack)), autoPatch: true);
            Log.Message($"[ExperimentalOptimizations] JobDriver_Wait:CheckForAutoAttack optimized");
        }

        private static bool CheckForAutoAttack(JobDriver_Wait __instance)
        {
            var p = __instance.pawn;
            return p.IsHashIntervalTick(p.Faction == null || p.Faction.IsPlayer ? 40 : 120);
        }
    }
}