using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ExperimentalOptimizations.Fix
{
    //[FixOn(InitStage.ModInit)]
    //public class Vanilla_JobDriver_Wait_CheckForAutoAttack
    //{
    //    public static void Patch()
    //    {
    //        typeof(JobDriver_Wait).Method(nameof(JobDriver_Wait.CheckForAutoAttack))
    //            .Patch(prefix: typeof(Vanilla_JobDriver_Wait_CheckForAutoAttack).HarmonyMethod(nameof(CheckForAutoAttack)), autoPatch: true);
    //        Log.Message($"[ExperimentalOptimizations] JobDriver_Wait:CheckForAutoAttack optimized");
    //    }

    //    private static bool CheckForAutoAttack(JobDriver_Wait __instance)
    //    {
    //        var p = __instance.pawn;
    //        int interval = p.Faction == null || p.Faction.IsPlayer ? 40 : 120;
    //        return (Time.frameCount + __instance.pawn.thingIDNumber.HashOffset()) % interval == 0;
    //    }
    //}
    /*
    public static bool IsHashIntervalTick(this Thing t, int interval)
	{
		return t.HashOffsetTicks() % interval == 0;
	}

    public static int HashOffsetTicks(this Thing t)
	{
		return Find.TickManager.TicksGame + t.thingIDNumber.HashOffset();
	}
     */
}