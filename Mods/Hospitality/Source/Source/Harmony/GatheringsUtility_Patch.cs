using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    /// <summary>
    /// Androids don't have food or rest need. Vanilla assumes everyone does. Here's the fix that shouldn't be from this mod.
    /// </summary>
    public static class GatheringsUtility_Patch
    {
        [HarmonyPatch(typeof(GatheringsUtility), "ShouldGuestKeepAttendingGathering")]
        public class ShouldGuestKeepAttendingGathering
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn p, ref bool __result)
            {
                if (p.needs?.food == null || p.needs?.rest == null) return ReplacementForAndroids(p, out __result);
                return true;
            }

            private static bool ReplacementForAndroids(Pawn p, out bool __result)
            {
                __result = !p.Downed && (p.needs?.food == null || !p.needs.food.Starving) && p.health.hediffSet.BleedRateTotal <= 0f && (p.needs?.rest == null || p.needs.rest.CurCategory < RestCategory.Exhausted)
                           && !p.health.hediffSet.HasTendableNonInjuryNonMissingPartHediff(false) && p.Awake() && !p.InAggroMentalState && !p.IsPrisoner;
                return false;
            }
        }
    }
}