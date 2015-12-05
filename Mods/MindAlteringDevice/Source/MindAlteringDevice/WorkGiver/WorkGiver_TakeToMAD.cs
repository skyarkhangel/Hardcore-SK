using System;
using Verse;
using Verse.AI;
using MAD;
namespace RimWorld
{
    public class WorkGiver_TakeToMAD : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.OnCell;
            }
        }
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            }
        }
        public override bool HasJobOnThing(Pawn pawn, Thing t)
        {
            Pawn pawn2 = t as Pawn;

            if (pawn2 == null || !pawn2.IsPrisonerOfColony || !pawn2.guest.PrisonerIsSecure ||
                pawn2.holder != null || (pawn2.Broken && pawn2.BrokenStateDef.isAggro) ||
                !pawn.CanReserveAndReach(pawn2, PathEndMode.OnCell, pawn.NormalMaxDanger(), 1))
            {
                return false;
            }
            Thing thing = FindMAD(pawn, pawn2);

            return thing != null && pawn2.CanReserve(thing, 1);
        }

        public override Job JobOnThing(Pawn pawn, Thing t)
        {
            Pawn pawn2 = t as Pawn;
            if (pawn2 == null || !pawn2.IsPrisonerOfColony || !pawn2.guest.PrisonerIsSecure ||
                pawn2.holder != null || (pawn2.Broken && pawn2.BrokenStateDef.isAggro) ||
                !pawn.CanReserveAndReach(pawn2, PathEndMode.OnCell, pawn.NormalMaxDanger(), 1))
            {
                return null;
            }
            Building_MAD building_MAD = MadUtility.FindMADFor(pawn2, pawn, true, false, false);
            return new Job(DefDatabase<JobDef>.GetNamed("CarryToMAD"), pawn2, building_MAD);

        }
        protected Thing FindMAD(Pawn pawn, Pawn patient)
        {
            return MadUtility.FindMADFor(patient, pawn, patient.HostFaction == pawn.Faction, false, true);
        }
    }
}