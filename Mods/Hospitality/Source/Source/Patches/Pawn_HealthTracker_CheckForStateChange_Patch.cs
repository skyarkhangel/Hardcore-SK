using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "CheckForStateChange", null)]
    public static class Pawn_HealthTracker_CheckForStateChange_Patch
    {
        public static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn, Hediff hediff)
        {
            if (Settings.disableGuests) return;
            if (!HealthAIUtility.ShouldSeekMedicalRest(___pawn) && !___pawn.health.hediffSet.HasNaturallyHealingInjury() 
                                                                && Utilities.GuestUtility.GuestHasNoLord(___pawn))
            {
                Utilities.GuestUtility.GiveLordToRoguePawn(___pawn);
            }
        }
    }
}