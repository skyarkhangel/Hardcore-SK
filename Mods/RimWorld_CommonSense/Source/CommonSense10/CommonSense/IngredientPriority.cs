using System;
using System.Collections.Generic;
using Harmony;
using RimWorld;
using Verse;
using Verse.AI;

namespace CommonSense
{
    public static class IngredientPriority
    {
        [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
        public static class WorkGiver_DoBill_TryStartNewDoBillJob_CommonSensePatch
        {
            static void Postfix(WorkGiver_DoBill __instance, bool __result, Pawn pawn, List<ThingCount> chosen)
            {
                //return;
                if (!__result || !Settings.adv_haul_all_ings)
                    return;

                Utility.OptimizePath(chosen);
            }
        }

        [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredientsInSet_AllowMix")]
        public static class WorkGiver_DoBill_TryFindBestBillIngredientsInSet_AllowMix_CommonSensePatch
        {
            public static bool Prefix(List<Thing> availableThings,  Bill bill)
            {

                if (!Settings.prefer_spoiling_ingredients || bill.recipe.addsHediff != null)
                    return true;

                availableThings.Sort(
                    delegate (Thing a, Thing b)
                    {

                        float p = a.GetStatValue(StatDefOf.MedicalPotency) - b.GetStatValue(StatDefOf.MedicalPotency);
                        if (p > 0)
                            return -1;
                        else if (p < 0)
                            return 1;

                        CompRottable compa = a.TryGetComp<CompRottable>();
                        CompRottable compb = b.TryGetComp<CompRottable>();

                        if (compa == null)
                            if (compb == null)
                                return 0;
                            else
                                return 1;
                        else if (compb == null)
                            return -1;
                        else
                        {

                            return (int)(compa.PropsRot.TicksToRotStart - compa.RotProgress) / 2500 - (int)(compb.PropsRot.TicksToRotStart - compb.RotProgress) / 2500;
                        }
                    }
                );

                return true;
            }
        }

        [HarmonyPatch(typeof(FoodUtility), "FoodOptimality")]
        static class FoodUtility_FoodOptimality
        {
            static void Postfix(ref float __result, Pawn eater, Thing foodSource, ThingDef foodDef, float dist, bool takingToInventory = false)
            {
                if(Settings.allow_feeding_with_plants && (eater.needs == null || eater.needs.mood == null))
                {
                    float modifier = 0f;
                    FoodPreferability pref = foodDef.ingestible.preferability;
                    if (eater.RaceProps.Eats(FoodTypeFlags.Plant))
                    {
                        if (foodDef.ingestible.foodType == FoodTypeFlags.Plant)
                            modifier += 5f;
                        if (foodSource is Plant)
                            modifier += 25f;
                    }
                    switch (pref)
                    {
                        case FoodPreferability.DesperateOnlyForHumanlikes:
                            modifier += 5f;
                            break;
                        case FoodPreferability.RawBad:
                            modifier += 5f;
                            break;
                        case FoodPreferability.RawTasty:
                            modifier -= 5f;
                            break;
                        case FoodPreferability.MealAwful:
                            modifier += 5f;
                            break;
                        case FoodPreferability.MealFine:
                            modifier -= 10f;
                            break;
                        case FoodPreferability.MealLavish:
                            modifier -= 15f;
                            break;
                        default:
                            modifier -= 5f;
                            break;
                    }
                    __result += modifier;
                }

                if (!Settings.prefer_spoiling_meals)
                    return;

                const float qday = 2500f * 6f;
                const float aday = qday * 4f;
                CompRottable compRottable = foodSource.TryGetComp<CompRottable>();
                if (compRottable != null)
                {
                    float t = compRottable.PropsRot.TicksToRotStart - compRottable.RotProgress;
                    if (t > 0 && t < aday * 2f)
                    {
                        __result += (float)Math.Truncate((1f + (aday * 2f - t) / qday) * 3f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes))]
        public static class ThingListGroupHelper_Includes_CommonSensePatch
        {
            static bool Prefix(ref bool __result, ThingRequestGroup group, ThingDef def)
            {
                if (!Settings.allow_feeding_with_plants || group != ThingRequestGroup.FoodSourceNotPlantOrTree)
                    return true;

                __result = (def.IsNutritionGivingIngestible && def.thingClass != typeof(Plant)) || def.thingClass == typeof(Building_NutrientPasteDispenser);
                return false;
            }
        }
    }
}
