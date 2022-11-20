using System;
using System.Collections.Generic;
using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using Verse;
using Verse.AI;
using FoodUtility = Hospitality.Utilities.FoodUtility;

namespace Hospitality
{
    public static class Toils_Ingest_Patch
    {
        [HarmonyPatch(typeof(Toils_Ingest), nameof(Toils_Ingest.TakeMealFromDispenser))]
        public static class TakeMealFromDispenser_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(TargetIndex ind, Pawn eater, ref Toil __result)
            {
                if (eater.IsArrivedGuest(out _) && __result != null)
                {
                    var dispenser = ((Building_NutrientPasteDispenser)eater.jobs.curJob.GetTarget(ind).Thing);
                    //If guest didnt eat from a vending machine, ignore the rest
                    if (dispenser == null) return;

                    __result.finishActions ??= new List<Action>();
                    __result.finishActions.Add(delegate
                    {
                        var food = eater.carryTracker.CarriedThing;
                        if (food != null && eater.CurJob.GetTarget(ind) == food)
                        {
                            FoodUtility.TryPayForFood(eater, dispenser);
                        }
                    });
                }
            }
        }
    }
}
