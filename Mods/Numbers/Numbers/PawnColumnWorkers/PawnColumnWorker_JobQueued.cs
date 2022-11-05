namespace Numbers
{
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_JobQueued : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            if (pawn.jobs?.jobQueue.Any() ?? false)
            {
                string text = pawn.jobs.jobQueue[0].job.GetReport(pawn).CapitalizeFirst();

                GenText.SetTextSizeToFit(text, new Rect(0f, 0f, Mathf.CeilToInt(Text.CalcSize(def.LabelCap).x), GetMinCellHeight(pawn)));

                return text;
            }
            return null;
        }

        public override int Compare(Pawn a, Pawn b)
            => (a.jobs?.jobQueue?.Count ?? 0).CompareTo(b.jobs?.jobQueue?.Count ?? 0);

        protected override string GetTip(Pawn pawn)
            => pawn.jobs?.jobQueue?.Count.ToString();

        public override int GetMinWidth(PawnTable table)
            => Mathf.Max(base.GetMinWidth(table), 200);

        public override int GetMinHeaderHeight(PawnTable table)
            => Mathf.CeilToInt(Text.CalcSize(Numbers_Utility.WordWrapAt(def.LabelCap, GetMinWidth(table))).y);
    }
}
