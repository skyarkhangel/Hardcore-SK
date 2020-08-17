using UnityEngine;
using Verse;

namespace Hospitality.MainTab
{
    internal class PawnColumnWorker_AccommodationArea : PawnColumnWorker_AreaBase
    {
        protected override Area GetArea(Pawn pawn)
        {
            var comp = pawn.CompGuest();
            return comp?.GuestArea;
        }

        protected override void SetArea(Pawn pawn, Area area)
        {
            var comp = pawn.CompGuest();
            if (comp != null) comp.GuestArea = area;
        }

        protected override void DrawTopArea(Rect rect)
        {
            rect.width -= 10;
            rect.x += 5;
            if (Widgets.ButtonText(rect, "ManageDefaults".Translate(), true, false))
            {
                Find.WindowStack.Add(new Dialog_ManageDefaults(Find.CurrentMap));
            }
        }
    }
}
