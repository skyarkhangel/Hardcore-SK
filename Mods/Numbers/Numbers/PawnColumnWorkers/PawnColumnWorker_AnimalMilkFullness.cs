namespace Numbers
{
    using System.Linq;
    using RimWorld;
    using Verse;

    public class PawnColumnWorker_AnimalMilkFullness : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            if (pawn.ageTracker.CurLifeStage.milkable)
            {
                var comp = pawn.AllComps.FirstOrDefault(x => x is CompMilkable);

                if (comp != null)
                    return ((CompMilkable)comp).Fullness.ToStringPercent();
            }
            return null;
        }

        public override int Compare(Pawn a, Pawn b)
            => GetScoreFor(a).CompareTo(GetScoreFor(b));

        private float GetScoreFor(Pawn pawn)
        {
            if (pawn.ageTracker.CurLifeStage.milkable)
            {
                var comp = pawn.AllComps.FirstOrDefault(x => x is CompMilkable);

                if (comp != null)
                    return ((CompMilkable)comp).Fullness;
            }
            return -1;
        }
    }
}
