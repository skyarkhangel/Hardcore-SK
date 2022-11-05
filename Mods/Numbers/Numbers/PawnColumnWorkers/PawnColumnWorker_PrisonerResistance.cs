namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_PrisonerResistance : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            return pawn.guest.Resistance.ToString("F1");
        }

        public override int Compare(Pawn a, Pawn b) => a.guest.Resistance.CompareTo(b.guest.Resistance);

        public override int GetMinHeaderHeight(PawnTable table) => Mathf.CeilToInt(Text.CalcSize(Numbers_Utility.WordWrapAt(def.LabelCap, GetMinWidth(table))).y);
    }
}
