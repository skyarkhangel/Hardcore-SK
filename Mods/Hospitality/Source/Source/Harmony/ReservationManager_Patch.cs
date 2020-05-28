using HarmonyLib;
using Verse;
using Verse.AI;

namespace Hospitality.Harmony
{
    /// <summary>
    /// Suppresses "Could not reserve" error, caused by guests doing the job of a colonist. Doesn't seem to be a problem further.
    /// 
    /// Well actually, normal colonists cause it as well. So let's suppress it entirely. It's very annoying.
    /// </summary>
    public class ReservationManager_Patch
    {
        [HarmonyPatch(typeof(ReservationManager), "LogCouldNotReserveError")]
        public class LogCouldNotReserveError
        {
            [HarmonyPrefix]
            public static bool Prefix(ReservationManager __instance, Pawn claimant, LocalTargetInfo target)
            {
                // We disable the error message entirely, it does no harm
                return false;

                // Code for only disabling it for guests:
                //if (claimant.IsGuest()) return false;
                
                //Pawn pawn = __instance.FirstRespectedReserver(target, claimant);
                //if (pawn.IsGuest()) return false;

                //return true;
            }
        }
    }
}