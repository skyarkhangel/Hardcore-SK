using System.Collections.Generic;
using Gastronomy.Dining;
using Verse;
using Verse.AI;

namespace Gastronomy.Waiting
{
    public class JobDriver_MakeTable : JobDriver
    {
        private const TargetIndex IndexSpot = TargetIndex.A;
        private const TargetIndex IndexInteractionCell = TargetIndex.B;
        private DiningSpot DiningSpot => job.GetTarget(IndexSpot).Thing as DiningSpot;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            var diningSpot = DiningSpot;

            if (!pawn.Reserve(diningSpot, job, 1, 1, null, errorOnFailed))
            {
                Log.Message($"{pawn.NameShortColored} FAILED to reserve dining spot for making table at {diningSpot.Position}.");
                return false;
            }

            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var begin = Toils_Waiting.GetRandomDiningSpotCellForMakingTable(IndexSpot, IndexInteractionCell);
            var end = Toils_General.Wait(10, IndexSpot);

            this.FailOnDestroyedOrNull(IndexSpot);
            this.FailOnForbidden(IndexSpot);
            yield return begin;
            yield return Toils_Jump.JumpIf(end, () => pawn.CurJob?.GetTarget(IndexInteractionCell).IsValid == false);
            yield return Toils_Goto.GotoThing(IndexInteractionCell, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(IndexInteractionCell);
            yield return Toils_Waiting.MakeTableReady(IndexSpot, IndexInteractionCell);
            yield return end;
        }
    }
}
