using System.Collections.Generic;
using System.Linq;
using Gastronomy.Dining;
using RimWorld;
using Verse;
using Verse.AI;

namespace Gastronomy.Waiting
{
    public class WorkGiver_MakeTable : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => pawn.GetRestaurant().diningSpots;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            var list = pawn.GetRestaurant().diningSpots;

            return list.Count == 0;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is DiningSpot spot)) return false;
            if (RestaurantUtility.IsRegionDangerous(pawn, spot.GetRegion()) && !forced) return false;
            if (spot.GetReservationSpots().Any(s => s == SpotState.Clear || s > SpotState.Ready))
            {
                var canReserve = pawn.Map.reservationManager.CanReserve(pawn, spot);
                if (!canReserve)
                {
                    //var reserver = pawn.Map.reservationManager.FirstRespectedReserver(spot, pawn);
                    //Log.Message($"{pawn.NameShortColored} can't reserve {spot.Position} for making. Is reserved by {reserver?.NameShortColored}. ");
                    return false;
                }

                //Log.Message($"{pawn.NameShortColored} can make table at {spot.Position}.");
                return true;
            }

            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var diningSpot = (DiningSpot) t;

            //Log.Message($"{pawn.NameShortColored} can get a make table job at {diningSpot.Position}.");

            return JobMaker.MakeJob(WaitingUtility.makeTableDef, diningSpot);
        }
    }
}
