using System.Linq;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality 
{
    /// <summary>
    /// Loaded via xml. Added so guests are upset when they can't afford a bed.
    /// </summary>
    public class ThoughtWorker_CantAffordBed : ThoughtWorkerCached
    {
        protected override bool ShouldCache(Pawn pawn)
        {
            if (pawn == null) return false;
            if (pawn.thingIDNumber == 0) return false; // What do you know!!!

            if (Current.ProgramState != ProgramState.Playing) return false;
            if (!pawn.IsArrivedGuest(out var compGuest)) return false;

            if(compGuest.rescued) return false;
            if(compGuest.HasBed) return false;
            return true;
        }

        protected override ThoughtState GetStateToCache(Pawn pawn)
        {
            var silver = pawn.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver);
            var money = silver?.stackCount ?? 0;

            var beds = pawn.MapHeld.GetGuestBeds(pawn.GetGuestArea()).ToArray();
            if(beds.Length == 0) return ThoughtState.Inactive;

            if (!beds.Any(bed => bed.AnyUnoccupiedSleepingSlot)) return ThoughtState.Inactive;
            if (beds.Any(bed => bed.RentalFee <= money && bed.AnyUnownedSleepingSlot)) return ThoughtState.Inactive;

            return ThoughtState.ActiveDefault;
        }
    }
}