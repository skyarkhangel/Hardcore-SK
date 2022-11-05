using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Revolus.AnimalsAreFun {
    public class OutsidePath_JoyGiver : JoyGiver {
        public override Job TryGiveJob(Pawn pawn) {
            if (!Common.PawnMayEnjoyPlayingOutside(pawn)) {
                return null;
            }

            var walkingAnimal = Common.GetAnimal(pawn);
            if (walkingAnimal is null) {
                AnimalsAreFun.Debug($"no valid animal found");
                return null;
            }

            if (!Common.TryFindOutsideWalkPath(pawn, walkingAnimal, out var firstCell, out var queue)) {
                AnimalsAreFun.Debug($"no path");
                return null;
            }

            var job = JobMaker.MakeJob(this.def.jobDef, firstCell.Value, walkingAnimal);
            job.targetQueueA = queue;
            job.locomotionUrgency = LocomotionUrgency.Jog;
            AnimalsAreFun.Debug($"found animal {walkingAnimal.ToStringSafe()}, made job {job.ToStringSafe()}");
            return job;
        }
    }

    public abstract class OutsidePath_JobDriver : JobDriver {
        protected const TargetIndex OutsideCellIndex = TargetIndex.A;
        protected const TargetIndex WalkingAnimalIndex = TargetIndex.B;

        protected List<LocalTargetInfo> OutsidePath => this.job.targetQueueA;

        protected LocalTargetInfo WalkingAnimalTargetInfo => this.job.GetTarget(WalkingAnimalIndex);
        protected Pawn WalkingAnimal => (Pawn) this.WalkingAnimalTargetInfo.Thing;
        
        protected Job expectedAnimalJob = null;

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            return this.pawn.Reserve(this.WalkingAnimalTargetInfo, this.job, errorOnFailed: errorOnFailed);
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_References.Look(ref this.expectedAnimalJob, "expectedAnimalJob");
        }

        protected void SetupNewToils() {
            this.FailOn(this.PawnIsFailed);
            this.FailOn(this.AnimalIsFailed);
            this.FailOn(this.AnimalHasWrongJob);
            this.AddFinishAction(this.UnqueueExpectedJob);
        }

        protected bool PawnIsFailed() {
            var pawn = this.pawn;
            return Common.PawnOrAnimalIsGoneOrIncapable(pawn) || !JoyUtility.EnjoyableOutsideNow(pawn);
        }

        protected bool AnimalIsFailed() => Common.PawnOrAnimalIsGoneOrIncapable(this.WalkingAnimal);

        protected bool AnimalHasWrongJob() {
            var expectedAnimalJob = this.expectedAnimalJob;
            if (expectedAnimalJob is null) {
                return false;
            }
            
            var curJob = this.WalkingAnimal.jobs.curJob;
            return curJob != null && curJob != expectedAnimalJob;
        }

        protected Action MakeEndWhenDone(Func<Toil> endToil) {
            return delegate {
                if (Find.TickManager.TicksGame > this.startTick + this.job.def.joyDuration) {
                    this.JumpToToil(endToil());
                } else {
                    JoyUtility.JoyTickCheckEnd(this.pawn);
                }
            };
        }

        protected Action MakeWalkToQueuedCell(Func<Toil> startToil) => delegate {
            var walkingQueue = this.OutsidePath;
            if (walkingQueue.Count <= 0) {
                return;
            }

            var firstCell = walkingQueue[0];
            walkingQueue.RemoveAt(0);
            this.job.targetA = firstCell;
            this.JumpToToil(startToil());
        };

        protected Action MakeSetLocomotionUrgency(LocomotionUrgency urgency) => delegate {
            this.job.locomotionUrgency = urgency;
        };

        protected void SetAnimalJob(Job animalJob) {
            this.UnqueueExpectedJob();

            if (animalJob is null) {
                return;
            }

            var animalJobs = this.WalkingAnimal.jobs;
            animalJobs.StopAll();
            animalJobs.StartJob(animalJob);

            this.expectedAnimalJob = animalJob;
        }

        protected Action MakeSetAnimalJob(Func<Job> makeAnimalJob) => delegate {
            this.SetAnimalJob(makeAnimalJob());
        };

        protected Action MakeChatWithAnimal() => delegate {
            this.pawn.interactions.TryInteractWith(this.WalkingAnimal, InteractionDefOf.AnimalChat);
        };

        protected void UnqueueExpectedJob() {
            var expectedAnimalJob = this.expectedAnimalJob;
            this.expectedAnimalJob = null;
            if (expectedAnimalJob != null) {
                var walkingAnimal = this.WalkingAnimal;
                if (!Common.PawnOrAnimalIsGone(walkingAnimal)) {
                    walkingAnimal.jobs.EndCurrentOrQueuedJob(expectedAnimalJob, JobCondition.Succeeded);
                }
            }
        }
    }
}
