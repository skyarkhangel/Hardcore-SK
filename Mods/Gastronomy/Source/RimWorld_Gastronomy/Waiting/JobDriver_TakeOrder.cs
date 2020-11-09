using System.Collections.Generic;
using Gastronomy.Dining;
using RimWorld;
using Verse;
using Verse.AI;

namespace Gastronomy.Waiting
{
    public class JobDriver_TakeOrder : JobDriver
    {
        private const TargetIndex IndexSpot = TargetIndex.A;
        private const TargetIndex IndexStanding = TargetIndex.A;
        private const TargetIndex IndexPatron = TargetIndex.B;
        private const TargetIndex IndexChairPos = TargetIndex.C;
        private DiningSpot DiningSpot => job.GetTarget(IndexSpot).Thing as DiningSpot;
        private Pawn Patron => job.GetTarget(IndexPatron).Pawn;
        private IntVec3 ChairPos => job.GetTarget(IndexChairPos).Cell;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            var patron = Patron;
            var patronJob = patron.jobs.curDriver as JobDriver_Dine;
            var diningSpot = patronJob?.DiningSpot;

            if (diningSpot == null)
            {
                Log.Message($"{pawn.NameShortColored} couldn't take order from {patron?.NameShortColored}: patronJob = {patron.jobs.curDriver?.GetType().Name}");
                return false;
            }

            if (!pawn.Reserve(patron, job, 1, -1, null, errorOnFailed))
            {
                Log.Message($"{pawn.NameShortColored} FAILED to reserve patron {patron.NameShortColored}.");
                return false;
            }

            //Log.Message($"{pawn.NameShortColored} reserved patron {patron.NameShortColored}.");
            job.SetTarget(IndexSpot, diningSpot);
            return true;
        }

        //public override string GetReport()
        //{
        //    //if (job?.plantDefToSow == null) return base.GetReport();
        //    return "JobDineGoReport".Translate();
        //}

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var wait = Toils_General.Wait(50, IndexSpot).FailOnNotDiningQueued(IndexPatron);
            var end = Toils_General.Wait(5);

            this.FailOnNotDiningQueued(IndexPatron); // Careful never to use B for something else

            yield return Toils_Waiting.FindRandomAdjacentCell(IndexSpot, IndexStanding); // A is first the dining spot, then where we'll stand
            yield return Toils_Goto.GotoCell(IndexStanding, PathEndMode.OnCell).FailOnRestaurantClosed();
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
            yield return wait;
            yield return Toils_Jump.JumpIf(wait, () => !(Patron?.jobs.curDriver is JobDriver_Dine)); // Not dining right now
            yield return Toils_Waiting.GetDiningSpot(IndexPatron, IndexSpot); // A is dining spot again now
            yield return Toils_Waiting.GetSpecificDiningSpotCellForMakingTable(IndexSpot, IndexPatron, IndexChairPos);
            yield return Toils_Waiting.TakeOrder(IndexPatron);
            yield return Toils_Interpersonal.SetLastInteractTime(IndexPatron);
            yield return Toils_Jump.JumpIf(end, () => DiningSpot?.IsSpotReady(ChairPos) == true);
            yield return Toils_Goto.GotoCell(IndexSpot, PathEndMode.ClosestTouch);
            yield return Toils_Waiting.MakeTableReady(IndexSpot, IndexChairPos);
            yield return end;
        }
    }
}
