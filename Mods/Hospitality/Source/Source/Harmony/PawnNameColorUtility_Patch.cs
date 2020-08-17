using HarmonyLib;
using UnityEngine;
using Verse;

namespace Hospitality.Harmony {
    /// <summary>
    /// Guests use their faction color for their name labels
    /// </summary>
    public class PawnNameColorUtility_Patch
    {
        [HarmonyPatch(typeof(PawnNameColorUtility), "PawnNameColorOf")]
        public class PawnNameColorOf
        {
            [HarmonyPrefix]
            public static bool Prefix(ref Color __result, Pawn pawn)
            {
                if (pawn == null) return true;
                if (!pawn.IsGuest() || pawn.IsTrader(false)) return true;
                if (pawn.Faction == null) return true;

                __result = pawn.Faction.Color;
                return false;
            }
        }
    }
}