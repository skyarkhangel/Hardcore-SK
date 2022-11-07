using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
    /// <summary>
    /// So guests will properly unclaim their beds.
    /// </summary>
    internal static class Pawn_Ownership_Patch
    {
        [HarmonyPatch(typeof(Pawn_Ownership), nameof(Pawn_Ownership.UnclaimBed))]
        public class UnclaimBed
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn ___pawn)
            {
                //Log.Message($"{___pawn?.NameShortColored}: UnclaimBed. bed = {___pawn?.CompGuest()?.bed?.Position}");
                ___pawn.CompGuest()?.ClearOwnership();
                return true;
            }
        }

        [HarmonyPatch(typeof(Pawn_Ownership))]
        [HarmonyPatch(nameof(Pawn_Ownership.OwnedBed), MethodType.Getter)]
        public class OwnedBed
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn ___pawn, ref Building_Bed __result)
            {
                if (!___pawn.IsGuest()) return true;
                __result = ___pawn.CompGuest()?.bed;
                return false;
            }
        }
    }
}
