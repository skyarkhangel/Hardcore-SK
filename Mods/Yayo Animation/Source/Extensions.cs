using Verse;

namespace yayoAni
{
    public static class Extensions
    {
        public static bool ButtonTextTooltip(this Listing_Standard listing, string label, string tooltip, string highlightTag = null, float widthPct = 1f)
        {
#if IDEOLOGY
            var rect = listing.GetRect(30f);
#else
            var rect = listing.GetRect(30f, widthPct);
#endif
            var pressed = false;

            if (listing.BoundingRectCached == null || rect.Overlaps(listing.BoundingRectCached.Value))
            {
                pressed = Widgets.ButtonText(rect, label);
                if (highlightTag != null) 
                    UIHighlighter.HighlightOpportunity(rect, highlightTag);
            }

            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(rect))
                    Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, tooltip);
            }

            listing.Gap(listing.verticalSpacing);
            return pressed;
        }
    }
}