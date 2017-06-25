using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace QOLTweaksPack.tweaks
{

    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredientsInSet")]
    static class WorkGiver_DoBill_TryFindBestBillIngredientsInSet_Postfix
    {
        [HarmonyPostfix]
        private static void TryFindBestBillIngredientsInSet(ref bool __result, List<Thing> availableThings, Bill bill, List<ThingAmount> chosen)
        {
            if (QOLTweaksPack.ButcherSpace.Value == false)
                return;

            if (bill.recipe.specialProducts != null)
            {
                foreach (SpecialProductType specialProductType in bill.recipe.specialProducts)
                {
                    if (specialProductType == SpecialProductType.Butchery)
                    {
                        TryFindBestBillIngredientsInSetButchery(ref __result, availableThings, bill, chosen);
                        return;
                    }
                }
            }

            List<ThingCountClass> rottableProducts = new List<ThingCountClass>();
            if (bill.recipe.products != null)
            {
                foreach (ThingCountClass product in bill.recipe.products)
                {
                    if (product.thingDef.HasComp(typeof(CompRottable)))
                    {
                        rottableProducts.Add(product);
                    }
                }
            }

            TryFindBestBillIngredientsInSetRottable(ref __result, availableThings, bill, chosen, rottableProducts);
        }
        
        private static void TryFindBestBillIngredientsInSetButchery(ref bool __result, List<Thing> availableThings, Bill bill, List<ThingAmount> chosen)
        {
            if (QOLTweaksPack.DeadlockProtection.Value == true && bill.Map.resourceCounter.GetCountIn(ThingCategoryDefOf.MeatRaw) < 10)
                return;

            if (chosen.Count < 1 || chosen.Count > 1 || chosen[0].count != 1)
            {
                return;
            }
            Thing corpse = chosen[0].thing;

            if (corpse as Corpse == null)
                return;

            if (!CanStoreCount(bill.Map, MeatDefFor(corpse as Corpse), EstimatedMeatCount(corpse as Corpse)))
            {
                __result = false;
                chosen.Clear();

                List<ThingDef> testedRaces = new List<ThingDef>();
                testedRaces.Add((corpse as Corpse).InnerPawn.def);

                foreach(Thing alternativeCorpse in availableThings)
                {
                    if (testedRaces.Contains((alternativeCorpse as Corpse).InnerPawn.def))
                        continue;
                    else
                        testedRaces.Add((alternativeCorpse as Corpse).InnerPawn.def);

                    if (alternativeCorpse as Corpse == null)
                        continue;

                    if (CanStoreCount(bill.Map, MeatDefFor(alternativeCorpse as Corpse), EstimatedMeatCount(alternativeCorpse as Corpse)))
                    {
                        __result = true;
                        chosen.Add(new ThingAmount(alternativeCorpse, 1));
                        return;
                    }
                }
            }
        }

        private static bool CanStoreCount(Map map, ThingDef thingDef, int requiredAmount)
        {
            List<SlotGroup> storageList = map.slotGroupManager.AllGroupsListInPriorityOrder;
            List<SlotGroup> storageFiltered = new List<SlotGroup>();

            int total = 0;

            foreach (SlotGroup storage in storageList)
            {
                if (StorageAllowedToAccept(storage.Settings, thingDef))
                {
                    foreach(IntVec3 cell in storage.CellsList)
                    {
                        int local = MaximumStorageInCellFor(cell, map, thingDef);
                        total += local;
                        if (total >= requiredAmount)
                            return true;
                    }
                }
            }
            return false;
        }

        private static int MaximumStorageInCellFor(IntVec3 c, Map map, ThingDef thing)
        {
            List<Thing> list = map.thingGrid.ThingsListAt(c);
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing2 = list[i];
                if (thing2.def.EverStoreable)
                {
                    if (!CanStackWith(thing, thing2.def))
                    {
                        return 0;
                    }
                    if (thing2.stackCount >= thing.stackLimit)
                    {
                        return 0;
                    }
                    else
                    {
                        return thing.stackLimit - thing2.stackCount;
                    }
                }
                if (thing2.def.entityDefToBuild != null && thing2.def.entityDefToBuild.passability != Traversability.Standable)
                {
                    return 0;
                }
                if (thing2.def.surfaceType == SurfaceType.None && thing2.def.passability != Traversability.Standable)
                {
                    return 0;
                }
            }
            return thing.stackLimit;
        }

        private static bool CanStackWith(ThingDef thing, ThingDef other)
        {
            return thing == other;
        }

        private static ThingDef MeatDefFor(Corpse corpse)
        {
            return (corpse as Corpse).InnerPawn.RaceProps.meatDef;
        }

        private static int EstimatedMeatCount(Corpse corpse)
        {
            int meatCount = (int)corpse.InnerPawn.GetStatValue(StatDefOf.MeatAmount, true);
            return meatCount;
        }

        private static void TryFindBestBillIngredientsInSetRottable(ref bool __result, List<Thing> availableThings, Bill bill, List<ThingAmount> chosen, List<ThingCountClass> rottableProducts)
        {
            if(QOLTweaksPack.DeadlockProtection.Value == true)
            {
                foreach (ThingCountClass product in rottableProducts)
                {
                    if (bill.Map.resourceCounter.GetCount(product.thingDef) < 1)
                        return;
                }
            }            

            foreach (ThingCountClass product in rottableProducts)
            {
                if (!CanStoreCount(bill.Map, product.thingDef, product.count))
                {
                    __result = false;
                    chosen.Clear();
                    return;
                }
            }
        }
        
        private static bool StorageAllowedToAccept(StorageSettings storage, ThingDef def)
        {
            if (!storage.filter.Allows(def))
            {
                return false;
            }
            if (storage.owner != null)
            {
                StorageSettings parentStoreSettings = storage.owner.GetParentStoreSettings();
                if (parentStoreSettings != null && !StorageAllowedToAccept(parentStoreSettings, def))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
