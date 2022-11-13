using HarmonyLib;
using RimWorld;
using Verse;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(Need_Rest), nameof(Need_Rest.TickResting))]
    public class Need_Rest_TickResting_Patch
    {
        public static void Postfix(Need_Rest __instance)
        {
            if (__instance.pawn.IsBeingThrottled())
                __instance.lastRestTick = GenTicks.TicksGame + __instance.pawn.GetTimeDelta() * 2;
        }
    }
}