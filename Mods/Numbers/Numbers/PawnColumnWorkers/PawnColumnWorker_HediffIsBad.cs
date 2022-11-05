namespace Numbers
{
    using System.Collections.Generic;
    using System.Linq;
    using Verse;

    public class PawnColumnWorker_HediffIsBad : PawnColumnWorker_AllHediffs
    {
        protected override IEnumerable<Hediff> VisibleHediffs(Pawn pawn)
        {
            return base.VisibleHediffs(pawn).Where(x => x.def.isBad);
        }
    }
}
