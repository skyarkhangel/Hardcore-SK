using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace Nandonalt_ColonyLeadership
{
    internal class JobGiver_Rebellion : ThinkNode_JobGiver
    {
        private IntRange waitTicks = new IntRange(80, 140);

        private static List<Thing> potentialTargets = new List<Thing>();

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_Rebellion JobGiver_Rebellion = (JobGiver_Rebellion)base.DeepCopy(resolve);
            JobGiver_Rebellion.waitTicks = this.waitTicks;
            return JobGiver_Rebellion;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
        
            if (pawn.mindState.nextMoveOrderIsWait)
            {
                Job job = new Job(JobDefOf.WaitWander);
                job.expiryInterval = this.waitTicks.RandomInRange;
                pawn.mindState.nextMoveOrderIsWait = false;
                return job;
            }
            if (Rand.Value < 0.75f)
            {
           
                Pawn target = getMostHatedLeaderBy(pawn);
                if (target != null)
                {
                  
                    LocalTargetInfo dest = new LocalTargetInfo(target);

                   
                        if (!pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
                        {
                            pawn.mindState.nextMoveOrderIsWait = false;
                        }
                        else if (!pawn.CanReserve(dest.Thing, 1))
                        {
                            pawn.mindState.nextMoveOrderIsWait = false;
                        }
                        else
                        {
                            Pawn pTarg = (Pawn)dest.Thing;

                            Building_Bed building_Bed = RestUtility.FindBedFor(pTarg, pawn, true, false, false);
                            if (building_Bed == null)
                            {
                               
                                return null;
                            }
                            Job job = new Job(DefDatabase<JobDef>.GetNamed("ArrestLeader"), pTarg, building_Bed);
                         
                            job.count = 1;
                            pawn.mindState.nextMoveOrderIsWait = true;
                            return job;
                        }                                     
                  
                }
                else
                {
                    pawn.mindState.nextMoveOrderIsWait = false;
                }
                
            }
            IntVec3 intVec = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 10f, null, Danger.Deadly);
            if (intVec.IsValid)
            {
                pawn.mindState.nextMoveOrderIsWait = true;
                pawn.Map.pawnDestinationManager.ReserveDestinationFor(pawn, intVec);
                return new Job(JobDefOf.GotoWander, intVec);
            }
            return null;
        }

        public static Pawn getMostHatedLeaderBy(Pawn pawn)
        {
            List<Pawn> pawns = IncidentWorker_LeaderElection.getAllColonists();
            List<Pawn> tpawns2 = new List<Pawn>();

            foreach (Pawn current in pawns)
            {
                Hediff h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
                Hediff h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
                Hediff h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
                Hediff h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
                if (h1 != null || h2 != null || h3 != null || h4 != null) { tpawns2.Add(current); }
            }

            Pawn temp = null;
            float lastopinion = 100;
            foreach (Pawn p2 in tpawns2)
            {
                if (p2 != pawn)
                {
                    float opinion = pawn.relations.OpinionOf(p2);
                    if (opinion <= lastopinion)
                    {
                        lastopinion = opinion;
                        temp = p2;
                    }
                }
            }

            if (temp != null)
            {
                if (pawn.relations.OpinionOf(temp) > -60f) temp = null;
            }

            return temp;
        }
     
    }
}