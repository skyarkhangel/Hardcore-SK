using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace SK_Enviro.AI
{
    public class JobGiver_DefendAnimal : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            JobDef huntJobDef = Animals_AI.GetHuntForAnimalsJobDef();
            if ((pawn.jobs.curJob == null) || ((pawn.jobs.curJob.def != huntJobDef) && pawn.jobs.curJob.checkOverrideOnExpire))
            {
                Pawn threat = pawn.mindState.meleeThreat;
                Pawn targetOfThreat = pawn;

                if (threat == null)
                {
                    IEnumerable<Pawn> herdMembers = HerdAIUtility_Pets.FindHerdMembers(pawn);
                    foreach (Pawn herdMember in herdMembers)
                    {
                        if (herdMember.mindState.meleeThreat != null)
                        {
                            threat = herdMember.mindState.meleeThreat;
                            pawn.mindState.meleeThreat = threat;
                            targetOfThreat = herdMember;
                            break;
                        }
                    }
                }


                if (threat == null || threat.Dead || threat.Downed
                    || (targetOfThreat.mindState.lastMeleeThreatHarmTick - Find.TickManager.TicksGame) > 300
                    || (targetOfThreat.Position - threat.Position).LengthHorizontalSquared > HerdAIUtility_Pets.HERD_DISTANCE
                    || !GenSight.LineOfSight(pawn.Position, threat.Position))
                {
                    pawn.mindState.meleeThreat = null;
                    return null;
                }
                else
                {
                    return new Job(huntJobDef)
                    {
                        targetA = threat,
                        maxNumMeleeAttacks = 4,
                        killIncappedTarget = true,
                        checkOverrideOnExpire = true,
                        expiryInterval = 500
                    };
                } 
            }
            return null;
        }
    }
}
