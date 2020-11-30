using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using Verse;

namespace CommonSense
{
    class MealStacking
    {
        [HarmonyPatch(typeof(Thing), "CanStackWith", new Type[] { typeof(Thing) })]
        static class CompIngredients_CanStackWith_CommonSensePatch
        {
            static int getflags(CompIngredients compIngredients)
            {
                int b = 0;
                //0 - clean;
                //1 - positive
                //2 - negative
                //4 - humanlike

                foreach (var ing in compIngredients.ingredients)
                    if (!ing.IsIngestible)
                        continue;
                    else if (FoodUtility.IsHumanlikeMeat(ing))
                        b = b | 4;
                    else if (ing.ingestible.specialThoughtAsIngredient != null)
                        if (ing.ingestible.specialThoughtAsIngredient.stages.Count > 0)
                            //if(ing.ingestible.specialThoughtAsIngredient.stages[0].baseMoodEffect > 0)
                            //    b = b | 1;
                            //else 
                            if (!Settings.odd_is_normal && ing.ingestible.specialThoughtAsIngredient.stages[0].baseMoodEffect < 0)
                                b = b | 2;

                return b;
            }

            static void Postfix(ref bool __result, ref Thing __instance, ref Thing other)
            {
                if (!Settings.separate_meals || __instance == null || !__result || other == null || !other.def.IsIngestible)
                    return;

                CompIngredients ings = __instance.TryGetComp<CompIngredients>();

                if (ings == null)
                    return;

                CompIngredients otherings = other.TryGetComp<CompIngredients>();

                if (otherings == null)
                    return;

                __result = getflags(ings) == getflags(otherings);
            }
        }
    }
}
