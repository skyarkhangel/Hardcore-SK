using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using GuestUtility = Hospitality.Utilities.GuestUtility;

namespace Hospitality.Patches
{
    /// <summary>
    /// So guests will care
    /// </summary>
    internal static class FoodUtility_Patch
    {
        [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.IsFoodSourceOnMapSociallyProper))]
        public class IsFoodSourceOnMapSociallyProperPatch
        {
            public static void Postfix(Thing t, Pawn getter, Pawn eater, bool allowSociallyImproper, ref bool __result)
            {
                if (!GuestUtility.IsArrivedGuest(eater, out _)) return;
                __result = Utilities.FoodUtility.GuestCanUseFoodSourceInternal(eater, t);
            }
        }

        [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap))]
        public class BestFoodSourceOnMapPatch
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn getter, Pawn eater, bool desperate, ThingDef foodDef, ref Thing __result)
            {
                if (!GuestUtility.IsArrivedGuest(eater, out _)) return;
                if (Utilities.FoodUtility.GuestCanUseFoodSourceExceptions(eater, __result, foodDef, desperate)) return;
                __result = null;
            }
        }

        [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.FoodOptimality))]
        public class FoodOptimalityPatch
        {
            private const int OptimalityBonus = 30;

            [HarmonyPostfix]
            public static void Postfix(ref float __result, Pawn eater, Thing foodSource, ThingDef foodDef, float dist, bool takingToInventory = false)
            {
                if (GuestUtility.IsGuest(eater) && foodSource is Building_NutrientPasteDispenser nutrientPasteDispenser)
                {
                    var comp = foodSource.TryGetComp<CompVendingMachine>();
                    if (comp != null && comp.IsActive())
                    {
                        var marketValue = nutrientPasteDispenser.DispensableDef.BaseMarketValue;

                        __result += (marketValue - comp.CurrentPrice) / marketValue * OptimalityBonus;
                    }
                }
                //Log.Message($"FoodOptimality of {foodSource?.Label} for {eater}, result: {__result}");
            }
        }

        /// <summary>
        /// Patching _NewTemp if it exists, or original version if it doesn't, so players with older versions don't run into issues.
        /// Also: Goddammit, Ludeon :(
        /// </summary>
        [HarmonyPatch]
        public class TryFindBestFoodSourceFor_Patch
        {
            [HarmonyTargetMethod]
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.TryFindBestFoodSourceFor));
            }

            [HarmonyPostfix]
            public static void Postfix(Pawn getter, Pawn eater, ref bool __result, ref Thing foodSource, ref ThingDef foodDef, ref bool desperate)
            {
                if (!GuestUtility.IsArrivedGuest(eater, out _)) return;
                if (Utilities.FoodUtility.GuestCanUseFoodSourceExceptions(eater, foodSource, foodDef, desperate)) return;
                __result = false;
            }
        }
    }
}


    
