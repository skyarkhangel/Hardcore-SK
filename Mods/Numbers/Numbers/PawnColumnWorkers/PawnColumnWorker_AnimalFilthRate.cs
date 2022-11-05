namespace Numbers
{
    using RimWorld;
    using Verse;

    public class PawnColumnWorker_AnimalFilthRate : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            return GetScoreFor(pawn).ToString("F2");
        }

        public override int Compare(Pawn a, Pawn b)
            => GetScoreFor(a).CompareTo(GetScoreFor(b));

        private float GetScoreFor(Pawn pawn)
        {
            return pawn.GetStatValueForPawn(StatDefOf.FilthRate, pawn);
        }
    }
}