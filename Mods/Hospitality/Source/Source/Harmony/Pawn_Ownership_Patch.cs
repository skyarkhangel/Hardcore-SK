using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    /// <summary>
    /// So guests will properly unclaim their beds.
    /// </summary>
    internal static class Pawn_Ownership_Patch
    {
        [HarmonyPatch(typeof(Pawn_Ownership), "UnclaimBed")]
        public class UnclaimBed
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn ___pawn)
            {
                ___pawn.CompGuest()?.ClearOwnership();
                return true;
            }
        }

        [HarmonyPatch(typeof(Pawn_Ownership), "get_OwnedBed")]
        public class get_OwnedBed
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn ___pawn, ref Building_Bed __result)
            {
                if (!___pawn.IsGuest(false)) return true;
                __result = ___pawn.CompGuest()?.bed;
                return false;
            }
        }
    }
}
