using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace QOLTweaksPack.utilities
{
    static class ItemCounter
    {
        internal static int CountProductsWithEquivalency(Bill_Production bill, RecipeWorkerCounter counter)
        {
            HashSet<string> eqNames = QOLTweaksPack.MealSelection.Value.InnerList;

            ThingCountClass thingCountClass = counter.recipe.products[0];
            //Log.Message("Looking for equivalencies to " + thingCountClass.thingDef);
            if (thingCountClass.thingDef.CountAsResource)
            {
                int total = 0;
                foreach (string eqName in eqNames)
                {
                    ThingDef def = DefDatabase<ThingDef>.GetNamed(eqName);
                    int local = bill.Map.resourceCounter.GetCount(def);
                    total += local;
                    //Log.Message("Counted " + local + " of " + def.defName);
                }
                return total;
            }
            int num = bill.Map.listerThings.ThingsOfDef(thingCountClass.thingDef).Count;
            if (thingCountClass.thingDef.Minifiable)
            {
                List<Thing> list = bill.Map.listerThings.ThingsInGroup(ThingRequestGroup.MinifiedThing);
                for (int i = 0; i < list.Count; i++)
                {
                    MinifiedThing minifiedThing = (MinifiedThing)list[i];
                    if (eqNames.Contains(minifiedThing.InnerThing.def.defName))
                    {
                        num++;
                    }
                }
            }
            return num;
        }
    }
}
