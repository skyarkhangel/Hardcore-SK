using HarmonyLib;
using Verse.AI;

namespace Hospitality.Harmony
{
    /// <summary>
    /// Works together with ForbidUtility_Patch to prevent guests from forbidding items during work
    /// </summary>
    public class JobDriver_Patch
    {
        [HarmonyPatch(typeof(JobDriver), "DriverTick")]
        public class DriverTick
        {
            [HarmonyPrefix]
            public static void Prefix(JobDriver __instance)
            {
                ForbidUtility_Patch.currentToilWorker = __instance.pawn;
            }
        }
    }
}