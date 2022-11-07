using System.Collections.Generic;
using Hospitality.Utilities;
using RimWorld;
using Verse.AI;

namespace Hospitality
{
    public class JobDriver_SwipeFood : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnAggroMentalState(TargetIndex.A);
            this.FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch);
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).socialMode = RandomSocialMode.Off;
            yield return Toils_General.Do(delegate
            {
                var target = pawn.CurJob.targetA.Pawn;
                pawn.ScroungedFoodFrom(target, true);
            });
        }
    }
}