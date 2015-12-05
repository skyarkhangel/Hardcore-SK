using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace SK_collect
{
    public class JobDriver_CollectPeat : JobDriver
    {
        private const TargetIndex CellInd = TargetIndex.A;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnBurningImmobile(CellInd);

            yield return Toils_Reserve.Reserve(CellInd);

            yield return Toils_Goto.GotoCell(CellInd, PathEndMode.Touch);

            yield return Toils_General.Wait(500);

            yield return Toils_Collect.MakeAndSpawnThingRandomRange(ThingDef.Named("Peat"), 1, 4);

            yield return Toils_Collect.RemoveDesignationAtPosition(GetActor().jobs.curJob.GetTarget(CellInd).Cell, DefDatabase<DesignationDef>.GetNamed("CollectPeat"));
        }
    }
}