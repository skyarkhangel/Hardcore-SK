using Hospitality.Utilities;
using Verse;
using Verse.AI;

namespace Hospitality
{
    public class JobGiver_ClaimBed : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn guest)
        {
            var guestComp = guest.CompGuest();
            if (guestComp == null) return null;
            if (guestComp.HasBed) return null;

            // Wait longer if we have more money, but do it as soon as otherwise possible
            if (GenTicks.TicksGame < guestComp.lastBedCheckTick  + guest.GetMoney()*4) return null;
            
            guestComp.lastBedCheckTick = GenTicks.TicksGame + 500; // Recheck ever x ticks

            var bed = guest.FindBedFor();
            if (bed == null) return null;

            return new Job(InternalDefOf.ClaimGuestBed, bed) {takeExtraIngestibles = bed.RentalFee}; // Store RentalFee to avoid cheating
        }
    }
}
