using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Revolus.AnimalsAreFun {
    public class Walkies_JobDriver : OutsidePath_JobDriver {
        protected override IEnumerable<Toil> MakeNewToils() {
            this.SetupNewToils();

            Toil gotoAnimalInitial = null;
            Toil sayHelloToAnimal = null;
            Toil gotoFirstCell = null;
            Toil gotoAnimalAgain = null;
            Toil gotoNextCell = null;
            Toil sayByeToAnimal = null;

            var endWhenDone = this.MakeEndWhenDone(() => sayByeToAnimal);
            var walkToQueuedCell = this.MakeWalkToQueuedCell(() => gotoFirstCell);
            var setJog = this.MakeSetLocomotionUrgency(LocomotionUrgency.Jog);
            var setWalk = this.MakeSetLocomotionUrgency(LocomotionUrgency.Walk);

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
                animalFollowJob.locomotionUrgency = LocomotionUrgency.Walk;
                return animalFollowJob;
            }));
            sayHelloToAnimal.AddPreInitAction(setWalk);

            // go to first cell
            gotoFirstCell = Toils_Goto.GotoCell(OutsideCellIndex, PathEndMode.OnCell);
            gotoFirstCell.tickAction = endWhenDone;
            gotoFirstCell.socialMode = RandomSocialMode.SuperActive;
            gotoFirstCell.AddPreInitAction(setWalk);

            // go to animal again
            gotoAnimalAgain = Toils_Goto.GotoThing(WalkingAnimalIndex, PathEndMode.Touch);
            gotoAnimalAgain.tickAction = endWhenDone;
            gotoAnimalAgain.socialMode = RandomSocialMode.SuperActive;
            gotoAnimalAgain.AddPreInitAction(setWalk);

            // go to next cell
            gotoNextCell = new Toil();
            gotoNextCell.initAction = walkToQueuedCell;
            gotoNextCell.tickAction = endWhenDone;
            gotoNextCell.socialMode = RandomSocialMode.SuperActive;
            gotoNextCell.AddPreInitAction(setWalk);

            // say bye to animal
            sayByeToAnimal = new Toil();
            sayByeToAnimal.defaultCompleteMode = ToilCompleteMode.Delay;
            sayByeToAnimal.defaultDuration = 90;
            sayByeToAnimal.socialMode = RandomSocialMode.Quiet;
            sayByeToAnimal.AddPreInitAction(setJog);

            // return toils
            return new Toil[] {
                gotoAnimalInitial, sayHelloToAnimal, gotoFirstCell, gotoAnimalAgain, gotoNextCell, sayByeToAnimal
            };
        }
    }
}
