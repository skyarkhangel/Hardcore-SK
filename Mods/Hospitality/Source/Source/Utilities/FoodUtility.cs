using System.Linq;
using RimWorld;
using Verse;

namespace Hospitality.Utilities
{
    public static class FoodUtility
    {
        
        public static bool GuestCanSatisfyFoodNeed(Pawn guest)
        {
            Rand.PushState(Find.TickManager.TicksAbs);

            //Search FoodSource
#pragma warning disable CS0612 // Type or member is obsolete
            if (RimWorld.FoodUtility.TryFindBestFoodSourceFor(guest, guest, false, out var foodSource, out var foodDef, false, true, false, false, false, false, false, false, false, false, FoodPreferability.RawTasty))
#pragma warning restore CS0612 // Type or member is obsolete
            {
                if (foodSource != null && foodDef != null)
                {
                    Rand.PopState();
                    return true;
                }
            }

            Rand.PopState();
            return false;
        }

        public static bool GuestCanUseFoodSourceInternal(Pawn guest, Thing foodSource)
        {
            //Log.Message($"Checking FoodSource for {guest.NameShortColored}: {foodSource?.LabelCap} ({foodSource?.Position})");

            //We need to get current status data of the guest
            var foodDef = RimWorld.FoodUtility.GetFinalIngestibleDef(foodSource, true);
            var desperate = guest.needs.food?.CurCategory == HungerCategory.Starving;

            //Log.Message($"FooDef: {foodDef?.LabelCap}| Desperate: {desperate}");
            //If they are starving, they simply take the next best food source
            if (desperate || guest.GetMapComponent().guestsCanTakeFoodForFree)
            {
                return true;
            }
            
            if (!WillConsume(guest, foodDef)) return false;

            //Check whether the current food source is a dispenser set as a vending machine for this guest
            //Log.Message($"Dispenser: {foodSource is Building_NutrientPasteDispenser}| CanBeUsed: {(foodSource.TryGetComp<CompVendingMachine>()?.CanBeUsedBy(guest, foodDef) ?? false)}");
            if (foodSource is Building_NutrientPasteDispenser dispenser && (dispenser.TryGetComp<CompVendingMachine>()?.CanBeUsedBy(guest) ?? false))
            {
                return true;
            }
            return false;
        }

        //This method is meant to be extended as we find new foodsources that go past the check in 'GuestCanUseFoodSourceInternal'
        public static bool GuestCanUseFoodSourceExceptions(Pawn guest, Thing foodSource, ThingDef foodDef, bool desperate)
        {
            return true;
        }

        public static bool TryPayForFood(Pawn buyerGuest, Building_NutrientPasteDispenser dispenser)
        {
            var vendingMachine = dispenser.TryGetComp<CompVendingMachine>();
            if (vendingMachine.IsActive() && dispenser.CanDispenseNow)
            {
                if (vendingMachine.IsFree) return true;

                if (!vendingMachine.CanAffordFast(buyerGuest, out Thing silver)) return false;

                vendingMachine.ReceivePayment(buyerGuest.inventory.innerContainer, silver);

                var marketValueRate = vendingMachine.CurrentPrice / dispenser.DispensableDef.BaseMarketValue;
                if (marketValueRate >= 1.25f)
                {
                    buyerGuest.needs.mood.thoughts.memories.TryGainMemory(InternalDefOf.GuestExpensiveFood);
                }
                else if (marketValueRate <= 0.75f)
                {
                    buyerGuest.needs.mood.thoughts.memories.TryGainMemory(InternalDefOf.GuestCheapFood);
                }
                return true;
            }
            return false;
        }

        public static bool WillConsume(Pawn pawn, ThingDef foodDef)
        {
            if (foodDef == null) return false;
            var restrictions = pawn.foodRestriction.CurrentFoodRestriction;

            if (!restrictions.Allows(foodDef)) return false;

            var fineAsFood = foodDef.ingestible?.preferability == FoodPreferability.Undefined || foodDef.ingestible?.preferability == FoodPreferability.NeverForNutrition || pawn.WillEat(foodDef);
            return !foodDef.IsDrug && fineAsFood;
        }
    }
}
