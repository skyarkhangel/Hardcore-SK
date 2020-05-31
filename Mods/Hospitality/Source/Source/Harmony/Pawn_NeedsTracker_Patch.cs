using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    /// <summary>
    /// Added Joy and Comfort to guests
    /// </summary>
    internal static class Pawn_NeedsTracker_Patch
    {
        [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
        public class ShouldHaveNeed
        {
            private static readonly NeedDef defComfort = DefDatabase<NeedDef>.GetNamed("Comfort");
            private static readonly NeedDef defBeauty = DefDatabase<NeedDef>.GetNamed("Beauty");
            private static readonly NeedDef defSpace = DefDatabase<NeedDef>.GetNamed("RoomSize");

            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, NeedDef nd, Pawn ___pawn)
            {
                if ((nd == NeedDefOf.Joy || nd == defComfort || nd == defBeauty || nd == defSpace) && ___pawn.IsGuest()) // ADDED
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
    }
}