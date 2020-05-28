using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hospitality
{
    public class JobDriver_BrowseItems : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.Touch);
            yield return new Toil {tickAction = delegate {
                pawn.rotationTracker.FaceCell(job.GetTarget(TargetIndex.B).Cell);
                pawn.GainComfortFromCellIfPossible();
                if (pawn.IsHashIntervalTick(100))
                {
                    pawn.jobs.CheckForJobOverride();
                }
            },
                defaultCompleteMode = ToilCompleteMode.Never};
        }
    }
}