using System.Collections.Generic;
using Verse.AI;



namespace MAD
{
    public class JobDriver_OpenMAD : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_General.Wait(250);
            yield return new Toil

            {
                initAction = delegate
                {
                    ((Building_MAD)TargetA.Thing).EjectContents();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }
    }
}
