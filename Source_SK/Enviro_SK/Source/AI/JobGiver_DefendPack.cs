using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace SK_Enviro.AI
{
    public class JobGiver_DefendPack : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            ThingRequest pawnReq = ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            Predicate<Thing> hostilePredicate = t => 
            {
                Pawn hostile = t as Pawn;
                return ( ((Pawn)pawn).Faction.HostileTo(Faction.OfColony) && !hostile.Dead && !hostile.Downed && 
                    hostile.Faction.HostileTo(Faction.OfColony) && !hostile.IsPrisonerOfColony);
            };
            
            Pawn closestEnemy = 
                GenClosest.ClosestThingReachable(pawn.Position, pawnReq, 
                    PathEndMode.OnCell, TraverseParms.For(pawn), 100f, hostilePredicate) as Pawn;

            if (closestEnemy == null || closestEnemy == pawn || !GenSight.LineOfSight(pawn.Position, closestEnemy.Position))
                return null;

            return new Job(JobDefOf.AttackMelee)
            {
                targetA=closestEnemy,
                expiryInterval = 200,
            };
        }
    }
}
