namespace Numbers
{
    using RimWorld;
    using Verse;

    //todo: probably better with icons to hover over for a tooltip or smth
    public class PawnColumnWorker_GenesRegrowTime : PawnColumnWorker_Text
    {
        private int GetTicksRemaining(Pawn pawn)
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.XenogermReplicating);

            if (hediff == null)
            {
                return 0;
            }

            var disappears = hediff.TryGetComp<HediffComp_Disappears>();

            if (disappears == null)
            {
                return 0;
            }

            return disappears.ticksToDisappear;
        }

        protected override string GetTextFor(Pawn pawn)
        {
            var ticks = GetTicksRemaining(pawn);

            if (ticks == 0)
            {
                return null;
            }

            return ticks.ToStringTicksToPeriod(shortForm: true);
        }


        public override int Compare(Pawn a, Pawn b) 
            => this.GetTicksRemaining(a).CompareTo(this.GetTicksRemaining(b));
    }

}
