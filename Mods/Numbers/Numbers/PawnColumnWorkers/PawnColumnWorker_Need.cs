namespace Numbers
{
    using System.Collections.Generic;
    using System.Reflection;
    using RimWorld;
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    public class PawnColumnWorker_Need : PawnColumnWorker
    {
        private static readonly FieldInfo needThreshPercent = typeof(Need).GetField("threshPercents", BindingFlags.NonPublic | BindingFlags.Instance);

        //mostly from Koisama
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn.needs == null)
                return;

            if (pawn.RaceProps.IsMechanoid)
                return;

            if (!Numbers_Settings.showMoreInfoThanVanilla && pawn.RaceProps.Animal && pawn.Faction == null)
                return;

            Need need = pawn.needs.TryGetNeed(def.Ext().need);

            if (need == null)
                return;

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

            Widgets.FillableBar(rect3, need.CurLevelPercentage);
            Widgets.FillableBarChangeArrows(rect3, need.GUIChangeArrow);

            List<float> threshPercents = (List<float>)needThreshPercent.GetValue(need);
            if (threshPercents != null)
            {
                foreach (float t in threshPercents)
                {
                    NeedDrawBarThreshold(rect3, t, need.CurLevelPercentage);
                }
            }

            float curInstantLevel = need.CurInstantLevelPercentage;
            if (curInstantLevel >= 0f)
            {
                NeedDrawBarInstantMarkerAt(rect3, curInstantLevel);
            }
            Text.Font = GameFont.Small;
        }

        private void NeedDrawBarThreshold(Rect barRect, float threshPct, float curLevel)
        {
            float num = (barRect.width <= 60f) ? 1 : 2;
            Rect position = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
            Texture2D image;
            if (threshPct < curLevel)
            {
                image = BaseContent.BlackTex;
                GUI.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else
            {
                image = BaseContent.GreyTex;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }
            GUI.DrawTexture(position, image);
            GUI.color = Color.white;
        }

        private void NeedDrawBarInstantMarkerAt(Rect barRect, float pct)
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
            => (a.needs?.TryGetNeed(def.Ext().need)?.CurLevel ?? 0)
                .CompareTo(b.needs?.TryGetNeed(def.Ext().need)?.CurLevel ?? 0);
    }
}
