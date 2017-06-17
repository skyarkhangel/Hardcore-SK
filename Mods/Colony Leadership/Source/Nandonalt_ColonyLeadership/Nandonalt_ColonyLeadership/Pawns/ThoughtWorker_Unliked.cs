using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Nandonalt_ColonyLeadership
{
    public class ThoughtWorker_Unliked : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            Need_LeaderLevel need = (Need_LeaderLevel)p.needs.TryGetNeed(DefDatabase<NeedDef>.GetNamed("LeaderLevel"));
            if(need == null)
            {
                return ThoughtState.Inactive;
            }
            else
            {
                if (need.opinion <= -50)
                {
                    return ThoughtState.ActiveAtStage(1);
                }
                else if (need.opinion <= -10)
                {
                    return ThoughtState.ActiveAtStage(0);
                }
                
            }
            return ThoughtState.Inactive;

        }
    }
}
