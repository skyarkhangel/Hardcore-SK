using Verse;

namespace JTFieldSurgery
{

    /**
     *<summary>If the targetted hediff is less severe, this hediff is reduced to match its severity. If the targetted hediff is removed, this hediff is also removed.</summary>
     */
    class HediffComp_CapToHediff : HediffComp
    {
        protected const int SeverityUpdateInterval = 200;

        HediffCompProperties_CapToHediff Props {
            get {
                return (HediffCompProperties_CapToHediff)props;
            }
        }

        public override void CompPostTick (ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick (SeverityUpdateInterval)) {
                if (Pawn.health.hediffSet.HasHediff (Props.hediffDef)) {
                    var target_hediff = Pawn.health.hediffSet.GetFirstHediffOfDef (Props.hediffDef);
                    if (parent.Severity > target_hediff.Severity) parent.Severity = target_hediff.Severity;
                }
            }
        }
    }

    public class HediffCompProperties_CapToHediff : HediffCompProperties
    {
        public HediffDef hediffDef;

        public HediffCompProperties_CapToHediff ()
        {
            compClass = typeof (HediffComp_CapToHediff);
        }
    }

}