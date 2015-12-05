using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Verse;
using Verse.AI;
using RimWorld;

namespace SK_Enviro.AI
{
    public class JobDriver_AnimalEatCorpse : JobDriver
    {
        public JobDriver_AnimalEatCorpse()
        {
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyed<JobDriver_AnimalEatCorpse>(TargetIndex.A);
            this.FailOnDespawnedIfNonNull<JobDriver_AnimalEatCorpse>(TargetIndex.A);
            this.FailOn<JobDriver_AnimalEatCorpse>(eaterIsKilled);
            Toil resCorpse = new Toil();
            resCorpse.initAction = new Action(() =>
            {
                Pawn actor = resCorpse.actor;
                Thing target = resCorpse.actor.CurJob.GetTarget(TargetIndex.A).Thing;
                if (!target.SpawnedInWorld || !Find.Reservations.CanReserve(actor, target, 1))
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    Find.Reservations.Reserve(actor, target, 1);
                }
            });
            resCorpse.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return resCorpse;

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(60);

            Toil stripCorpse = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            stripCorpse.initAction = new Action(doStripCorpse);
            yield return stripCorpse;
            yield return Toils_General.Wait(60);

            Toil chewCorpse = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            chewCorpse.initAction = new Action(doChewCorpse);
            chewCorpse.WithEffect(EffecterDef.Named("EatMeat"), TargetIndex.A);
            chewCorpse.EndOnDespawned(TargetIndex.A);
            yield return chewCorpse;
        }

        private bool eaterIsKilled()
        {
            return pawn.Dead || pawn.Downed || pawn.HitPoints == 0;
        }

        private void doStripCorpse()
        {
            Thing t = CurJob.targetA.Thing;
            Corpse corpse = t as Corpse;
            if ((corpse != null) && corpse.AnythingToStrip())
            {
                corpse.StripDeteriorate();
            }
        }

        private void doChewCorpse()
        {
            Corpse corpse = TargetThingA as Corpse;
            if (corpse != null)
            {
                IntVec3 centerPos = corpse.Position;
                List<Thing> leftOvers = Animals_AI.ButcherCorpseProducts(corpse, pawn).ToList<Thing>();
                Thing leftOver = null;
                for (int i = 0; i < leftOvers.Count; i++)
                {
                    if (!GenPlace.TryPlaceThing(leftOvers[i], centerPos, ThingPlaceMode.Near, out leftOver))
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                    if (leftOver != null)
                        leftOver.SetForbidden(true);
                }
                if (pawn.needs.mood != null)
                    pawn.needs.mood.thoughts.TryGainThought(ThoughtDef.Named("AteStraightFromCorpse"));
                corpse.Destroy();
            }
            pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
        }
    }
}
