namespace Numbers
{
    using System;
    using UnityEngine;
    using Verse;

    static class Numbers_SettingsHelper
    {
        public static void SliderLabeled(this Listing_Standard ls, string label, ref int val, string format, float min = 0f, float max = 100f, string tooltip = null)
        {
            float fVal = val;
            ls.SliderLabeled(label: label, val: ref fVal, format: format, min: min, max: max);
            val = (int)fVal;
        }
        public static void SliderLabeled(this Listing_Standard ls, string label, ref float val, string format, float min = 0f, float max = 1f, string tooltip = null)
        {
            Rect rect = ls.GetRect(height: Text.LineHeight);
            Rect rect2 = rect.LeftPart(pct: .70f).Rounded();
            Rect rect3 = rect.RightPart(pct: .30f).Rounded().LeftPart(pct: .67f).Rounded();
            Rect rect4 = rect.RightPart(pct: .10f).Rounded();

            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect: rect2, label: label);

            float result = Widgets.HorizontalSlider(rect: rect3, value: val, leftValue: min, rightValue: max, middleAlignment: true);
            val = result;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect: rect4, label: String.Format(format: format, arg0: val));
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect: rect, tip: tooltip);
            }

            Text.Anchor = anchor;
            ls.Gap(gapHeight: ls.verticalSpacing);
        }
    }
}
