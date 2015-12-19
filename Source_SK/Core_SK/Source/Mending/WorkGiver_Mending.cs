using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace SK_Mending
{
    public class WokrGiver_Mending : WorkGiver_Scanner
    {
        private MenderBuildingComp mbc = null;
        private Pawn menderPawnForPredicate = null;

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(ThingDef.Named("TableMender"));
            }
        }

        public override Job JobOnThing(Pawn menderPawn, Thing menderTableThing)
        {
            menderPawnForPredicate = menderPawn;
            Building_WorkTable building_WorkTable = menderTableThing as Building_WorkTable;
            if (building_WorkTable == null)
            {
                return null;
            }
            else
            {
                this.mbc = ThingCompUtility.TryGetComp<MenderBuildingComp>(menderTableThing);
                if (this.mbc == null)
                {
                    return null;
                }
                else if (!ReservationUtility.CanReserveAndReach(menderPawn, menderTableThing, PathEndMode.Touch, DangerUtility.NormalMaxDanger(menderPawn)))
                {
                    return null;
                }
                else
                {
                    CompPowerTrader comp = building_WorkTable.GetComp<CompPowerTrader>();
                    if (comp != null)
                    {
                        if (!comp.PowerOn)
                        {
                            return null;
                        }
                    }
                    try
                    {
                        Thing thing = GenClosest.ClosestThingReachable(menderTableThing.Position, ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.Touch, TraverseParms.For(menderPawn, DangerUtility.NormalMaxDanger(menderPawn), TraverseMode.ByPawn, false), this.mbc.searchRadius / 2f, new Predicate<Thing>(this.SearchPredicate), null, -1);
                        if (thing == null)
                        {
                            return null;
                        }
                        IntVec3 invalid = IntVec3.Invalid;
                        if (!StoreUtility.TryFindBestBetterStoreCellFor(thing, menderPawn, 0, menderPawn.Faction, out invalid, true))
                        {
                            invalid = IntVec3.Invalid;
                        }
                        else
                        {
                            ReservationUtility.Reserve(menderPawn, invalid, 1);
                        }
                        Job job = new Job(DefDatabase<JobDef>.GetNamed("JobDriver_Mending", true), menderTableThing, thing);
                        job.maxNumToCarry = 1;
                        if (invalid != IntVec3.Invalid)
                        {
                            job.targetC = invalid;
                        }
                        return job;
                    }
                    catch (Exception E)
                    {
                        Log.Error(E.ToString());
                        return null;
                    }
                }
            }
        }

        private bool SearchPredicate(Thing t)
        {
            bool result;
            try
            {
                if (!this.mbc.GetAllowances().Allows(t))
                {
                    result = false;
                    return result;
                }
                if (t.HitPoints <= 0 || t.HitPoints >= t.MaxHitPoints)
                {
                    result = false;
                    return result;
                }
                if (Find.Reservations.FirstReserverOf(t, Faction.OfColony) != null)
                {
                    result = false;
                    return result;
                }
                if (ForbidUtility.IsForbidden(t, menderPawnForPredicate))
                {
                    result = false;
                    return result;
                }
                if (ForbidUtility.IsForbidden(t, Faction.OfColony))
                {
                    result = false;
                    return result;
                }
                if (FireUtility.IsBurning(t))
                {
                    result = false;
                    return result;
                }
                if (!this.mbc.outsideItems && !Find.RoofGrid.Roofed(t.Position))
                {
                    result = false;
                    return result;
                }
                result = true;
                return result;
            }
            catch (Exception E)
            {
                Log.Error(E.ToString());
            }
            result = false;
            return result;
        }
    }
}
