namespace Numbers
{
    using System.Linq;
    using RimWorld;
    using Verse;

    public class PawnColumnWorker_Trait : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            return pawn.story?.traits?.allTraits.Select(x => x.Label).ToCommaList();
        }
    }
}
