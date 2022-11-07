using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality
{
    /// <summary>
    /// Loaded via xml. Added so guests are upset when they have nothing to eat.
    /// </summary>
    public class ThoughtWorker_HasNoFood : ThoughtWorkerCached
    {
        public override int ThoughtCacheInterval => GenTicks.TickLongInterval;

        protected override ThoughtState GetStateToCache(Pawn pawn)
        {
            return Utilities.FoodUtility.GuestCanSatisfyFoodNeed(pawn) ? ThoughtState.Inactive : ThoughtState.ActiveDefault;
        }

        protected override bool ShouldCache(Pawn pawn)
        {
            if (pawn == null) return false;
            if (pawn.thingIDNumber == 0) return false; // What do you know!!!

            if (Current.ProgramState != ProgramState.Playing)
            {
                return false;
            }
            if (!pawn.IsArrivedGuest(out var compGuest)) return false;
            if (compGuest == null) return false;
            if (!compGuest.arrived) return false;
            return true;
        }
    }
}
