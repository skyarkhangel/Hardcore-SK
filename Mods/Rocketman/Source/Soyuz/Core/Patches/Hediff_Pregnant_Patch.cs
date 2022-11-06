using System.Runtime.CompilerServices;
using HarmonyLib;
using Verse;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(Hediff_Pregnant), nameof(Hediff_Pregnant.Tick))]
    public class Hediff_Pregnant_Tick_Patch
    {
        public static void Prefix(Hediff_Pregnant __instance)
        {
            var pawn = __instance.pawn;
            if (pawn.IsBeingThrottled())
            {
                int deltaT = pawn.GetTimeDelta();
                __instance.ageTicks += deltaT - 1;
                __instance.GestationProgress += (deltaT - 1) / (pawn.RaceProps.gestationPeriodDays * 60000f);
            }
        }
    }
}