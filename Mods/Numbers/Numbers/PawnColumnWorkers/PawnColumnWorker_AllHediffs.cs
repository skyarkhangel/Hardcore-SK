namespace Numbers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_AllHediffs : PawnColumnWorker_Icon
    {
        protected override Texture2D GetIconFor(Pawn pawn)
            => VisibleHediffs(pawn).Any()
                ? StaticConstructorOnGameStart.Plus
                : null;

        protected override string GetIconTip(Pawn pawn)
        {
            StringBuilder icontipBuilder = new StringBuilder();
            foreach (IGrouping<BodyPartRecord, Hediff> diffs in VisibleHediffGroupsInOrder(pawn))
            {
                foreach (IGrouping<int, Hediff> current in diffs.GroupBy(x => x.UIGroupKey))
                {
                    int count = current.Count();
                    string text = current.First().LabelCap;
                    if (count != 1)
                    {
                        text = text + " x" + count;
                    }
                    icontipBuilder.AppendWithComma(text);
                }
            }
            return icontipBuilder.ToString();
        }

        public override int Compare(Pawn a, Pawn b)
            => VisibleHediffs(a).Count().CompareTo(VisibleHediffs(b).Count());

        protected override string GetHeaderTip(PawnTable table)
            => base.GetHeaderTip(table) + "\n\n" + "Numbers_ColumnHeader_Tooltip".Translate();

        public override void DoHeader(Rect rect, PawnTable table)
        {
            base.DoHeader(rect, table);
            GUI.color = Color.cyan;
            float scale = 0.7f;
            int sizeOfVanillaRescueTex = 24;

            Vector2 headerIconSize = new Vector2(
                Mathf.Min(sizeOfVanillaRescueTex, StaticConstructorOnGameStart.Plus.width) * scale,
                Mathf.Min(sizeOfVanillaRescueTex, StaticConstructorOnGameStart.Plus.height) * scale);

            int xOffSet = (int)((rect.width - headerIconSize.x) / 4f);
            Rect position = new Rect(
                x: rect.x + xOffSet,
                y: rect.yMax - Mathf.Min(sizeOfVanillaRescueTex, StaticConstructorOnGameStart.Plus.height),
                width: headerIconSize.x,
                height: headerIconSize.y);

            GUI.DrawTexture(position, StaticConstructorOnGameStart.Plus);
            GUI.color = Color.white;
        }

        protected virtual IEnumerable<Hediff> VisibleHediffs(Pawn pawn)
        {
            List<Hediff_MissingPart> mpca = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
            foreach (Hediff_MissingPart t in mpca)
            {
                yield return t;
            }

            IEnumerable<Hediff> visibleDiffs = pawn.health.hediffSet.hediffs.Where(d => !(d is Hediff_MissingPart) && d.Visible);
            foreach (Hediff diff in visibleDiffs)
            {
                yield return diff;
            }
        }

        private IEnumerable<IGrouping<BodyPartRecord, Hediff>> VisibleHediffGroupsInOrder(Pawn pawn)
            => VisibleHediffs(pawn)
                .GroupBy(x => x.Part)
                .OrderByDescending(x => GetListPriority(x.First().Part));

        private static float GetListPriority(BodyPartRecord rec)
            => rec == null
                ? 9999999f
                : (int)rec.height * 10000 + rec.coverageAbsWithChildren;
    }
}
