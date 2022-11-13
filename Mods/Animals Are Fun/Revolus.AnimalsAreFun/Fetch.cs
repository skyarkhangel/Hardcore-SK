using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Revolus.AnimalsAreFun {
    public class Fetch_JobDriver : OutsidePath_JobDriver {
        protected override IEnumerable<Toil> MakeNewToils() {
            this.SetupNewToils();

            Toil gotoAnimalInitial = null;
            Toil sayHelloToAnimal = null;
            Toil gotoFirstCell = null;
            Toil throwBall = null;
            Toil waitForAnimalToFetchBall = null;
            Toil sayByeToAnimal = null;

            var endWhenDone = this.MakeEndWhenDone(() => sayByeToAnimal);
            var setJog = this.MakeSetLocomotionUrgency(LocomotionUrgency.Jog);
            var setWalk = this.MakeSetLocomotionUrgency(LocomotionUrgency.Walk);
            var setStop = this.MakeSetLocomotionUrgency(LocomotionUrgency.None);

            // initial go to animal
            gotoAnimalInitial = Toils_Goto.GotoThing(WalkingAnimalIndex, PathEndMode.Touch);
            gotoAnimalInitial.socialMode = RandomSocialMode.Quiet;
            gotoAnimalInitial.AddPreInitAction(setJog);

            // say hello to animal
            sayHelloToAnimal = new Toil();
            sayHelloToAnimal.initAction = this.MakeChatWithAnimal();
            sayHelloToAnimal.defaultCompleteMode = ToilCompleteMode.Delay;
            sayHelloToAnimal.defaultDuration = 90;
            sayHelloToAnimal.socialMode = RandomSocialMode.SuperActive;
            sayHelloToAnimal.AddFinishAction(this.MakeSetAnimalJob(() => {
                var animalFollowJob = JobMaker.MakeJob(JobDefOf.Follow, this.pawn);
                animalFollowJob.locomotionUrgency = LocomotionUrgency.Jog;
                return animalFollowJob;
            }));
            sayHelloToAnimal.AddPreInitAction(setWalk);

            // go to first cell
            gotoFirstCell = Toils_Goto.GotoCell(OutsideCellIndex, PathEndMode.OnCell);
            gotoFirstCell.tickAction = endWhenDone;
            gotoFirstCell.socialMode = RandomSocialMode.Quiet;
            gotoFirstCell.AddPreInitAction(setJog);

            // throw ball
            throwBall = new Toil();
            throwBall.initAction = () => this.DoThrowBallAndMakeAnimalFetch(sayByeToAnimal);
            throwBall.tickAction = endWhenDone;
            throwBall.socialMode = RandomSocialMode.SuperActive;
            throwBall.AddPreInitAction(setStop);

            // wait for animal to return ball
            waitForAnimalToFetchBall = new Toil();
            waitForAnimalToFetchBall.defaultCompleteMode = ToilCompleteMode.Delay;
            waitForAnimalToFetchBall.defaultDuration = 8000;
            waitForAnimalToFetchBall.socialMode = RandomSocialMode.SuperActive;
            waitForAnimalToFetchBall.AddPreInitAction(setStop);
            waitForAnimalToFetchBall.tickAction = delegate {
                if (Find.TickManager.TicksGame > this.startTick + this.job.def.joyDuration || this.expectedAnimalJob is null) {
                    this.JumpToToil(sayByeToAnimal);
                } else if (!this.expectedAnimalJob.targetA.IsValid) {
                    this.JumpToToil(throwBall);
                } else {
                    JoyUtility.JoyTickCheckEnd(this.pawn);
                }                
            };

            // say bye to animal
            sayByeToAnimal = new Toil();
            sayByeToAnimal.defaultCompleteMode = ToilCompleteMode.Delay;
            sayByeToAnimal.defaultDuration = 90;
            sayByeToAnimal.socialMode = RandomSocialMode.Quiet;
            sayByeToAnimal.AddPreInitAction(setJog);

            // return toils
            return new Toil[] {
                gotoAnimalInitial, sayHelloToAnimal, gotoFirstCell, throwBall, waitForAnimalToFetchBall, sayByeToAnimal
            };
        }

        private void DoThrowBallAndMakeAnimalFetch(Toil whenFinishedGoto) {
            var walkingQueue = this.OutsidePath;
            if (walkingQueue.Count <= 0) {
                this.JumpToToil(whenFinishedGoto);
                return;
            }

            var targetCell = walkingQueue[0];
            walkingQueue.RemoveAt(0);
            this.job.targetA = targetCell;

            this.pawn.rotationTracker.FaceTarget(targetCell);

            FleckMaker.ThrowStone(this.pawn, targetCell.Cell);
            var animalJob = JobMaker.MakeJob(
                AnimalsAreFunDefOf.Revolus_AnimalsAreFun_Walkies_Job_Animal,
                targetCell, this.pawn
            );
            this.SetAnimalJob(animalJob);
        }
    }

    public class Fetch_JobDriver_Animal : JobDriver {
        public const TargetIndex CellIndex = TargetIndex.A;
        public const TargetIndex HandlerIndex = TargetIndex.B;

        protected LocalTargetInfo CellTargetInfo => this.job.GetTarget(CellIndex);

        protected LocalTargetInfo HandlerTargetInfo => this.job.GetTarget(HandlerIndex);
        protected Pawn Handler => (Pawn) this.HandlerTargetInfo.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            Toil watchBall = null;
            Toil gotoBall = null;
            Toil pickUpBall = null;
            Toil gotoHandler = null;
            Toil giveBallToHandler = null;

            watchBall = new Toil();
            watchBall.defaultCompleteMode = ToilCompleteMode.Delay;
            watchBall.defaultDuration = 120;
            watchBall.AddPreInitAction(delegate {
                this.pawn.rotationTracker.FaceTarget(this.CellTargetInfo);
            });

            gotoBall = Toils_Goto.GotoCell(CellIndex, PathEndMode.OnCell);
            gotoBall.AddPreInitAction(delegate {
                this.job.locomotionUrgency = LocomotionUrgency.Sprint;
            });

            pickUpBall = new Toil();
            pickUpBall.defaultCompleteMode = ToilCompleteMode.Delay;
            pickUpBall.defaultDuration = 90;

            gotoHandler = Toils_Goto.GotoCell(HandlerIndex, PathEndMode.Touch);
            gotoHandler.AddPreInitAction(delegate {
                this.job.locomotionUrgency = LocomotionUrgency.Jog;
            });

            giveBallToHandler = new Toil();
            giveBallToHandler.defaultCompleteMode = ToilCompleteMode.Delay;
            giveBallToHandler.defaultDuration = 120;

            return new Toil[] { watchBall, gotoBall, pickUpBall, gotoHandler, giveBallToHandler };
        }
    }
}
