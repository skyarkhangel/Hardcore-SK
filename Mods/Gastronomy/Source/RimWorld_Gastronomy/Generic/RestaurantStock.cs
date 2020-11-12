using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Gastronomy.Dining;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Gastronomy
{
    public class RestaurantStock : IExposable
    {
        public class Stock
        {
            public ThingDef def;
            public int ordered;
            [NotNull] public readonly List<Thing> items = new List<Thing>();
        }

        private const float MinOptimality = 100f;
        private const int JoyOptimalityWeight = 300;

        private class ConsumeOptimality
        {
            public Pawn pawn;
            public ThingDef def;
            public float value;
        }

        //[NotNull] private readonly List<Thing> stockCache = new List<Thing>();
        [NotNull] private readonly List<ConsumeOptimality> eatOptimalityCache = new List<ConsumeOptimality>();
        [NotNull] private readonly List<ConsumeOptimality> joyOptimalityCache = new List<ConsumeOptimality>();
        [NotNull] private Map Map => Restaurant.map;
        [NotNull] private RestaurantMenu Menu => Restaurant.Menu;
        [NotNull] private RestaurantController Restaurant { get; }
        [NotNull] private readonly Dictionary<ThingDef, Stock> stockCache = new Dictionary<ThingDef,Stock>();
        [NotNull] public IReadOnlyDictionary<ThingDef, Stock> AllStock => stockCache;

        public RestaurantStock([NotNull] RestaurantController restaurant)
        {
            Restaurant = restaurant;
        }

        public void ExposeData() { }

        public bool HasAnyFoodFor([NotNull] Pawn pawn, bool allowDrug)
        {
            //Log.Message($"{pawn.NameShortColored}: HasFoodFor: Defs: {stock.Select(item=>item.def).Count(s => WillConsume(pawn, allowDrug, s))}");
            return stockCache.Keys.Any(def => WillConsume(pawn, allowDrug, def));
        }

        public class DefOptimality
        {
            public ThingDef def { get; }
            public float optimality { get; }

            public DefOptimality(ThingDef def, float optimality)
            {
                this.def = def;
                this.optimality = optimality;
            }
        }

        public ThingDef GetBestMealTypeFor([NotNull] Pawn pawn, bool allowDrug, bool includeEat = true, bool includeJoy = true)
        {
            if (GetMealOptions(pawn, allowDrug, includeEat, includeJoy)
                .TryMaxBy(def => def.optimality, out var best))
            {
                //Log.Message($"{pawn.NameShortColored}: GetBestFoodFor: {best?.label}");
                return best.def;
            }
            return null;
        }

        public ThingDef GetRandomMealTypeFor([NotNull] Pawn pawn, bool allowDrug, bool includeEat = true, bool includeJoy = true)
        {
            if (GetMealOptions(pawn, allowDrug, includeEat, includeJoy)
                .TryRandomElementByWeight(def => def.optimality, out var random))
            {
                //Log.Message($"{pawn.NameShortColored} picked {random.def.label} with a score of {random.optimality}");
                return random.def;
            }
            return null;
        }

        private IEnumerable<DefOptimality> GetMealOptions(Pawn pawn, bool allowDrug, bool includeEat, bool includeJoy)
        {
            return stockCache.Keys
                .Where(def => Restaurant.Orders.CanBeOrdered(def))
                .Where(def => WillConsume(pawn, allowDrug, def))
                .Where(def => CanAfford(pawn, def))
                .Select(def => new DefOptimality(def, GetMealOptimalityScore(pawn, def, includeEat, includeJoy)))
                .Where(def => def.optimality >= MinOptimality);
        }

        private bool CanAfford(Pawn pawn, ThingDef def)
        {
            if (Restaurant.guestPricePercentage <= 0) return true;
            if (!pawn.IsGuest()) return true;
            return pawn.GetSilver() >= def.GetPrice(Restaurant);
        }

        private float GetMealOptimalityScore(Pawn pawn, ThingDef def, bool includeEat = true, bool includeJoy = true)
        {
            if (!IsAllowedIfDrug(pawn, def)) return 0;

            float score = 0;
            //var debugMessage = new StringBuilder($"{pawn.NameShortColored}: {def.LabelCap} ");
            if (includeEat && pawn.needs.food != null)
            {
                var optimality = GetCachedOptimality(pawn, def, eatOptimalityCache, CalcEatOptimality);
                var factor = NutritionVsNeedFactor(pawn, def);
                score += optimality * factor;
                //debugMessage.Append($"EAT = {optimality:F0} * {factor:F2} ");
            }

            if (includeJoy && pawn.needs.joy != null)
            {
                var optimality = GetCachedOptimality(pawn, def, joyOptimalityCache, CalcJoyOptimality);
                var factor = JoyVsNeedFactor(pawn, def);
                score += optimality * factor;
                //debugMessage.Append($"JOY = {optimality:F0} * {factor:F2} ");
            }

            //debugMessage.Append($"= {score:F0}");
            //Log.Message(debugMessage.ToString());
            return score;
        }

        private static float CalcEatOptimality(Pawn pawn, ThingDef def)
        {
            return Mathf.Max(0, FoodUtility.FoodOptimality(pawn, null, def, 0));
        }

        private static float CalcJoyOptimality(Pawn pawn, ThingDef def)
        {
            var toleranceFactor = pawn.needs.joy.tolerances.JoyFactorFromTolerance(def.ingestible.JoyKind);
            var drugCategoryFactor = GetDrugCategoryFactor(def);
            return toleranceFactor * drugCategoryFactor * JoyOptimalityWeight;
        }

        private static float GetDrugCategoryFactor(ThingDef def)
        {
            return def.ingestible.drugCategory switch
            {
                DrugCategory.None => 3.5f,
                DrugCategory.Social => 3.0f,
                DrugCategory.Medical => 1.5f,
                _ => 1.0f
            };
        }

        private static bool IsAllowedIfDrug(Pawn pawn, ThingDef def)
        {
            if (!def.IsDrug) return true;
            if (pawn.drugs == null) return true;
            if (pawn.InMentalState) return true;
            if (pawn.IsTeetotaler()) return false;
            if (pawn.story?.traits.DegreeOfTrait(TraitDefOf.DrugDesire) > 0) return true; // Doesn't care about schedule no matter the schedule
            var drugPolicyEntry = pawn.GetPolicyFor(def);
            //Log.Message($"{pawn.NameShortColored} vs {def.label} as drug: for joy = {drugPolicyEntry?.allowedForJoy}");
            if (drugPolicyEntry?.allowedForJoy == false) return false;
            return true;
        }

        private static float GetCachedOptimality(Pawn pawn, ThingDef def, [NotNull] List<ConsumeOptimality> optimalityCache, [NotNull] Func<Pawn, ThingDef, float> calcFunction)
        {
            // Expensive, must be cached
            var optimality = optimalityCache.FirstOrDefault(o => o.pawn == pawn && o.def == def);
            if (optimality == null)
            {
                // Optimality can be negative
                optimality = new ConsumeOptimality {pawn = pawn, def = def, value = calcFunction(pawn, def)};
                optimalityCache.Add(optimality);
            }
            // From 0 to 300-400ish
            return optimality.value;
        }

        private static float NutritionVsNeedFactor(Pawn pawn, ThingDef def)
        {
            var need = pawn.needs.food?.NutritionWanted ?? 0;
            if (need < 0.1f) return 0;
            var provided = def.ingestible.CachedNutrition;
            if (provided < 0.01f) return 0;
            var similarity = 1 - Mathf.Abs(need - provided) / need;
            var score = Mathf.Max(0, need * similarity);
            //Log.Message($"{pawn.NameShortColored}: {def.LabelCap} EAT Need = {need:F2} Provided = {provided:F2} Similarity = {similarity:F2} Score = {score:F2}");
            return score;
        }

        private static float JoyVsNeedFactor(Pawn pawn, ThingDef def)
        {
            var need = 1 - pawn.needs.joy?.CurLevelPercentage ?? 0;
            if (def.ingestible.joyKind == null) return 0;
            var score = def.ingestible.joy * need;
            //Log.Message($"{pawn.NameShortColored}: {def.LabelCap} JOY Need = {need:F2} Provided = {def.ingestible.joy:F2} Score = {score:F2}");
            return score;
        }

        private static bool WillConsume(Pawn pawn, bool allowDrug, ThingDef s)
        {
            var result = s != null && (allowDrug || !s.IsDrug) && pawn.WillEat(s);
            //Log.Message($"{pawn.NameShortColored} will consume {s.label}? will eat? {pawn.WillEat(s)} result = {result}");
            return result;
        }

        public Thing GetServableThing(Order order, Pawn pawn)
        {
            if (stockCache.TryGetValue(order.consumableDef, out var stock))
            {
                return stock.items.OrderBy(o => pawn.Position.DistanceToSquared(o.Position))
                    .FirstOrDefault(o => pawn.CanReserveAndReach(o, PathEndMode.Touch, Danger.None, o.stackCount, 1));
            }
            return null;
        }

        public void RareTick()
        {
            // Refresh entire stock
            foreach (var stock in stockCache)
            {
                stock.Value.items.Clear();
                stock.Value.ordered = 0;
            }

            foreach (var thing in Map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSource)
                .Where(t => t.def.IsIngestible && !t.def.IsCorpse && Menu.IsOnMenu(t)))
            {
                if (thing?.def == null) continue;
                if (!stockCache.TryGetValue(thing.def, out var stock))
                {
                    stock = new Stock {def = thing.def};
                    stockCache.Add(thing.def, stock);
                }

                stock.items.Add(thing);
            }

            // Slowly empty optimality caches again
            if (eatOptimalityCache.Count > 0) eatOptimalityCache.RemoveAt(0);
            if (joyOptimalityCache.Count > 0) joyOptimalityCache.RemoveAt(0);
        }

        [NotNull]
        public IReadOnlyCollection<Thing> GetAllStockOfDef(ThingDef def)
        {
            if (!stockCache.TryGetValue(def, out var stock)) return new Thing[0];
            return stock.items;
        }
    }
}
