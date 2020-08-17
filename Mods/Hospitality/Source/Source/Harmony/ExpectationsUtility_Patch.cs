using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    /// <summary>
    /// Different expectations for guests (and expectations at all!)
    /// </summary>
    public class ExpectationsUtility_Patch
    {
        [HarmonyPatch(typeof(ExpectationsUtility), "CurrentExpectationFor", typeof(Pawn))]
        public class CurrentExpectationForPawn
        {
            [HarmonyPostfix]
            public static void Postfix(ref ExpectationDef __result, Pawn p, List<ExpectationDef> ___wealthExpectationsInOrder)
            {
                if (__result == null) return; // Original method aborted, so will we
                if (p.IsGuest())
                {
                    __result = CurrentExpectationFor(p.MapHeld, ___wealthExpectationsInOrder);
                }
            }

            // Copied
            private static ExpectationDef CurrentExpectationFor(Map map, List<ExpectationDef> wealthExpectations)
            {
                float wealthTotal = map.wealthWatcher.WealthTotal * 2; // Doubled for guests
                foreach (ExpectationDef expectationDef in wealthExpectations) 
                {
                    if (wealthTotal < expectationDef.maxMapWealth)
                    {
                        return expectationDef;
                    }
                }
                return wealthExpectations[wealthExpectations.Count - 1];
            }
        }
    }
}