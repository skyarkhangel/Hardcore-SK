using Harmony;
using QOLTweaksPack.utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace QOLTweaksPack.tweaks
{
    [HarmonyPatch(typeof(Bill_Production), "ShouldDoNow")]
    static class WorkGiver_ShouldDoNow_Postfix
    {
        [HarmonyPostfix]
        private static void ShouldDoNow(Bill_Production __instance, ref bool __result)
        {
            if (__result == false)
                return;
            if (QOLTweaksPack.CookingEquivalency.Value == false)
                return;

            if(__instance.repeatMode == BillRepeatModeDefOf.TargetCount)
            {
                if (__instance.recipe.products == null || __instance.recipe.products.Count == 0)
                    return;
                ThingDef product = __instance.recipe.products[0].thingDef;
                if (QOLTweaksPack.MealSelection.Value.InnerList.Contains(product.defName))
                {
                    int num = ItemCounter.CountProductsWithEquivalency(__instance, __instance.recipe.WorkerCounter);
                    if (__instance.pauseWhenSatisfied && num >= __instance.targetCount)
                    {
                        __instance.paused = true;
                    }
                    if (num <= __instance.unpauseWhenYouHave || !__instance.pauseWhenSatisfied)
                    {
                        __instance.paused = false;
                    }
                    __result = !__instance.paused && num < __instance.targetCount;
                }
            }
        }    
    }

    [HarmonyPatch(typeof(Bill_Production), "DoConfigInterface")]
    static class WorkGiver_DoConfigInterface_Postfix
    {
        [HarmonyPostfix]
        private static void DoConfigInterface(Bill_Production __instance, Rect baseRect, Color baseColor)
        {
            if (QOLTweaksPack.CookingEquivalency.Value == false)
                return;
            if (__instance.repeatMode != BillRepeatModeDefOf.TargetCount)
                return;

            ThingDef product = __instance.recipe.products[0].thingDef;
            if (QOLTweaksPack.MealSelection.Value.InnerList.Contains(product.defName))
            {
                int num = ItemCounter.CountProductsWithEquivalency(__instance, __instance.recipe.WorkerCounter);

                Rect rect = new Rect(78f, 32f, 30f, 30f);
                GUI.color = new Color(0.75f, 1f, 0.5f, 0.75f);
                Widgets.Label(rect, "/"+num);
                GUI.color = baseColor;
            }
        }
    }
}
