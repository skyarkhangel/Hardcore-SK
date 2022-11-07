using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
    /// <summary>
    /// So guests don't become prisoners
    /// </summary>
    internal static class Pawn_GuestTracker_Patch
    {
        [HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.SetGuestStatus))]
        public class SetGuestStatus
        {
            [HarmonyPrefix]
            public static bool Prefix(ref GuestStatus guestStatus, Pawn ___pawn)
            {
                if (___pawn?.IsGuest() != true) return true;

                // Became hostile?
                if (___pawn.Faction.HostileTo(Faction.OfPlayer))
                {
                    // Remove from guest list
                    ___pawn.MapHeld.GetMapComponent()?.PresentGuests.Remove(___pawn);
                    // But don't try to make them prisoners
                    return false;
                }
                guestStatus = GuestStatus.Guest;
                return true;
            }
        }
    }
}