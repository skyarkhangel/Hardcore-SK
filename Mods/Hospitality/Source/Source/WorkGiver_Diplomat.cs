using RimWorld;
using Verse;
using Verse.AI;

namespace Hospitality
{
    public class WorkGiver_Diplomat : WorkGiver_Scanner
    {
        private readonly JobDef jobDef = DefDatabase<JobDef>.GetNamed("GuestImproveRelationship");

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var guest = t as Pawn;
            return pawn.ShouldImproveRelationship(guest);
        }

        public override bool ShouldSkip(Pawn pawn, bool forced)
        {
            return pawn?.WorkTagIsDisabled(def.workType.workTags) ?? true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(jobDef, t);
        }

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override PathEndMode PathEndMode => PathEndMode.OnCell;
    }
}
