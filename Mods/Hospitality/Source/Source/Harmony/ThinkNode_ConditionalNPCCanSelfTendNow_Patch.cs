using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    public class ThinkNode_ConditionalNPCCanSelfTendNow_Patch
    {
        /// <summary>
        /// Disable self-tending of guests
        /// </summary>
        [HarmonyPatch(typeof(ThinkNode_ConditionalNPCCanSelfTendNow), "Satisfied")]
        public class Satisfied
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result, Pawn pawn)
            {
                if (!__result) return;
                if (pawn.IsArrivedGuest()) __result = false;
            }
        }
    }
}
