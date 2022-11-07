using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hospitality.Utilities;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Hospitality {
    internal static class ItemUtility
    {
        private static readonly Dictionary<string, MethodInfo> alienFrameworkMethods = new Dictionary<string, MethodInfo>();

        public static float priceFactor = 0.55f;


        public static void PocketHeadgear(this Pawn pawn)
        {
            if (pawn?.apparel?.WornApparel == null || pawn.inventory?.innerContainer == null) return;
            // Don't do it when the pawn wants to stay anonymous
            if (ModsConfig.IdeologyActive && pawn.ideo?.Ideo?.HasPrecept(InternalDefOf.VME_Anonymity_Required) == true) return;

            var headgear = pawn.apparel.WornApparel.Where(CoversHead).ToArray();
            foreach (var apparel in headgear)
            {
                if (apparel == null) continue;
                // Don't drop headgear that is required by title
                if (IsRequiredByRoyalty(pawn, apparel.def)) continue;

                if (pawn.GetInventorySpaceFor(apparel) < 1) continue;

                if (pawn.apparel.TryDrop(apparel, out var droppedApp))
                {
                    var item = droppedApp?.SplitOff(1);
                    if (item != null)
                    {
                        pawn.inventory.TryAddItemNotForSale(item);
                        bool success = pawn.inventory.innerContainer.Contains(item);
                        if (!success) pawn.apparel.Wear(droppedApp);
                    }
                }
            }
        }

        public static bool IsRequiredByRoyalty(Pawn pawn, ThingDef apparelDef)
        {
            if (pawn.royalty == null) return false;
            try
            {
                return pawn.royalty.AllTitlesForReading.Any(title => title.def.requiredApparel != null && title.def.requiredApparel.Exists(req => req.ApparelMeetsRequirement(apparelDef)));
            }
            catch (Exception e)
            {
                Log.Error($"Failed to read royalty titles or their required apparel. This means you are using a mod that changes these and broke them.\n{e}");
                return false;
            }
        }

        public static bool CoversHead(this Apparel a)
        {
            return a.def.apparel.bodyPartGroups.Any(g => g == BodyPartGroupDefOf.UpperHead || g == BodyPartGroupDefOf.FullHead);
        }

        public static void WearHeadgear(this Pawn pawn)
        {
            if (pawn?.apparel?.WornApparel == null || pawn.inventory?.innerContainer == null) return;

            var container = pawn.inventory.innerContainer;
            var headgear = container.OfType<Apparel>().Where(CoversHead).InRandomOrder().ToArray();
            foreach (var apparel in headgear)
            {
                if (pawn.apparel.CanWearWithoutDroppingAnything(apparel.def))
                {
                    container.Remove(apparel);
                    pawn.apparel.Wear(apparel);
                }
            }
        }

        public static void TryGiveBackpack(this Pawn p)
        {
            var def = InternalDefOf.Apparel_Backpack;
            if (def == null) return;

            if (p.inventory.innerContainer.Contains(def)) return;

            ThingDef stuff = GenStuff.RandomStuffFor(def);
            var item = (Apparel) ThingMaker.MakeThing(def, stuff);
            item.stackCount = 1;
            p.apparel.Wear(item, false);
        }

        #region Combat Extended integration
        private static MethodBase canFitInInventory;

        public static int GetInventorySpaceFor(this Pawn pawn, Thing current)
        {
            if (pawn == null || current == null) return 0;

            var inventory = pawn.GetInventory();
            if (inventory == null) return current.stackCount;

            object[] arguments = {current, 0, false, true};

            try
            {
                canFitInInventory ??= AccessTools.GetDeclaredMethods(inventory.GetType()).First(x => x.Name == "CanFitInInventory" && x.GetParameters()[0].ParameterType == typeof(Thing));

                if (canFitInInventory == null)
                {
                    Log.ErrorOnce($"CanFitInInventory not found.", 4363476);
                    return current.stackCount;
                }
                var success = (bool) canFitInInventory.Invoke(inventory, arguments);
                if (!success) return 0;
                return (int) arguments[1];
            }
            catch (Exception e)
            {
                Log.Error(e.StackTrace);
                return current.stackCount;
            }
        }

        private static ThingComp GetInventory(this Pawn pawn)
        {
            return pawn.AllComps.FirstOrDefault(c => c.GetType().Name == "CompInventory");
        }
        #endregion

        public static int GetMoney(this Pawn pawn)
        {
            var money = pawn.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver);
            return money?.stackCount ?? 0;
        }

        public static bool IsIngestible(this Thing thing)
        {
            return thing.def.IsIngestible && thing.def.ingestible.preferability != FoodPreferability.RawBad && thing.def.ingestible.preferability != FoodPreferability.MealAwful;
        }

        public static bool IsFood(this Thing thing)
        {
            return thing.def.ingestible != null && thing.def.ingestible.preferability != FoodPreferability.NeverForNutrition && thing.def.ingestible.preferability != FoodPreferability.DesperateOnlyForHumanlikes
                   && thing.def.ingestible.preferability != FoodPreferability.DesperateOnly;
        }

        /// <summary>
        /// methodName = "CanWear", "CanEat" or "CanEquip"
        /// </summary>
        public static bool AlienFrameworkAllowsIt(ThingDef raceDef, ThingDef thingDef, [NotNull] string methodName)
        {
            if (!alienFrameworkMethods.TryGetValue(methodName, out var method))
            {
                var type = GenTypes.GetTypeInAnyAssembly("AlienRace.RaceRestrictionSettings");
                if (type != null)
                {
                    method = type.GetMethod(methodName, new[] {typeof(ThingDef), typeof(ThingDef)});
                    if (method == null) Log.Error($"Alien Framework does not have a method '{methodName}'.");
                }

                alienFrameworkMethods.Add(methodName, method); // we add it as null if not found, so it will return true
            }

            return method == null || (bool) method.Invoke(null, new object[] {thingDef, raceDef});
        }

        public static bool IsBuyableAtAll(Pawn pawn, Thing thing)
        {
            if (thing.def.isUnfinishedThing) return false;

            if (thing.def == ThingDefOf.Silver) return false;

            if (!pawn.MayPurchaseThing(thing)) return false;

            if (thing.def.thingSetMakerTags != null && thing.def.thingSetMakerTags.Contains("NotForGuests")) return false;

            if (!IsBuyableNow(pawn, thing)) return false;
            //if (!thing.IsSociallyProper(pawn))
            //{
            //    Log.Message(thing.Label + ": is not proper for " + pawn.NameStringShort);
            //    return false;
            //}
            var cost = Mathf.CeilToInt(GetPurchasingCost(thing));
            
            if (cost > GetMoney(pawn))
            {
                return false;
            }

            if (BoughtByPlayer(pawn, thing))
            {
                return false;
            }

            //if (thing.IsInValidStorage()) Log.Message(thing.Label + " in storage ");
            return true;
        }

        public static float GetPurchasingCost([NotNull]this Thing thing)
        {
            if (IsFood(thing) && thing.GetMapComponent().guestsCanTakeFoodForFree) return 0;
            return thing.MarketValue * priceFactor;
        }

        private static bool BoughtByPlayer(Pawn pawn, Thing thing)
        {
            var lord = pawn.GetLord();
            return !(lord?.CurLordToil is LordToil_VisitPoint toil) || toil.BoughtOrSoldByPlayer(thing);
        }

        public static bool IsBuyableNow(Pawn pawn, Thing thing)
        {
            if (!thing.SpawnedOrAnyParentSpawned)
            {
                return false;
            }

            if (thing.ParentHolder is Pawn)
            {
                //Log.Message(thing.Label+": is inside pawn "+pawn.NameStringShort);
                return false;
            }

            if (thing.IsForbidden(Faction.OfPlayer))
            {
                //Log.Message(thing.Label+": is forbidden for "+pawn.NameStringShort);
                return false;
            }

            if (!pawn.HasReserved(thing) && !pawn.CanReserve(thing))
            {
                //Log.Message(thing.Label+": can't be reserved or reached by "+pawn.NameStringShort);
                return false;
            }

            if (pawn.GetInventorySpaceFor(thing) < 1)
            {
                return false;
            }

            return true;
        }

        public static bool MayPurchaseThing(this ITrader guestTrader, Thing thing)
        {
            if (thing == null || guestTrader == null) return false;
            if (thing.def.tradeability.PlayerCanSell()) return true;
            return guestTrader.IsGuestTrader() && thing.def.thingCategories?.Contains(ThingCategoryDefOf.FoodMeals) == true;
        }
    }
}
