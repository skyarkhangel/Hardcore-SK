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
    public class JobDriver_EquipSidearm : JobDriver
    {
        public virtual bool MemorizeOnPickup { get { return true; } }

        public virtual Toil OnFinish() { return null; }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //Reservation logic when dealing with queues.
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    ThingWithComps thingWithComps = (ThingWithComps)this.job.targetA.Thing;
                    ThingWithComps thingWithComps2;
                    if (thingWithComps.def.stackLimit > 1 && thingWithComps.stackCount > 1)
                    {
                        thingWithComps2 = (ThingWithComps)thingWithComps.SplitOff(1);
                    }
                    else
                    {
                        if (thingWithComps.Spawned)
                            thingWithComps.DeSpawn(DestroyMode.Vanish);

                        if (thingWithComps.holdingOwner != null)
                            thingWithComps.holdingOwner.Remove(thingWithComps);

                        thingWithComps2 = thingWithComps;
                    }
                    bool success = this.pawn.inventory.innerContainer.TryAdd(thingWithComps2);
                    if (thingWithComps.def.soundInteract != null)
                    {
                        thingWithComps.def.soundInteract.PlayOneShot(new TargetInfo(this.pawn.Position, this.pawn.Map, false));
                    }
                    if (success)
                    {
                        CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(this.pawn);
                        if (pawnMemory == null)
                            return;
                        if(MemorizeOnPickup)
                            pawnMemory.InformOfAddedSidearm(thingWithComps2);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            Toil onFinish = OnFinish();
            if (onFinish != null)
                yield return onFinish;
        }

    }
}
