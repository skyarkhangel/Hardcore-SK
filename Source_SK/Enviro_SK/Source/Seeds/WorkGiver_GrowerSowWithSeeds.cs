using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace SK_Enviro.Seeds
{
    internal class WorkGiver_GrowerSowWithSeeds : WorkGiver_GrowerSow
    {
        public override Job JobOnCell(Pawn pawn, IntVec3 cell)
        {
            Job job = base.JobOnCell(pawn, cell);
            if (job != null && job.plantDefToSow != null)
            {
                ThingDef_PlantWithSeeds thingDef_PlantWithSeeds = job.plantDefToSow as ThingDef_PlantWithSeeds;
                if (thingDef_PlantWithSeeds != null)
                {
                    Thing bestSeedThingForSowing = this.GetBestSeedThingForSowing(pawn, cell, thingDef_PlantWithSeeds);
                    if (bestSeedThingForSowing != null)
                    {
                        return new Job(DefDatabase<JobDef>.GetNamed("SowWithSeeds", true), cell, bestSeedThingForSowing)
                        {
                            plantDefToSow = job.plantDefToSow,
                            maxNumToCarry = 5
                        };
                    }
                    return null;
                }
            }
            return job;
        }

        private Thing GetBestSeedThingForSowing(Pawn pawn, IntVec3 cell, ThingDef_PlantWithSeeds customPlantDef)
        {
            Thing result = null;
            Predicate<Thing> validator = (Thing tempThing) => !tempThing.IsForbidden(pawn.Faction) && pawn.AnimalAwareOf(tempThing) && pawn.CanReserve(tempThing, 1);
            if (customPlantDef.SeedDef != null)
            {
                result = GenClosest.ClosestThingReachable(cell, ThingRequest.ForDef(customPlantDef.SeedDef), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
            }
            return result;
        }
    }
}
