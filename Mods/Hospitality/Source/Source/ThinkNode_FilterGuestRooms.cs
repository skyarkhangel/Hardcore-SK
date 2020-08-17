using RimWorld;
using Verse;
using Verse.AI;

namespace Hospitality
{
    public class ThinkNode_FilterGuestRooms : ThinkNode_Priority
    {
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            ThinkResult result = base.TryIssueJobPackage(pawn, jobParams);

            if (result.IsValid)
            {
                if (IsForbidden(result)) return ThinkResult.NoJob;

                // Area is now checked in Reachability_Patch
                //if (IsOutsideArea(result.Job, pawn.GetGuestArea())) return ThinkResult.NoJob;

                return result;
            }

            return ThinkResult.NoJob;
        }

        private static bool IsForbidden(ThinkResult result)
        {
            bool forbidden = false;
            {
                if (result.Job.targetA.HasThing && result.Job.targetA.Thing.IsForbidden(Faction.OfPlayer)) forbidden = true;
                if (result.Job.targetB.HasThing && result.Job.targetB.Thing.IsForbidden(Faction.OfPlayer)) forbidden = true;
                if (result.Job.targetC.HasThing && result.Job.targetC.Thing.IsForbidden(Faction.OfPlayer)) forbidden = true;
            }
            return forbidden;
        }
    }
}
