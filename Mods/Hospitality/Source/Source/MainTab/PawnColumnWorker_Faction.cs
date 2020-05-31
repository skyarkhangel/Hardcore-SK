using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality.MainTab
{
    public class PawnColumnWorker_Faction : PawnColumnWorker_Text
    {
        public override int GetOptimalWidth(PawnTable table)
        {
            return Mathf.Clamp(200, GetMinWidth(table), GetMaxWidth(table));
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn.Faction != null)
            {
                var rect2 = rect;
                rect2.width -= 10;
                var color = pawn.Faction.Color;
                color.a = 0.2f;
                Widgets.DrawBoxSolid(rect2, color);
            }

            base.DoCell(rect, pawn, table);
        }

        protected override string GetTextFor(Pawn pawn)
        {
            if (pawn.Faction == null) return string.Empty;
            return pawn.Faction.Name;
        }

        public override int Compare(Pawn a, Pawn b)
        {
            return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
        }

        private static int GetValueToCompare(Pawn pawn)
        {
            if (pawn.Faction == null)
            {
                return -2147483648;
            }

            return pawn.Faction.loadID;
        }
    }
}
