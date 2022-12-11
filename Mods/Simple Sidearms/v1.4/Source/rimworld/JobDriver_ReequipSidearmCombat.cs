using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace SimpleSidearms.rimworld
{
    public class JobDriver_ReequipSidearmCombat : JobDriver_ReequipSidearm
    {
        public override Toil OnFinish()
        {
            return Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);
        }

    }
}
