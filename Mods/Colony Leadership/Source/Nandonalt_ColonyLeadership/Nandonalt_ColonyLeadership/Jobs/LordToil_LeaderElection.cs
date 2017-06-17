using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace Nandonalt_ColonyLeadership
{
    public class LordToil_LeaderElection : LordToil
    {
        private IntVec3 spot;

        public LordToil_LeaderElection(IntVec3 spot)
        {
            this.spot = spot;
        }

        public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
        {
            return DefDatabase<DutyDef>.GetNamed("GatherLeader").hook;
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("GatherLeader"), this.spot, -1f);
            }
        }
    }
}
