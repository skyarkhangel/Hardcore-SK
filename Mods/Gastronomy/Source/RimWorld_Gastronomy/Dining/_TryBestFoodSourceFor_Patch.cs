using HarmonyLib;
using RimWorld;
using Verse;

namespace Gastronomy.Dining
{
    /// <summary>
    /// For allowing pawns to find DiningSpots when hungry. This should later be replaced with BestFoodSourceOnMap, so alternatives are considered
    /// </summary>
    internal static class _TryBestFoodSourceFor_Patch
    {
        [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.TryFindBestFoodSourceFor))]
        public class TryFindBestFoodSourceFor
        {
            [HarmonyPrefix]
            internal static bool Prefix(Pawn getter, Pawn eater, ref bool __result, ref Thing foodSource, ref ThingDef foodDef, ref bool desperate)
            {
                if (desperate) return true; // Run original code

                if (getter != eater) return true; // Run original code

                // Only if time assignment allows
                if (!eater.GetTimeAssignment().allowJoy) return true;

                if (!getter.IsAbleToDine()) return true;

                var diningSpot = DiningUtility.FindDiningSpotFor(eater, false);

                var bestType = diningSpot?.GetRestaurant().Stock.GetBestMealTypeFor(eater, false);
                if (bestType == null) return true; // Run original code

                foodDef = bestType;
                foodSource = diningSpot;
                //Log.Message($"{getter.NameShortColored} found diningSpot at {diningSpot.Position} with {foodDef?.label}.");
                __result = true;
                return false; // Don't run original code
            }
        }
    }
}
