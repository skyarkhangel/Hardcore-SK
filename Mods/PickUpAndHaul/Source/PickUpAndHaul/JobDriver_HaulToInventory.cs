using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PickUpAndHaul
{
    public class JobDriver_HaulToInventory : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Log.Message($"{pawn} starting HaulToInventory job: {job.targetQueueA.ToStringSafeEnumerable()}:{job.countQueue.ToStringSafeEnumerable()}");
            this.pawn.ReserveAsManyAsPossible(this.job.targetQueueA, this.job);
            this.pawn.ReserveAsManyAsPossible(this.job.targetQueueB, this.job);
            return this.pawn.Reserve(this.job.targetQueueA[0], this.job) && pawn.Reserve(job.targetB, this.job);
        }

        //get next, goto, take, check for more. Branches off to "all over the place"
        protected override IEnumerable<Toil> MakeNewToils()
        {
            CompHauledToInventory takenToInventory = pawn.TryGetComp<CompHauledToInventory>();

            Toil wait = Toils_General.Wait(2);
            
            Toil nextTarget = Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A); //also does count
            yield return nextTarget;

            //honestly the workgiver checks for encumbered, so until CE checks are in this is unnecessary
            //yield return CheckForOverencumbered();//Probably redundant without CE checks

            Toil gotoThing = new Toil
            {
                initAction = () =>
                {
                    this.pawn.pather.StartPath(this.TargetThingA, PathEndMode.ClosestTouch);
                },
                defaultCompleteMode = ToilCompleteMode.PatherArrival
            };
            gotoThing.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return gotoThing;

            Toil takeThing = new Toil
            {
                initAction = () =>
                {
                    Pawn actor = this.pawn;
                    Thing thing = actor.CurJob.GetTarget(TargetIndex.A).Thing;
                    Toils_Haul.ErrorCheckForCarry(actor, thing);

                    //get max we can pick up
                    int countToPickUp = Mathf.Min(job.count, MassUtility.CountToPickUpUntilOverEncumbered(actor, thing));
                    Log.Message($"{actor} is hauling to inventory {thing}:{countToPickUp}");

                    // yo dawg, I heard you like delegates so I put delegates in your delegate, so you can delegate your delegates.
                    // because compilers don't respect IF statements in delegates and toils are fully iterated over as soon as the job starts.
                    try
                    {
                        ((Action)(() =>
                        {
                            if (ModCompatibilityCheck.CombatExtendedIsActive)
                            {
                                //CombatExtended.CompInventory ceCompInventory = actor.GetComp<CombatExtended.CompInventory>();
                                //ceCompInventory.CanFitInInventory(thing, out countToPickUp);
                            }
                        }))();
                    }
                    catch (TypeLoadException) { }

                    if (countToPickUp > 0)
                    {
                        Thing splitThing = thing.SplitOff(countToPickUp);
                        actor.inventory.GetDirectlyHeldThings().TryAdd(splitThing, false);
                        takenToInventory.RegisterHauledItem(splitThing);

                        try
                        {
                            ((Action)(() =>
                            {
                                if (ModCompatibilityCheck.CombatExtendedIsActive)
                                {
                                    //CombatExtended.CompInventory ceCompInventory = actor.GetComp<CombatExtended.CompInventory>();
                                    //ceCompInventory.UpdateInventory();
                                }
                            }))();
                        }
                        catch (TypeLoadException) { }
                    }
                    //thing still remains, so queue up hauling if we can + end the current job (smooth/instant transition)
                    //This will technically release the reservations in the queue, but what can you do
                    if (thing.Spawned)
                    {
                        Job haul = HaulAIUtility.HaulToStorageJob(actor, thing);
                        if (haul?.TryMakePreToilReservations(actor, false) ?? false)
                        {
                            actor.jobs.jobQueue.EnqueueFirst(haul, new JobTag?(JobTag.Misc));
                        }
                        actor.jobs.curDriver.JumpToToil(wait);
                    }
                }
            };
            yield return takeThing;
            yield return Toils_Jump.JumpIf(nextTarget, () => !job.targetQueueA.NullOrEmpty<LocalTargetInfo>());

            //maintain cell reservations on the trip back
            //TODO: do that when we carry things
            //I guess that means TODO: implement carrying the rest of the items in this job instead of falling back on HaulToStorageJob
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.ClosestTouch);

            yield return new Toil()//Queue next job
            {
                initAction = () =>
                {
                    Pawn actor = pawn;
                    Job curJob = actor.jobs.curJob;
                    LocalTargetInfo storeCell = curJob.targetB;

                    Job unloadJob = new Job(PickUpAndHaulJobDefOf.UnloadYourHauledInventory, storeCell);
                    if (unloadJob.TryMakePreToilReservations(actor, false))
                    {
                        actor.jobs.jobQueue.EnqueueFirst(unloadJob, new JobTag?(JobTag.Misc));
                        this.EndJobWith(JobCondition.Succeeded);
                        //This will technically release the cell reservations in the queue, but what can you do
                    }
                }
            };
            yield return wait;
        }
        
        public Toil CheckForOverencumbered()
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing nextThing = curJob.targetA.Thing;

                //float usedBulkByPct = 1f;
                //float usedWeightByPct = 1f;

                //try
                //{
                //    ((Action)(() =>
                //    {
                //        if (ModCompatibilityCheck.CombatExtendedIsActive)
                //        {
                //            CompInventory ceCompInventory = actor.GetComp<CompInventory>();
                //            usedWeightByPct = ceCompInventory.currentWeight / ceCompInventory.capacityWeight;
                //            usedBulkByPct = ceCompInventory.currentBulk / ceCompInventory.capacityBulk;
                //        }
                //    }))();
                //}
                //catch (TypeLoadException) { }


                if (!(MassUtility.EncumbrancePercent(actor) <= 0.9f /*|| usedBulkByPct >= 0.7f || usedWeightByPct >= 0.8f*/))
                {
                    Job haul = HaulAIUtility.HaulToStorageJob(actor, nextThing);
                    if (haul?.TryMakePreToilReservations(actor, false) ?? false)
                    {
                        //note that HaulToStorageJob etc doesn't do opportunistic duplicate hauling for items in valid storage. REEEE
                        actor.jobs.jobQueue.EnqueueFirst(haul, JobTag.Misc);
                        this.EndJobWith(JobCondition.Succeeded);
                    }
                }
            };
            return toil;
        }
    }
}