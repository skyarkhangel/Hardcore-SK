using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality.MainTab
{
    internal class PawnColumnWorker_ShoppingArea : PawnColumnWorker_AreaBase
    {
        protected override Area GetArea(Pawn pawn)
        {
            var comp = pawn.CompGuest();
            return comp?.ShoppingArea;
        }

        protected override void SetArea(Pawn pawn, Area area)
        {
            var comp = pawn.CompGuest();
            if (comp != null) comp.ShoppingArea = area;
        }

        protected override void DrawTopArea(Rect rect)
        {
            rect.width -= 10;
            rect.x += 5;
            if (Widgets.ButtonText(rect, "ManageAreas".Translate(), true, false))
            {
                Find.WindowStack.Add(new Dialog_ManageAreas(Find.CurrentMap));
            }
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            // Changed check
            if (!pawn.IsGuest()) return;

            GenericUtility.DoAreaRestriction(rect, GetArea(pawn), area=>SetArea(pawn, area), GenericUtility.GetShoppingLabel);
        }
    }
}
