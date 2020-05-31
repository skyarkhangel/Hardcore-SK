using System.Linq;
using RimWorld;
using Verse;

namespace Hospitality {
    /// <summary>
    /// Loaded via xml. Added so are upset when they can't afford a bed.
    /// </summary>
    public class ThoughtWorker_CantAffordBed : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            if (pawn == null) return ThoughtState.Inactive;
            if (pawn.thingIDNumber == 0) return ThoughtState.Inactive; // What do you know!!!

            if (Current.ProgramState != ProgramState.Playing)
            {
                return ThoughtState.Inactive;
            }
            if (!pawn.IsArrivedGuest()) return ThoughtState.Inactive;

            var compGuest = pawn.CompGuest();
            if (compGuest == null) return ThoughtState.Inactive;
            if(!compGuest.arrived) return ThoughtState.Inactive;

            if(compGuest.rescued) return ThoughtState.Inactive;
            if(compGuest.HasBed) return ThoughtState.Inactive;
            
            var silver = pawn.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver);
            var money = silver?.stackCount ?? 0;

            var beds = pawn.MapHeld.GetGuestBeds(pawn.GetGuestArea()).ToArray();
            if(beds.Length == 0) return ThoughtState.Inactive;

            return !beds.Any(bed => bed.rentalFee <= money && bed.AnyUnownedSleepingSlot);
        }
    }
}