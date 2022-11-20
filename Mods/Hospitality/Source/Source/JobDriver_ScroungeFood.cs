using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hospitality
{
    public class JobDriver_ScroungeFood : JobDriver
    {
        private Pawn OtherPawn => job.GetTarget(TargetIndex.A).Pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnAggroMentalState(TargetIndex.A);
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            Toil toil = GotoPawn(TargetIndex.A);
            toil.socialMode = RandomSocialMode.Off;
            yield return toil;
            Toil finalGoto = GotoPawn(TargetIndex.A);
            yield return Toils_Jump.JumpIf(finalGoto, () => !OtherPawn.Awake());
            Toil toil2 = Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
            toil2.socialMode = RandomSocialMode.Off;
            yield return toil2;
            finalGoto.socialMode = RandomSocialMode.Off;
            yield return finalGoto;
            yield return Toils_General.Do(delegate
            {
                if (!OtherPawn.Awake())
                {
                    OtherPawn.jobs.SuspendCurrentJob(JobCondition.InterruptForced);
                    var intDef = DefDatabase<InteractionDef>.GetNamed("ScroungeFoodAttempt");
                    if (!pawn.interactions.CanInteractNowWith(OtherPawn, intDef))
                    {
                        Log.Message($"{pawn.LabelCap} failed to scrounge food from {OtherPawn.Label}: Could not interact.");
                    }
                }
            });
            yield return Toils_Interpersonal.Interact(TargetIndex.A, DefDatabase<InteractionDef>.GetNamed("ScroungeFoodAttempt"));
        }

        public static Toil GotoPawn(TargetIndex targetInd)
        {
            var toil = ToilMaker.MakeToil();
            toil.tickAction = delegate
            {
                var actor = toil.actor;
                var target = actor.jobs.curJob.GetTarget(targetInd);

                if (target != actor.pather.Destination || (!actor.pather.Moving && !actor.CanReachImmediate(target, PathEndMode.Touch)))
                {
                    actor.pather.StartPath(target, PathEndMode.Touch);
                }
                else if (actor.CanReachImmediate(target, PathEndMode.Touch))
                {
                    actor.jobs.curDriver.ReadyForNextToil();
                }
            };
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            return toil;
        }
    }
}