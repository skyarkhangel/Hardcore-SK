using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hospitality
{
    public class JobDriver_BrowseItems : JobDriver
    {
        private int ticksSpent;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksSpent, "ticksSpent");
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            //yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.Touch);
            var toil = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Never,
                tickAction = delegate {
                    pawn.rotationTracker.FaceCell(job.GetTarget(TargetIndex.B).Cell);
                    pawn.GainComfortFromCellIfPossible();
                    ticksSpent++;
                }
            };
            toil.AddPreTickAction(delegate {
                const int minDuration = GenDate.TicksPerHour / 8;

                if (ticksSpent >= minDuration && pawn.IsHashIntervalTick(100))
                {
                    pawn.jobs.CheckForJobOverride();
                }
            });
            yield return toil;
        }
    }
}