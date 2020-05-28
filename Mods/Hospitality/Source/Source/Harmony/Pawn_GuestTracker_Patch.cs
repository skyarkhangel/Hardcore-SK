using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    internal static class Pawn_GuestTracker_Patch
    {
        // Detoured so guests don't become prisoners
        [HarmonyPatch(typeof(Pawn_GuestTracker), "SetGuestStatus")]
        public class SetGuestStatus
        {
            [HarmonyPrefix]
            public static void Prefix(ref bool prisoner, Pawn ___pawn)
            {
                // Added
                if (___pawn?.IsGuest() == true) prisoner = false;
            }
        }
    }
}