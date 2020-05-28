using System;

using RimWorld;
using Verse;
using Verse.AI;

namespace SeedsPlease
{
    public class WorkGiver_GrowerSowWithSeeds : WorkGiver_GrowerSow
    {
        const int SEEDS_TO_CARRY = 25;

    	public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        {
            var job = base.JobOnCell (pawn, c, forced);

            // plant has seeds, if there is a seed return a job, otherwise prevent it. Seeds with no category are forbidden.
            var seed = job?.plantDefToSow?.blueprintDef;
            if (seed != null && !seed.thingCategories.NullOrEmpty())
            {
                // Clear the area some...
                var zone = c.GetZone (pawn.Map);
                if (zone != null) {
                    foreach (var corner in GenAdj.AdjacentCells8WayRandomized ()) {
                        var cell = c + corner;
                        if (zone.ContainsCell (cell)) {
                            foreach (var thing in pawn.Map.thingGrid.ThingsAt (cell)) {
                                if (thing.def != job.plantDefToSow && thing.def.BlockPlanting && pawn.CanReserve (thing) && !thing.IsForbidden (pawn)) {
                                    if (thing.def.category == ThingCategory.Plant) {
                                        return new Job(JobDefOf.CutPlant, thing);
                                    }
                                    if (thing.def.EverHaulable) {
                                        return HaulAIUtility.HaulAsideJobFor(pawn, thing);
                                    }
                                }
                            }
                        }
                    }
                }

                Predicate<Thing> predicate = (Thing tempThing) =>
                    !ForbidUtility.IsForbidden (tempThing, pawn.Faction)
                    && ForbidUtility.InAllowedArea(tempThing.Position, pawn)
                    && PawnLocalAwareness.AnimalAwareOf (pawn, tempThing)
                    && ReservationUtility.CanReserve (pawn, tempThing, 1);

                Thing bestSeedThingForSowing = GenClosest.ClosestThingReachable (
                    c, pawn.Map, ThingRequest.ForDef (job.plantDefToSow.blueprintDef),
                    PathEndMode.ClosestTouch, TraverseParms.For (pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999,
                    predicate);

                if (bestSeedThingForSowing != null) {
                    return new Job (ResourceBank.JobDefOf.SowWithSeeds, c, bestSeedThingForSowing) {
                        plantDefToSow = job.plantDefToSow,
                        count = SEEDS_TO_CARRY
                    };
                }
                return null;
            }

            return job;
        }
    }
}
