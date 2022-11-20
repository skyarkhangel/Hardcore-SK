using System.Collections.Generic;
using System.Linq;
using Hospitality.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hospitality
{
    public class JobDriver_ClaimBed : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!(TargetA.Thing is Building_GuestBed newBed)) return false;
            if (pawn.Reserve(TargetA, job, newBed.SleepingSlotsCount, 0, null, errorOnFailed)) return true;

            Log.Message($"{pawn.LabelShort} failed to reserve {TargetA.Thing.LabelShort}!");
            return false;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(BedHasBeenClaimed);//.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return ClaimBed();
        }

        private bool BedHasBeenClaimed(Toil toil)
        {
            return !(TargetA.Thing is Building_GuestBed {AnyUnownedSleepingSlot: true});
        }

        private Toil ClaimBed()
        {
            return new Toil
            {
                initAction = () => {
                    var actor = GetActor();
                    var silver = actor.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver);
                    var money = silver?.stackCount ?? 0;
                    
                    // Check the stored RentalFee (takeExtraIngestibles)... if it was increased, cancel!
                    if (!(TargetA.Thing is Building_GuestBed newBed) 
                        || newBed.RentalFee > job.takeExtraIngestibles 
                        || newBed.RentalFee > money) 
                    {
                        actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                        return;
                    }

                    if (!newBed.AnyUnownedSleepingSlot)
                    {
                        actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                        return;
                    }

                    var compGuest = actor.CompGuest();
                    if (compGuest.HasBed) Log.Error($"{actor.LabelShort} already has a bed ({compGuest.bed.Label})");

                    compGuest.ClaimBed(newBed);

                    if (newBed.RentalFee > 0)
                    {
                        actor.inventory.innerContainer.TryDrop(silver, actor.Position, Map, ThingPlaceMode.Near, newBed.RentalFee, out silver);
                        actor.UpsetAboutFee(newBed.RentalFee);
                    }
                }
            }.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        }
    }
}
