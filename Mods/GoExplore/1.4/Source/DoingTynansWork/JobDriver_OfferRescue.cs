using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace LetsGoExplore
{
    public class JobDriver_OfferHelp : JobDriver
    {
        private const TargetIndex OtherPawnInd = TargetIndex.A;

        public Pawn OtherPawn => (Pawn)pawn.CurJob.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(OtherPawnInd);
            yield return Toils_Goto.GotoThing(OtherPawnInd, PathEndMode.Touch);
            yield return Toils_General.DoAtomic(delegate
            {
                OtherPawn.GetLord()?.ReceiveMemo("SetFree");
                Messages.Message("OfferHelpMessageLGE".Translate(), MessageTypeDefOf.PositiveEvent);
            });
        }
    }
}
