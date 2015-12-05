using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace SK_Enviro.AI
{
    class JobDriver_BashDoor : JobDriver
    {
        private int numMeleeAttacksLanded;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // go to blocking thing
            Toil Goto = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            Goto.FailOnDestroyed(TargetIndex.B);
            yield return Goto;

            // bash it down
            yield return BashIt();
        }

        public Toil BashIt()
        {
  

            Toil bashIt = new Toil();
            bashIt.tickAction = delegate
            {
                Pawn actor = bashIt.actor;
                Job curJob = actor.jobs.curJob;
                Thing t = curJob.GetTarget(TargetIndex.B).Thing;

                if (actor.natives.TryMeleeAttack(t))
                {
                    this.numMeleeAttacksLanded++;
                    if (numMeleeAttacksLanded >= curJob.maxNumMeleeAttacks)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                }
            };
            bashIt.defaultCompleteMode = ToilCompleteMode.Never;
            bashIt.EndOnDespawned(TargetIndex.B, JobCondition.Succeeded);
            bashIt.FailOn(hunterIsKilled);
            return bashIt;
        }

        private bool hunterIsKilled()
        {
            return pawn.Dead || pawn.HitPoints == 0;
        }
    }
}
