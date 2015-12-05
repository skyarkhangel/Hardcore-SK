using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
namespace MAD
{
    public static class MadUtility
    {
        public static Building_MAD FindMADFor(Pawn sleeper, Pawn traveler, bool sleeperWillBePrisoner, bool checkSocialProperness, bool forceCheckMedBed = false)
        {
            Predicate<Thing> MADValidator = delegate(Thing t)
            {
                if (!traveler.CanReserveAndReach(t, PathEndMode.OnCell, Danger.Some, 1))
                {
                    return false;
                }

                Building_MAD Building_MAD3 = (Building_MAD)t;

                if (Building_MAD3.HasAnyContents)
                {
                    return false;
                }
                else
                {
                    if (Building_MAD3.Faction != traveler.Faction)
                    {
                        return false;
                    }
                }
                return (!Building_MAD3.IsForbidden(traveler) && !Building_MAD3.IsBurning());
            };
            Predicate<Thing> validator = (Thing b) => MADValidator(b);

            Building_MAD building_MAD = (Building_MAD)GenClosest.ClosestThingReachable(sleeper.Position, ThingRequest.ForDef(DefDatabase<ThingDef>.GetNamed("MindAlteringDevice")), PathEndMode.OnCell, TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);

            if (building_MAD != null)
            {
                return building_MAD;
            }

            return null;
        }
    }
}
