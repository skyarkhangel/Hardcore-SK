using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using Verse.AI;

namespace Nandonalt_ColonyLeadership
{
    public class IncidentWorker_Rebellion : IncidentWorker
    {
        private const int FixedPoints = 30;

        public static void removeLeadership(Pawn current)
        {
            HediffLeader h1 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            HediffLeader h2 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            HediffLeader h3 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            HediffLeader h4 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));

            if (h2 != null) h1 = h2;
            if (h3 != null) h1 = h3;
            if (h4 != null) h1 = h4;


            Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("leaderExpired"), current, null);
            hediff.Severity = 1f;
            HediffLeader hl = (HediffLeader)hediff;
            hl.ticksLeader = h1.ticksLeader;
            current.health.AddHediff(hl, null, null);
            current.health.RemoveHediff(h1);
            current.needs.AddOrRemoveNeedsAsAppropriate();

        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;

            List<Pawn> pawns = IncidentWorker_LeaderElection.getAllColonists();
            List<Pawn> tpawns2 = new List<Pawn>();

            foreach (Pawn current in pawns)
            {
                Hediff h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
                        Hediff h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
                Hediff h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
                Hediff h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
                if (h1 == null && h2 == null && h3 == null && h4 == null && current.Map == map) { tpawns2.Add(current); }
            }
            Pawn pawn = tpawns2.RandomElement(); 
                        
            if (pawn != null && !pawn.Downed && !pawn.Dead)
            {
                if (pawn.CurJob != null && pawn.jobs.curDriver.layingDown != LayingDownState.NotLaying)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                }
                pawn.mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("Rebelling"), null, false, false, null);
                String s = "He";
                if (pawn.gender == Gender.Female) s = "She";
                Find.LetterStack.ReceiveLetter("RebelLetter".Translate(), "RebelLetterDesc".Translate(new object[] { pawn.Name.ToStringShort }), LetterDefOf.BadUrgent, pawn, null);
                return true;

            }

            return false;
        }
    }
}