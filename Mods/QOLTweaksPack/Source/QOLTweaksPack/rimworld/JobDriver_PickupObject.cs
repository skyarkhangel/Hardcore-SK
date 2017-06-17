using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace QOLTweaksPack.rimworld
{
    class JobDriver_PickupObject : JobDriver
    {
        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil reserveItem = Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return reserveItem;
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);            
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveItem, TargetIndex.A, TargetIndex.None, true);
            yield return Toils_General.Wait(500).FailOnDestroyedNullOrForbidden(TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
        }
    }
}
