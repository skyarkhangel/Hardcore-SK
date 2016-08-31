using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using CommunityCoreLibrary;

namespace Psychology.Detour
{
    // Token: 0x02000006 RID: 6
    internal static class _FoodUtility
    {

        internal static List<ThoughtDef> IngestThoughts()
        {
            if (_ingestThoughts == null)
            {
                _ingestThoughts = typeof(FoodUtility).GetField("ingestThoughts", BindingFlags.Static | BindingFlags.NonPublic);
            }
            return (List<ThoughtDef>)_ingestThoughts.GetValue(null);
        }

        // Token: 0x06000035 RID: 53 RVA: 0x000037FC File Offset: 0x000019FC
        internal static List<ThoughtDef> _ThoughtsFromIngesting(Pawn ingester, Thing t)
        {
            var ingestThoughts = IngestThoughts();
            ingestThoughts.Clear();

            if (
                (ingester.needs == null) ||
                (ingester.needs.mood == null)
            )
            {
                return ingestThoughts;
            }

            var mealDef = t.def;
            if (t is Building_AutomatedFactory)
            {
                mealDef = ((Building_AutomatedFactory)t).BestProduct(FoodSynthesis.IsMeal, FoodSynthesis.SortMeal);
            }
            else if (t is Building_NutrientPasteDispenser)
            {
                mealDef = ((Building_NutrientPasteDispenser)t).DispensableDef;
            }

            var corpse = t as Corpse;
            if (!ingester.story.traits.HasTrait(TraitDefOf.Ascetic))
            {
                if (mealDef.ingestible.preferability == FoodPreferability.MealLavish)
                {
                    ingestThoughts.Add(ThoughtDefOf.AteLavishMeal);
                    ingestThoughts.Add(ThoughtDefOfPsychology.AteLavishMealPickyEater);
                }
                else if (mealDef.ingestible.preferability == FoodPreferability.MealFine)
                {
                    ingestThoughts.Add(ThoughtDefOf.AteFineMeal);
                    ingestThoughts.Add(ThoughtDefOfPsychology.AteFineMealPickyEater);
                }
                else if (mealDef.ingestible.preferability == FoodPreferability.MealAwful)
                {
                    ingestThoughts.Add(ThoughtDefOf.AteAwfulMeal);
                    ingestThoughts.Add(ThoughtDefOfPsychology.AteAwfulMealPickyEater);
                }
                else if (mealDef.ingestible.tastesRaw)
                {
                    ingestThoughts.Add(ThoughtDefOf.AteRawFood);
                    ingestThoughts.Add(ThoughtDefOfPsychology.AteRawFoodPickyEater);
                }
                else if (corpse != null)
                {
                    ingestThoughts.Add(ThoughtDefOf.AteCorpse);
                }
                else if (ingester.story.traits.HasTrait(TraitDefOfPsychology.PickyEater))
                {
                    ingestThoughts.Add(ThoughtDefOfPsychology.AteNormalMealPickyEater);
                }
            }

            var isCannibal = ingester.story.traits.HasTrait(TraitDefOf.Cannibal);
            var comp = t.TryGetComp<CompIngredients>();
            if (
                (FoodUtility.IsHumanlikeMeat(mealDef)) &&
                (ingester.RaceProps.Humanlike)
            )
            {
                ingestThoughts.Add(!isCannibal ? ThoughtDefOf.AteHumanlikeMeatDirect : ThoughtDefOf.AteHumanlikeMeatDirectCannibal);
            }
            else if (comp != null)
            {
                for (int index = 0; index < comp.ingredients.Count; ++index)
                {
                    var ingredientDef = comp.ingredients[index];
                    if (ingredientDef.ingestible != null)
                    {
                        if (
                            (ingester.RaceProps.Humanlike) &&
                            (FoodUtility.IsHumanlikeMeat(ingredientDef))
                        )
                        {
                            ingestThoughts.Add(!isCannibal ? ThoughtDefOf.AteHumanlikeMeatAsIngredient : ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal);
                        }
                        else if (ingredientDef.ingestible.specialThoughtAsIngredient != null)
                        {
                            ingestThoughts.Add(ingredientDef.ingestible.specialThoughtAsIngredient);
                            if (ingredientDef.ingestible.specialThoughtAsIngredient == ThoughtDefOf.AteInsectMeatAsIngredient)
                                ingestThoughts.Add(ThoughtDefOfPsychology.AteInsectMeatAsIngredientPickyEater);
                        }
                    }
                }
            }
            else if (mealDef.ingestible.specialThoughtDirect != null)
            {
                ingestThoughts.Add(mealDef.ingestible.specialThoughtDirect);
                if (mealDef.ingestible.specialThoughtDirect == ThoughtDefOf.AteInsectMeatDirect)
                    ingestThoughts.Add(ThoughtDefOfPsychology.AteInsectMeatDirectPickyEater);
            }
            if (t.IsNotFresh())
            {
                ingestThoughts.Add(ThoughtDefOf.AteRottenFood);
                ingestThoughts.Add(ThoughtDefOfPsychology.AteRottenFoodPickyEater);
            }
            return ingestThoughts;
        }

        // Token: 0x04000015 RID: 21
        internal static FieldInfo _ingestThoughts;
    }
}
