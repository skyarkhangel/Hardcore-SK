using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace SK_Enviro.AI
{
    public class ThinkNode_CheckNearbyPawns : ThinkNode_Priority
    {
        private int CheckForEnemyTime; // Random 180 to 600
        public bool AttackOtherAnimals = false;

        public override ThinkResult TryIssueJobPackage(Pawn pawn)
        {
            Pawn mindStateTarget = pawn.mindState.enemyTarget as Pawn;

            // Has already a target? reapply attack job if needed.
            if (mindStateTarget != null)
            {
                return base.TryIssueJobPackage(pawn);
            }

            // Check for new target only every x ticks
            if (Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick < CheckForEnemyTime)
            {
                return ThinkResult.NoJob;
            }
            pawn.mindState.lastEngageTargetTick = Find.TickManager.TicksGame;

            CheckForEnemyTime = Rand.RangeInclusive(180, 600);

            mindStateTarget = FindTarget(pawn, JobGiver_AgressiveAttack.MaxSearchDistance, AttackOtherAnimals);

            if (mindStateTarget == null)
            {
                return ThinkResult.NoJob;
            }

            // target found: apply attack job
            pawn.mindState.enemyTarget = mindStateTarget;
            return base.TryIssueJobPackage(pawn);
        }


        // Find a valid attack target (no animals)
        public virtual Pawn FindTarget(Pawn pawn, float MaxAttackDistance, bool attackAnimals = false)
        {
            Predicate<Thing> predicate = (Thing t) =>
            {
                // Target is calling pawn
                if (t == pawn)
                    return false;

                Pawn pawn1 = t as Pawn;
                if (pawn1.Downed)
                    return false;

                if (!t.SpawnedInWorld || t.Destroyed || t.Position == IntVec3.Invalid)
                    return false;

                // No Animals?
                if (!attackAnimals && pawn1.RaceProps.Animal)
                    return false;

                // Not same type of animals
                if (pawn1.def == pawn.def)
                    return false;

                // From own faction
                if (pawn1.Faction == pawn.Faction)
                    return false;

                if (!GenSight.LineOfSight(pawn.Position, t.Position, false))
                    return false;

                return true;
            };

            Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position,
                                                                ThingRequest.ForGroup(ThingRequestGroup.Pawn),
                                                                PathEndMode.ClosestTouch,
                                                                TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn, false),
                                                                MaxAttackDistance,
                                                                predicate,
                                                                null,
                                                                2);

            if (pawn2 == null)
                return null;

            return pawn2;


            //IEnumerable<Pawn> foundPawns = Find.ListerPawns.AllPawns.Where(p =>
            //                                                                    !p.RaceProps.IsAnimal &&
            //                                                                    !p.InContainer &&
            //                                                                    !p.Destroyed && !p.Downed &&
            //                                                                    p.Position != IntVec3.Invalid &&
            //                                                                    p.def != pawn.def &&
            //                                                                    p.Position.InHorDistOf(pawn.Position, MaxAttackDistance) &&
            //                                                                    GenSight.LineOfSight(pawn.Position, p.Position, false));

            //if (foundPawns == null)
            //    return null;

            //return foundPawns.RandomElement<Pawn>();

        }

    }
}
