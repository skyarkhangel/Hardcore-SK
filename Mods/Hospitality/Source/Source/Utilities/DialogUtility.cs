using UnityEngine;
using Verse;

namespace Hospitality
{
    internal static class DialogUtility
    {
        public static void LabelWithTooltip(string label, string tooltip, Rect rect)
        {
            Widgets.Label(rect, label);
            DoTooltip(rect, tooltip);
        }

        private static void DoTooltip(Rect rect, string tooltip)
        {
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(rect)) Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, tooltip);
            }
        }

        public static void CheckboxLabeled(Listing_Standard listing, string label, ref bool checkOn, Rect rect, bool disabled = false, string tooltip = null)
        {
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(rect))
                    Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, tooltip);
            }

            Widgets.CheckboxLabeled(rect, label, ref checkOn, disabled);
            listing.Gap(listing.verticalSpacing);
        }
    }
}
