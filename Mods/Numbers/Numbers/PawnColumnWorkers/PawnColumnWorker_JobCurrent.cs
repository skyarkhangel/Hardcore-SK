namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_JobCurrent : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            if (!Numbers_Settings.showMoreInfoThanVanilla && !(pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer) && !pawn.InMentalState)
                return null;

            if (pawn.jobs.curDriver != null)
            {
                string text = pawn.jobs.curDriver.GetReport().CapitalizeFirst();
                GenText.SetTextSizeToFit(text, new Rect(0f, 0f, Mathf.CeilToInt(Text.CalcSize(def.LabelCap).x), GetMinCellHeight(pawn)));

                return text;
            }
            return null;
        }

        protected override string GetTip(Pawn pawn) => GetTextFor(pawn);

        public override int Compare(Pawn a, Pawn b)
            => (a.jobs?.curDriver.GetReport()).CompareTo(b.jobs?.curDriver.GetReport());

        public override int GetMinWidth(PawnTable table)
            => Mathf.Max(base.GetMinWidth(table), 200);

        public override int GetMinHeaderHeight(PawnTable table)
            => Mathf.CeilToInt(Text.CalcSize(Numbers_Utility.WordWrapAt(def.LabelCap, GetMinWidth(table))).y);
    }
}
