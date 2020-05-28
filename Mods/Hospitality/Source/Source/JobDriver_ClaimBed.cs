using System.Collections.Generic;
using System.Linq;
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
            if (pawn.Reserve(TargetA, job, newBed.SleepingSlotsCount, 0, null, errorOnFailed))
            {
                return true;
            }

            Log.Message($"{pawn.LabelShort} failed to reserve {TargetA.Thing.LabelShort}!");
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(BedHasBeenClaimed);//.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return ClaimBed();
        }

        private bool BedHasBeenClaimed(Toil toil)
        {
            return !(TargetA.Thing is Building_GuestBed newBed) || !newBed.AnyUnownedSleepingSlot;
        }

        private Toil ClaimBed()
        {
            return new Toil
            {
                initAction = () => {
                    var actor = GetActor();
                    var silver = actor.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver);
                    var money = silver?.stackCount ?? 0;
                    
                    // Check the stored rentalFee (takeExtraIngestibles)... if it was increased, cancel!
                    if (!(TargetA.Thing is Building_GuestBed newBed) 
                        || newBed.rentalFee > job.takeExtraIngestibles 
                        || newBed.rentalFee > money) 
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

                    if (newBed.rentalFee > 0)
                    {
                        actor.inventory.innerContainer.TryDrop(silver, actor.Position, Map, ThingPlaceMode.Near, newBed.rentalFee, out silver);
                        actor.UpsetAboutFee(newBed.rentalFee);
                    }
                }
            }.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        }
    }
}
