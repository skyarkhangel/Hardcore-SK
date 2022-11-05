namespace Numbers
{
    using RimWorld;
    using Verse;

    public class PawnColumnWorker_PsylinkLevel : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
            => (pawn.psychicEntropy?.Psylink?.level ?? 0).ToString();

        public override int Compare(Pawn a, Pawn b)
            => (a.psychicEntropy?.Psylink?.level ?? 0).CompareTo((b.psychicEntropy?.Psylink?.level ?? 0));

        public override int GetMinWidth(PawnTable table)
            => base.GetMinWidth(table) + 10;
    }
}
