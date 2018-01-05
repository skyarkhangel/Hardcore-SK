using System.Collections.Generic;
using RimWorld;
using Verse;

namespace JTFieldSurgery
{

    public class IngestionOutcomeDoer_TreatHediff : IngestionOutcomeDoer
    {
        ///<summary>(Optional) Single hediff to treat whenever this ingestible has been consumed.</summary>
        public HediffDef hediffDef;

        ///<summary>(Optional) List of hediffs to treat whenever this ingestible has been consumed.</summary>
        public List<HediffDef> hediffDefs = new List<HediffDef> ();

        protected override void DoIngestionOutcomeSpecial (Pawn pawn, Thing ingested)
        {
            foreach (Hediff diff in pawn.health.hediffSet.hediffs) {
                if (hediffDefs.Contains (diff.def)) diff.Tended (1.0f);
                else if (diff.def.defName == hediffDef.defName) diff.Tended (1.0f);
            }
        }
    }

}
