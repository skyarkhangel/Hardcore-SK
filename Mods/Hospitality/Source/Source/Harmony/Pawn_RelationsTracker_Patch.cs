using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    /// <summary>
    /// Mark rescued guests for reward
    /// </summary>
    internal static class Pawn_RelationsTracker_Patch
    {
        [HarmonyPatch(typeof(Pawn_RelationsTracker), "Notify_RescuedBy")]
        public class Notify_RescuedBy
        {
            [HarmonyPrefix]
            public static void Prefix(Pawn ___pawn)
            {
                if (___pawn.Faction == Faction.OfPlayer) return;

                var compGuest = ___pawn.CompGuest();
                if (compGuest != null) compGuest.rescued = true;
            }
        }
    }
}
