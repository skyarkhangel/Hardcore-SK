using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Verse;
using Verse.AI;

namespace SK_Enviro.AI
{
    public class JobDriver_HuntForAnimals : JobDriver
    {
        private int numMeleeAttacksLanded;

        public JobDriver_HuntForAnimals()
        {
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //this.EndOnDespawned(TargetIndex.A, JobCondition.Succeeded);
            //this.FailOn(hunterIsKilled);

            //yield return Toils_Combat.TrySetJobToUseAttackVerb();
            //Toil gotoPosition = Toils_Combat.GotoCastPosition(TargetIndex.A);
            //yield return gotoPosition;
            //Toil jump = Toils_Jump.JumpIfTargetNotHittable(TargetIndex.A, gotoPosition);
            //yield return jump;
            //Log.Message(string.Concat(pawn, " trying to kill ", TargetA));
            //yield return Toils_Combat.TrySetJobToUseAttackVerb();
            //yield return Toils_Combat.CastVerb(TargetIndex.A);
            //yield return Toils_Jump.Jump(jump);

            Toil followAndAttack = new Toil();
            followAndAttack.tickAction = () =>
            {
                Pawn actor = followAndAttack.actor;
                Job curJob = actor.jobs.curJob;
                Thing t = curJob.GetTarget(TargetIndex.A).Thing;
                Pawn pawn2 = t as Pawn;
                if ((t != actor.pather.Destination.Thing) || (!pawn.pather.Moving && !pawn.Position.AdjacentTo8WayOrInside(t)))
                {
                    actor.pather.StartPath(t, PathEndMode.Touch);
                }
                else if (pawn.Position.AdjacentTo8WayOrInside(t))
                {
                    if (((t is Pawn) && pawn2.Downed) && !curJob.killIncappedTarget)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                    if (actor.natives.TryMeleeAttack(t))
                    {
                        this.numMeleeAttacksLanded++;
                        if (numMeleeAttacksLanded >= curJob.maxNumMeleeAttacks)
                        {
                            EndJobWith(JobCondition.Succeeded);
                        }
                    }
                }
            };
            followAndAttack.defaultCompleteMode = ToilCompleteMode.Never;
            followAndAttack.EndOnDespawned(TargetIndex.A, JobCondition.Succeeded);
            followAndAttack.FailOn(hunterIsKilled);
            yield return followAndAttack;
        }


        private bool hunterIsKilled()
        {
            return pawn.Dead || pawn.HitPoints == 0;
        }
    }
}
