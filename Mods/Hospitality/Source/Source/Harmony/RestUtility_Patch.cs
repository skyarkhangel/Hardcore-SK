using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    /// <summary>
    /// Allow only visitors to use guest beds and guests to only use medical regular beds
    /// </summary>
    internal static class RestUtility_Patch
    {
        [HarmonyPatch(typeof(RestUtility), "IsValidBedFor")]
        public class IsValidBedFor
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn sleeper, Thing bedThing, ref bool __result, bool sleeperWillBePrisoner)
            {
                if (!__result) return;
                switch (bedThing) {
                    // guest bed
                    case Building_GuestBed guestBed:
                        if (!sleeper.IsGuest()) __result = false;
                        break;
                    // normal bed
                    case Building_Bed bed:
                        if (sleeper.IsGuest())
                        {
                            __result = HealthAIUtility.ShouldSeekMedicalRest(sleeper) || sleeper.health.hediffSet.HasNaturallyHealingInjury() || sleeperWillBePrisoner;
                        }
                        break;
                }
            }
        }
    }
}