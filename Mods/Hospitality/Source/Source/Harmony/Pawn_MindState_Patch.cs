using HarmonyLib;
using Verse.AI;

namespace Hospitality.Harmony
{
    /// <summary>
    /// So downed guests stay traders.
    /// </summary>
    public class Pawn_MindState_Patch
    {
        [HarmonyPatch(typeof(Pawn_MindState), "Reset")]
        public class TryStartMentalState
        {
            [HarmonyPostfix]
            public static void Postfix(ref Pawn_MindState __instance)
            {
                if (!__instance.pawn.IsGuest()) return;

                __instance.pawn.ConvertToTrader(false);
            }
        }
    }
}
