namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_Psyfocus : PawnColumnWorker
    {
        //mostly from PawnColumnWorker_Need

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (!pawn.HasPsylink)
                return;

            float curPsyfocusLevel = pawn.psychicEntropy.CurrentPsyfocus;
            float targetPsyfocusLevel = pawn.psychicEntropy.TargetPsyfocus;

            float barHeight = 14f;
            float barWidth = barHeight + 15f;
            if (rect.height < 50f)
            {
                barHeight *= Mathf.InverseLerp(0f, 50f, rect.height);
            }

            Text.Font = (rect.height <= 55f) ? GameFont.Tiny : GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
            rect3 = new Rect(rect3.x + barWidth, rect3.y, rect3.width - barWidth * 2f, rect3.height - barHeight);

            Widgets.FillableBar(rect3, curPsyfocusLevel);

            DrawPsyfocusTargetMarkerAt(rect3, targetPsyfocusLevel);
            Text.Font = GameFont.Small;
        }

        private void DrawPsyfocusTargetMarkerAt(Rect barRect, float pct)
        {
            float seekerSize = 12f;
            if (barRect.width < 150f)
            {
                seekerSize /= 2f;
            }
            Vector2 vector = new Vector2(barRect.x + barRect.width * pct, barRect.y + barRect.height);
            Rect position = new Rect(vector.x - seekerSize / 2f, vector.y, seekerSize, seekerSize);
            GUI.DrawTexture(position, StaticConstructorOnGameStart.BarInstantMarkerTex);
        }

        public override int GetMinWidth(PawnTable table)
            => Mathf.Max(base.GetMinWidth(table), 110);

        public override int Compare(Pawn a, Pawn b)
        {
            int hasPsylink = a.HasPsylink.CompareTo(b.HasPsylink);
            if (hasPsylink != 0) { return hasPsylink; }
            return (a.psychicEntropy?.CurrentPsyfocus ?? 0f).CompareTo(b.psychicEntropy?.CurrentPsyfocus ?? 0f);
        }
    }
}
