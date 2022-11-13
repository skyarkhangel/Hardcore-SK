namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_Inspiration : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
            => pawn.InspirationDef?.LabelCap;

        public override int Compare(Pawn a, Pawn b)
            => (a.Inspired ? a.InspirationDef.GetHashCode() : int.MinValue)
            .CompareTo(b.Inspired ? b.InspirationDef.GetHashCode() : int.MinValue);

        protected override string GetTip(Pawn pawn)
        {
            int? inspirationTimeRemaining = (int?)((pawn.InspirationDef?.baseDurationDays - pawn.Inspiration?.AgeDays) * GenDate.TicksPerDay);

            return inspirationTimeRemaining.HasValue ? "ExpiresIn".Translate().Resolve() + ": " + inspirationTimeRemaining.Value.ToStringTicksToPeriod() : string.Empty;
        }

        public override int GetMinWidth(PawnTable table)
            => Mathf.Max(base.GetMinWidth(table), 130);
    }
}
