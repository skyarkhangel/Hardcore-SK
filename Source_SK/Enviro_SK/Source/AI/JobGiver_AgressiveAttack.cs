using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace SK_Enviro
{
    class JobGiver_AgressiveAttack : ThinkNode_JobGiver
    {
        public const float MaxSearchDistance = 3f;
        private const int EnemyForgetTime = 200;
        private const int MaxMeleeChaseTicksMax = 600;

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {

            Pawn mindStateTarget = pawn.mindState.enemyTarget as Pawn;
            if (mindStateTarget == null)
            {
                return null;
            }

            // Check if target is still valid
            if (mindStateTarget.Destroyed || mindStateTarget.Downed || 
                Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick > EnemyForgetTime || 
                (pawn.Position - mindStateTarget.Position).LengthHorizontalSquared > (MaxSearchDistance * MaxSearchDistance) || 
                !GenSight.LineOfSight(pawn.Position, mindStateTarget.Position, false))
            {
                pawn.mindState.enemyTarget = null;
                return null;
            }

            if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
            {
                return null;
            }
            Job job = new Job(JobDefOf.AttackMelee, mindStateTarget)
            {
                maxNumMeleeAttacks = 1,
                expiryInterval = Rand.Range(EnemyForgetTime, MaxMeleeChaseTicksMax)
            };
            return job;
        }

    }
}
