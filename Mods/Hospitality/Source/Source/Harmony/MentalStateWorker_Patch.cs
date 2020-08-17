using Harmony;
using Verse;
using Verse.AI;

namespace Hospitality.Harmony
{
    /// <summary>
    /// Prevent guests from some mental breaks
    /// </summary>
    public class MentalStateWorker_Patch
    {
        [HarmonyPatch(typeof(MentalStateWorker), "StateCanOccur")]
        public class StateCanOccur
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, ref MentalStateWorker __instance, Pawn pawn)
            {
                if (!pawn.IsGuest()) return true;

                if (__instance.def.colonistsOnly) 
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}