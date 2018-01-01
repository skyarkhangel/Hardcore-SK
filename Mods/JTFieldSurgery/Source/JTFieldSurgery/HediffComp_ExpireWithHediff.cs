using Verse;

namespace JTFieldSurgery
{

    /**
     * <summary>If the targetted hediff is removed/not present, this hediff is also removed.</summary>
     */
    class HediffComp_ExpireWithHediff : HediffComp
    {
        protected const int SeverityUpdateInterval = 200;

        HediffCompProperties_ExpireWithHediff Props {
            get {
                return (HediffCompProperties_ExpireWithHediff)props;
            }
        }

        public override void CompPostTick (ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick (SeverityUpdateInterval)) {
                if (!Pawn.health.hediffSet.HasHediff (Props.hediffDef)) {
                    Pawn.health.RemoveHediff (parent);
                }
            }
        }
    }

    public class HediffCompProperties_ExpireWithHediff : HediffCompProperties
    {
        public HediffDef hediffDef;

        public HediffCompProperties_ExpireWithHediff ()
        {
            compClass = typeof (HediffComp_ExpireWithHediff);
        }
    }

}
