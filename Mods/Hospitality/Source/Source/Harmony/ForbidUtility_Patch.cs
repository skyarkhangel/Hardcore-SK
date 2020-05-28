using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    internal static class ForbidUtility_Patch
    {
        /// <summary>
        /// So guests will care
        /// </summary>
        [HarmonyPatch(typeof(ForbidUtility), "CaresAboutForbidden")]
        public class CaresAboutForbidden
        {
            [HarmonyPrefix]
            public static bool Replacement(ref bool __result, Pawn pawn, bool cellTarget)
            {
                // I have split up the original check to make some sense of it. Still doesn't make any sense.
                __result = CrazyRimWorldCheck(pawn) && !pawn.InMentalState && (!cellTarget || !ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(pawn));
                return false;
            }

            private static bool CrazyRimWorldCheck(Pawn pawn)
            {
                // Guests need this in PlayerHome
                return (pawn.HostFaction == null || pawn.HostFaction == Faction.OfPlayer && pawn.Spawned /*&& !pawn.Map.IsPlayerHome*/ && NotInPrison(pawn) && NotFleeingPrisoner(pawn));
            }

            private static bool NotFleeingPrisoner(Pawn pawn)
            {
                return !pawn.IsPrisoner || pawn.guest.PrisonerIsSecure;
            }

            private static bool NotInPrison(Pawn pawn)
            {
                return pawn.GetRoom() == null || !pawn.GetRoom().isPrisonCell;
            }
        }

        /// <summary>
        /// Set by JobDriver_Patch and stores who is doing a toil right now, in which case we don't want to forbid things.
        /// </summary>
        public static Pawn currentToilWorker;

        /// <summary>
        /// Things dropped by guests are never forbidden
        /// </summary>
        [HarmonyPatch(typeof(ForbidUtility), "SetForbidden")]
        public class SetForbidden
        {
            [HarmonyPrefix]
            public static bool Prefix(Thing t, bool value)
            {
                if (value && currentToilWorker.IsArrivedGuest())
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Area check for guests trying to access things outside their zone.
        /// </summary>
        [HarmonyPatch(typeof(ForbidUtility), "InAllowedArea")]
        public class InAllowedArea
        {
            [HarmonyPostfix]
            public static void Postfix(IntVec3 c, Pawn forPawn, ref bool __result)
            {
                if (!__result) return; // Not ok anyway, moving on
                if (!forPawn.IsArrivedGuest()) return;

                var area = forPawn.GetGuestArea();
                if (area == null) return;
                if (!c.IsValid || !area[c]) __result = false;
            }
        }
    }
}
