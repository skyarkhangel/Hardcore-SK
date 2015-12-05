using System;
using System.Collections.Generic;

using Verse;
using Verse.AI;
using RimWorld;

namespace SK_Enviro.Seeds
{
    internal class JobDriver_HaulToCell : Verse.AI.JobDriver_HaulToCell
    {
        private const TargetIndex HaulableInd = TargetIndex.A;
        private const TargetIndex StoreCellInd = TargetIndex.B;

        private static Toil CheckForGetOpportunityDuplicate(Toil getHaulTargetToil, TargetIndex haulableInd, TargetIndex storeCellInd)
        {
            Toil toil = new Toil();

            toil.initAction = () =>
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                if (actor.carrier.CarriedThing.def.stackLimit != 1 && actor.carrier.container.AvailableStackSpace > 0)
                {
                    int num2 = curJob.maxNumToCarry - actor.carrier.CarriedThing.stackCount;
                    if (num2 > 0)
                    {
                        Predicate<Thing> validator = t =>
                        {
                            if (((!t.SpawnedInWorld || (t.def != actor.carrier.CarriedThing.def)) || (!t.CanStackWith(actor.carrier.CarriedThing) || t.IsForbidden(actor.Faction))) ||
                                (t.IsInValidStorage() || ((storeCellInd != TargetIndex.None) && !curJob.GetTarget(storeCellInd).Cell.IsValidStorageFor(t))))
                            {
                                return false;
                            }

                            return actor.CanReserve(t, 1);
                        };
                        Thing pack = GenClosest.ClosestThingReachable(actor.Position, ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.ClosestTouch, TraverseParms.For(actor, Danger.Deadly, TraverseMode.ByPawn, true), 8f, validator, null, -1);
                        if (pack != null)
                        {
                            curJob.SetTarget(haulableInd, pack);
                            actor.jobs.curDriver.SetNextToil(getHaulTargetToil);
                            actor.jobs.curDriver.SetCompleteMode(ToilCompleteMode.Instant);
                        }
                    }
                }
            };

            return toil;
        }

        public override string GetReport()
        {
            Thing carriedThing;
            IntVec3 cell = pawn.jobs.curJob.targetB.Cell;
            if (pawn.carrier.CarriedThing != null)
                carriedThing = pawn.carrier.CarriedThing;
            else
                carriedThing = TargetThingA;
            
            string str = null;
            SlotGroup slotGroup = cell.GetSlotGroup();
            if (slotGroup != null)
                str = slotGroup.parent.SlotYielderLabel();
            
            if (str != null)
                return "ReportHaulingTo".Translate(carriedThing.LabelCap, str);
            
            return "ReportHauling".Translate(carriedThing.LabelCap);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyed<JobDriver_HaulToCell>(HaulableInd);
            this.FailOnBurningImmobile<JobDriver_HaulToCell>(StoreCellInd);
            if (!TargetThingA.IsForbidden(pawn.Faction))
                this.FailOnForbidden<JobDriver_HaulToCell>(HaulableInd);
            
            yield return Toils_Reserve.Reserve(StoreCellInd, 1);
            Toil getHaulTargetToil = Toils_Reserve.Reserve(HaulableInd, 1);
            yield return getHaulTargetToil;
            Toil toil_goto = Toils_Goto.GotoThing(HaulableInd, PathEndMode.ClosestTouch);
            toil_goto.FailOn<Toil>(() =>
            {
                Job curJob = toil_goto.actor.jobs.curJob;
                if (curJob.haulMode == HaulMode.ToCellStorage)
                {
                    Thing thing = curJob.GetTarget(HaulableInd).Thing;
                    if (!curJob.GetTarget(StoreCellInd).Cell.IsValidStorageFor(thing))
                        return true;
                }
                return false;
            });
            yield return toil_goto;
            yield return Toils_Haul.StartCarryThing(HaulableInd);
            if (CurJob.haulOpportunisticDuplicates)
                yield return CheckForGetOpportunityDuplicate(getHaulTargetToil, HaulableInd, StoreCellInd);
            
            Toil nextToilOnPlaceFailOrIncomplete = Toils_Haul.CarryHauledThingToCell(StoreCellInd);
            yield return nextToilOnPlaceFailOrIncomplete;
            yield return Toils_Haul.PlaceHauledThingInCell(StoreCellInd, nextToilOnPlaceFailOrIncomplete, true);
        }

    }
}

