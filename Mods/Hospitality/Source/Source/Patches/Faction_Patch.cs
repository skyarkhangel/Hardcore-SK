using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
    /// <summary>
    /// When a rescued guest leaves the map (not in a pod), mark him as rescued for the reward
    /// </summary>
    public class Faction_Patch
    {
        [HarmonyPatch(typeof(Faction), nameof(Faction.Notify_MemberExitedMap))]
        public class Notify_MemberExitedMap
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn member, ref bool free)
            {
                if (member.Faction == Faction.OfPlayer) return true;
                if (PawnUtility.IsTravelingInTransportPodWorldObject(member)) return false; // Fired in pod? Don't trigger

                var compGuest = member.CompGuest();

                if (compGuest == null || !compGuest.rescued || member.guest == null || PawnUtility.IsTravelingInTransportPodWorldObject(member)) return true;

                free = true;
                member.guest.hostFactionInt = Faction.OfPlayer; // Setting this makes the reward work
                compGuest.rescued = false; // Turn back off
                compGuest.wasDowned = false;

                return true;
            }
        }
    }
}
