using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace AnimalRangedVerbsUnlocker
{
    public class JobGiver_Manhunter : RimWorld.JobGiver_Manhunter
    {
        public List<VerbProperties> verbs;

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            Thing thing = this.FindPawnTarget(pawn);
            if (thing == null)
            {
                thing = this.FindTurretTarget(pawn);
            }
            if (thing == null)
            {
                return null;
            }
            if (thing.Position.AdjacentTo8Way(pawn.Position))
            {
                return this.MeleeAttackJob(pawn, thing);
            }
            if (thing != null && pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, false) && HasRangedVerb(pawn))
            {
                return this.RangedAttackJob(pawn, thing);
            }

            if (thing != null && pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, false))
            {
                return this.MeleeAttackJob(pawn, thing);
            }
            PawnPath pawnPath = PathFinder.FindPath(pawn.Position, thing.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false), PathEndMode.OnCell);
            if (!pawnPath.Found)
            {
                return null;
            }
            IntVec3 randomCell;
            try
            {
                IntVec3 loc;
                if (!pawnPath.TryFindLastCellBeforeBlockingDoor(out loc))
                {
                    Log.Error(pawn + " did TryFindLastCellBeforeDoor but found none when it should have been one.");
                    return null;
                }
                randomCell = CellFinder.RandomRegionNear(loc.GetRegion(), 9, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null).RandomCell;
            }
            finally
            {
                pawnPath.ReleaseToPool();
            }
            return new Job(JobDefOf.Goto, randomCell);
        }

        private bool HasRangedVerb(Pawn pawn)
        {
            this.verbs = pawn.VerbProperties;

            for (int i = 0; i < verbs.Count; i++)
            {
                if (verbs[i].range > 1.1f) return true;
            }
            return false;
        }


        private bool TryFindShootPosition(Pawn pawn, Thing target, out IntVec3 dest)
        {

            Verb verb = TryGetRangedVerb(pawn);
            if (verb == null)
            {
                dest = IntVec3.Invalid;
                return false;
            }

            return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                target = target,
                verb = verb,
                maxRangeFromTarget = verb.verbProps.range,
                wantCoverFromTarget = false
            }, out dest);
        }

        private Verb TryGetRangedVerb(Pawn pawn)
        {
            Verb verb = pawn.verbTracker.AllVerbs.Where(v => v.verbProps.range > 1.1f).RandomElement();
            return verb;
        }

        private Job MeleeAttackJob(Pawn pawn, Thing target)
        {
            return new Job(JobDefOf.AttackMelee, target)
            {
                maxNumMeleeAttacks = 1,
                expiryInterval = Rand.Range(420, 900),
                checkOverrideOnExpire = true
            };
        }

        private Job RangedAttackJob(Pawn pawn, Thing target)
        {
            if (target == null)
            {
                return null;
            }
            Verb verb = TryGetRangedVerb(pawn);
            if (verb == null)
            {
                return null;
            }

            IntVec3 intVec;
            if (!this.TryFindShootPosition(pawn, target, out intVec))
            {
                return null;
            }
            if (intVec == pawn.Position)
            {
                TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToAll;
                Thing thing = AttackTargetFinder.BestShootTargetFromCurrentPosition(pawn, null, verb.verbProps.range, verb.verbProps.minRange, targetScanFlags);
                if (thing != null)
                {
                    return new Job(DefDatabase<JobDef>.GetNamed("AnimalRangedAttack"), thing, verb.verbProps.warmupTicks, true)
                    {
                        verbToUse = verb
                    };
                }
            }
            Find.PawnDestinationManager.ReserveDestinationFor(pawn, intVec);
            return new Job(JobDefOf.Goto, intVec)
            {
                expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange,
                checkOverrideOnExpire = true
            };
        }

        private Thing FindPawnTarget(Pawn pawn)
        {
            Predicate<Thing> predicate = delegate(Thing t)
            {
                if (t == pawn)
                {
                    return false;
                }
                Pawn pawn2 = t as Pawn;
                return !pawn2.Downed && pawn2.RaceProps.intelligence >= Intelligence.ToolUser && pawn.HostileTo(pawn2);
            };
            Predicate<Thing> validator = predicate;
            return GenClosest.ClosestThingReachable(pawn.Position, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, true), 9999f, validator, null, -1, false);
        }

        private Thing FindTurretTarget(Pawn pawn)
        {
            return AttackTargetFinder.BestAttackTarget(pawn, (Thing t) => t is Building, 70f, 0f, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachable | TargetScanFlags.OnlyTargetCombatBuildings, default(IntVec3), 3.40282347E+38f);
        }
    }
}