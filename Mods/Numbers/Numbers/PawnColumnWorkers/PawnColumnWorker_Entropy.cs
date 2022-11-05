namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    public class PawnColumnWorker_Entropy : PawnColumnWorker
    {
        private static readonly Texture2D EntropyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.46f, 0.34f, 0.35f));

        //mostly from PawnColumnWorker_Need

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (!pawn.HasPsylink)
                return;

            float curEntropyLevel = pawn.psychicEntropy.EntropyRelativeValue;

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

            Widgets.FillableBar(rect3, curEntropyLevel, EntropyBarTex);

            Text.Font = GameFont.Small;
        }

        public override int GetMinWidth(PawnTable table)
            => Mathf.Max(base.GetMinWidth(table), 110);

        public override int Compare(Pawn a, Pawn b)
        {
            int hasPsylink = a.HasPsylink.CompareTo(b.HasPsylink);
            if (hasPsylink != 0) { return hasPsylink; }
            return (a.psychicEntropy?.EntropyRelativeValue ?? 0f).CompareTo(b.psychicEntropy?.EntropyRelativeValue ?? 0f);
        }
    }
}
