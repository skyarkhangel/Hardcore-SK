using System.Collections.Generic;
using Verse.AI;
using Verse;

namespace MAD
{
    public class JobDriver_CarryToMAD : JobDriver
    {


        // TargetIndex.A   => VICTIM
        // TargetIndex.B   => MAD

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Pawn Warden = this.pawn;
            Pawn Victim = (Pawn)base.TargetA.Thing;
            Building_MAD MAD = (Building_MAD)base.TargetB.Thing;


            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(100);
            yield return new Toil
            {
                initAction = delegate
                {
                    Warden.carrier.TryStartCarry(Victim);
                },
            };
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.InteractionCell);
            yield return Toils_General.Wait(250);
            yield return new Toil
                        {
                            initAction = delegate
                            {
                                MAD.TryAcceptThing(Victim);
                                Warden.carrier.container.Clear();
                            },
                            defaultCompleteMode = ToilCompleteMode.Delay
                        };
            yield break;
        }
    }

}