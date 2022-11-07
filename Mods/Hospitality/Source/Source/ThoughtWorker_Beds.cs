using System;
using System.Linq;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality
{
    /// <summary>
    /// Loaded via xml. Added so guests want beds.
    /// </summary>
    public class ThoughtWorker_Beds : ThoughtWorkerCached
    {
        protected override bool ShouldCache(Pawn pawn)
        {
            try
            {
                if (pawn == null) return false;
                if (pawn.thingIDNumber == 0) return false; // What do you know!!!

                if (Current.ProgramState != ProgramState.Playing) return false;

                if (!pawn.IsArrivedGuest(out var compGuest)) return false;

                if (compGuest == null) return false;
                if (!compGuest.arrived) return false;
                if (compGuest.rescued) return false;
                return true;
            }
            catch (Exception e)
            {
                Log.Warning(e.Message);
                return false;
            }
        }

        protected override ThoughtState GetStateToCache(Pawn pawn)
        {
            try
            {
                var compGuest = pawn.CompGuest();
                var area = pawn.GetGuestArea();

                var bedCount = pawn.MapHeld.GetGuestBeds(pawn.GetGuestArea()).Count(b => b?.def.useHitPoints == true); // Sleeping spots don't count
                if (bedCount == 0) return ThoughtState.ActiveAtStage(0);

                var visitors = pawn.GetMapComponent().PresentGuests.Count(p => StaysInArea(p, area));

                if (bedCount < visitors && !compGuest.HasBed) return ThoughtState.ActiveAtStage(1);
                if(bedCount > visitors*1.3f && bedCount > visitors+3) return ThoughtState.ActiveAtStage(3);

                return ThoughtState.ActiveAtStage(2);
            }
            catch(Exception e)
            {
                Log.Warning(e.Message);
                return ThoughtState.Inactive;
            }
        }

        private static bool StaysInArea(Pawn pawn, Area area)
        {
            if (pawn == null) return false;

            var comp = pawn.CompGuest();
            return comp?.arrived == true && comp.GuestArea == area;
        }
    }
}