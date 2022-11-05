namespace Numbers
{
    using System.Linq;
    using RimWorld;
    using Verse;

    //todo: probably better with icons to hover over for a tooltip or smth
    public class PawnColumnWorker_Endogenes : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            return pawn.genes?.Endogenes?.Select(gene => gene.Label).ToCommaList();
        }
    }

    public class PawnColumnWorker_Xenogenes : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            return pawn.genes?.Xenogenes?.Select(gene => gene.Label).ToCommaList();
        }
    }
}
