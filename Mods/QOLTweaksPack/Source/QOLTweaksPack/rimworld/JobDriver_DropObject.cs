using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse.AI;

namespace QOLTweaksPack.rimworld
{
    class JobDriver_DropObject : JobDriver
    {
        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.A);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.A, carryToCell, true);
        }
    }
}
