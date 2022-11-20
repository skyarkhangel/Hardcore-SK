namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_MentalState : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
            => pawn.MentalState?.InspectLine ?? string.Empty;

        public override int Compare(Pawn a, Pawn b)
            => ((int?)a.MentalState?.def?.category ?? -1)
                .CompareTo((int?)b.MentalState?.def?.category ?? -1);

        public override int GetMinHeaderHeight(PawnTable table)
            => Mathf.CeilToInt(Text.CalcSize(Numbers_Utility.WordWrapAt(def.LabelCap, GetMinWidth(table))).y);
    }
}
