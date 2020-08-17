using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    /// <summary>
    /// Don't notify about guests
    /// </summary>
    public class PawnUtility_Patch
    {
        [HarmonyPatch(typeof (PawnUtility), "ShouldSendNotificationAbout")]
        public class ShouldSendNotificationAbout
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result, Pawn p)
            {
                if (!__result) return;

                if (p.Faction != Faction.OfPlayer && p.HostFaction == Faction.OfPlayer && p.IsGuest())
                {
                    __result = false;
                }
            }
        }
    }
}