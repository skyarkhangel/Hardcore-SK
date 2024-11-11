using UnityEngine;
using Verse;

namespace StorageSelector.Patches.BillConfig
{
    public static class BillConfigLayout
    {
        public const float RepeatModeHeight = 200f;
        public const float StorageModeHeight = 70f;
        public const float WorkerSelectionHeight = 100f;

        public const float SectionGapBefore = 4f;
        public const float SectionGapAfter = 4f;
        public const float SectionGap = 8f;
        private const float ContentPadding = 4f;

        public static (Rect left, Rect middle, Rect right) GetColumnRects(Rect inRect)
        {
            float width = (inRect.width - 34f) / 3f;
            var left = new Rect(0f, 80f, width, inRect.height - 80f);
            var middle = new Rect(left.xMax + 17f, 50f, width, inRect.height - 50f - Window.CloseButSize.y);
            var right = new Rect(middle.xMax + 17f, 50f, width, inRect.height - 50f - Window.CloseButSize.y)
            {
                xMax = inRect.xMax
            };
            return (left, middle, right);
        }

        public static Rect GetHeaderRect()
        {
            return new Rect(40f, 0f, 400f, 34f);
        }

        public static Rect GetStyledButtonRect(Listing_Standard listing)
        {
            return listing.GetRect(30f);
        }

        public static Rect GetStyledSliderRect(Listing_Standard listing)
        {
            return listing.GetRect(31f);
        }

        public static Rect GetStyledInputRect(Listing_Standard listing)
        {
            return listing.GetRect(24f);
        }

        private static void DrawSectionBackground(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
        }

        public static void BeginStyled(this Listing_Standard listing, Rect rect)
        {
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            listing.Begin(rect);
        }

        public static (Listing_Standard section, Rect rect) BeginStyledSection(
            this Listing_Standard listing,
            float height,
            float gapBefore = 0f,
            float gapAfter = 0f)
        {
            if (gapBefore > 0f)
                listing.Gap(gapBefore);

            var outerRect = listing.GetRect(height);

            DrawSectionBackground(outerRect);

            var contentRect = outerRect.ContractedBy(ContentPadding);

            var section = new Listing_Standard();
            section.Begin(contentRect);

            if (gapAfter > 0f)
                listing.Gap(gapAfter);

            return (section, outerRect);
        }

        public static void EndStyledSection(this Listing_Standard listing, Listing_Standard section, Rect rect)
        {
            section.End();
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
        }
    }
}
