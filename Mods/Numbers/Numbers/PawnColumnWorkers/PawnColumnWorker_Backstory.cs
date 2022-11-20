namespace Numbers
{
    using RimWorld;
    using Verse;

    public class PawnColumnWorker_Backstory : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
            => pawn.story.TitleShortCap;

        public override int GetMinWidth(PawnTable table)
            => 80;

        public override int Compare(Pawn a, Pawn b)
            => a.story.TitleShortCap.CompareTo(b.story.TitleShortCap);
    }
}
