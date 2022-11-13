namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    class PawnColumnWorker_Faction : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
            => pawn.Faction?.Name;

        public override int GetMinWidth(PawnTable table)
            => Mathf.Max(base.GetMinWidth(table), 150);

        public override int Compare(Pawn a, Pawn b)
            => GetTextFor(a).CompareTo(GetTextFor(b));
    }
}
