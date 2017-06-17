using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Nandonalt_ColonyLeadership
{
    public class InteractionWorker_LeaderOnly : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {           
            Hediff h1 = initiator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            Hediff h2 = initiator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            Hediff h3 = initiator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            Hediff h4 = initiator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
            if (h1 != null || h2 != null || h3 != null || h4 != null) return 0.3f;
            else return 0f;
        }
    }
}

