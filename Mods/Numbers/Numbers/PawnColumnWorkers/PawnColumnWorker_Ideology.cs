namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_Ideology : PawnColumnWorker
    {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn?.Ideo == null)
            {
                return;
            }
            IdeoUIUtility.DrawIdeoPlate(rect, pawn.Ideo, pawn);
        }

        public override int GetMinWidth(PawnTable table)
        {
            return Mathf.Max(base.GetMinWidth(table), 140);
        }

        public override int Compare(Pawn a, Pawn b)
        {
            return (a.ideo?.Certainty ?? 0).CompareTo(b.ideo?.Certainty ?? 0);
        }
    }
}