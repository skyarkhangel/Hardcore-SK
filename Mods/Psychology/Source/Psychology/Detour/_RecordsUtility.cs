using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Verse;
using RimWorld;

namespace Psychology.Detour
{
    // Token: 0x0200031E RID: 798
    internal static class _RecordsUtility
    {
        // Token: 0x06000C57 RID: 3159 RVA: 0x0003C614 File Offset: 0x0003A814
        internal static void _Notify_BillDone(Pawn billDoer, List<Thing> products)
        {
            for (int i = 0; i < products.Count; i++)
            {
                var ShouldIncrementThingsCrafted = typeof(RecordsUtility).GetMethod("ShouldIncrementThingsCrafted", BindingFlags.Static | BindingFlags.NonPublic);
                if (products[i].def.IsNutritionGivingIngestible && products[i].def.ingestible.preferability >= FoodPreferability.MealAwful)
                {
                    billDoer.records.Increment(RecordDefOf.MealsCooked);
                    billDoer.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.CookedMealBleedingHeart, (Pawn)null);
                }
                else if (ShouldIncrementThingsCrafted != null && (bool)ShouldIncrementThingsCrafted.Invoke(null, new object[] { products[i] }))
                {
                    billDoer.records.Increment(RecordDefOf.ThingsCrafted);
                }
                else if (ShouldIncrementThingsCrafted == null)
                    Log.ErrorOnce("Unable to reflect RecordsUtility.ShouldIncrementThingsCrafted!", 305432421);
            }
        }
    }
}
