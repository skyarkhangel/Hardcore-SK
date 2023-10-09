using System;
using UnityEngine;
using Verse;

using static PeteTimesSix.SimpleSidearms.UI.UIComponents;

namespace PeteTimesSix.SimpleSidearms.UI
{
    public static class ListingExtensions
    {
        private static readonly Color SelectedButtonColor = new Color(.65f, 1f, .65f);
        private static float ButtonTextPadding = 5f;
        private static float AfterLabelMinGap = 10f;

        public static float ColumnGap = 17f;

        public static void CheckboxLabeled(this Listing_Standard instance, string label, ref bool checkOn, string tooltip = null, Action onChange = null)
        {
            var valueBefore = checkOn;
            instance.CheckboxLabeled(label, ref checkOn, tooltip);
            if (checkOn != valueBefore)
            {
                onChange?.Invoke();
            }
        }

        public static void SliderLabeled(this Listing_Standard instance, string label, ref float value, float min, float max, float displayMult = 1, int decimalPlaces = 0, string valueSuffix = "", string tooltip = null, Action onChange = null)
        {
            instance.Label($"{label}: {(value * displayMult).ToString($"F{decimalPlaces}")}{valueSuffix}", tooltip: tooltip);
            var valueBefore = value;
            value = instance.Slider(value, min, max);
            if (value != valueBefore)
            {
                onChange?.Invoke();
            }
        }

        public static void Spinner(this Listing_Standard instance, string label, ref int value, int increment = 1, int? min = null, int? max = null, string tooltip = null, Action onChange = null)
        {
            float lineHeight = Text.LineHeight;
            float labelWidth = Text.CalcSize(label).x + AfterLabelMinGap;

            var rect = instance.GetRect(lineHeight);
            var buttonSize = lineHeight;

            var textRect = new Rect(rect.x + buttonSize + 1, rect.y, rect.width - buttonSize * 2 - 2f, rect.height);
            NumberField(ref value, textRect, onChange);

            var leftButtonRect = new Rect(rect.x, rect.y, buttonSize, buttonSize);
            var rightButtonRect = new Rect(rect.x + rect.width - buttonSize, rect.y, buttonSize, buttonSize);
            if (Widgets.ButtonText(leftButtonRect, "-") && (!min.HasValue || min <= value - increment))
            {
                value -= increment;
                onChange?.Invoke();
            }
            if (Widgets.ButtonText(rightButtonRect, "+") && (!max.HasValue || max >= value + increment))
            {
                value += increment;
                onChange?.Invoke();
            }
        }

        public static void NumberField(ref int value, Rect rect, Action onChange = null)
        {
            string valText = Widgets.TextField(rect, value.ToString());
            if (int.TryParse(valText, out int result))
            {
                if (value != result)
                {
                    value = result;
                    onChange?.Invoke();
                }
            }
            else
            {
                DrawBadTextValueOutline(rect);
            }
        }

        public static void EnumSelector<T>(this Listing_Standard listing, string label, ref T value, string valueLabelPrefix, string valueTooltipPostfix = "_tooltip", string tooltip = null, Action onChange = null) where T : Enum
        {
            string[] names = Enum.GetNames(value.GetType());

            float lineHeight = Text.LineHeight;
            float labelWidth = Text.CalcSize(label).x + AfterLabelMinGap;

            float buttonsWidth = 0f;
            foreach (var name in names)
            {
                string text = (valueLabelPrefix + name).Translate();
                float width = Text.CalcSize(text).x + ButtonTextPadding * 2f;
                if (buttonsWidth < width)
                    buttonsWidth = width;
            }

            bool fitsOnLabelRow = (((buttonsWidth * names.Length) + labelWidth) < (listing.ColumnWidth));
            float buttonsRectWidth = fitsOnLabelRow ?
                listing.ColumnWidth - (labelWidth + 1f) : //little extra to handle naughty pixels
                listing.ColumnWidth;

            int rowNum = 0;
            int columnNum = 0;
            int maxColumnNum = 0;
            foreach (var name in names)
            {
                if ((columnNum + 1) * buttonsWidth > buttonsRectWidth)
                {
                    columnNum = 0;
                    rowNum++;
                }
                float x = (columnNum * buttonsWidth);
                float y = rowNum * lineHeight;
                columnNum++;
                if (rowNum == 0 && maxColumnNum < columnNum)
                    maxColumnNum = columnNum;
            }
            rowNum++; //label row
            if (!fitsOnLabelRow)
                rowNum++;

            Rect wholeRect = listing.GetRect((float)rowNum * lineHeight);

            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(wholeRect))
                {
                    Widgets.DrawHighlight(wholeRect);
                }
                TooltipHandler.TipRegion(wholeRect, tooltip);
            }

            Rect labelRect = wholeRect.TopPartPixels(lineHeight).LeftPartPixels(labelWidth);
            GUI.color = Color.white;
            Widgets.Label(labelRect, label);

            Rect buttonsRect = fitsOnLabelRow ?
                wholeRect.RightPartPixels(buttonsRectWidth) :
                wholeRect.BottomPartPixels(wholeRect.height - lineHeight);

            buttonsWidth = buttonsRectWidth / (float)maxColumnNum;

            rowNum = 0;
            columnNum = 0;
            foreach (var name in names)
            {
                if ((columnNum + 1) * buttonsWidth > (buttonsRectWidth + 2f))
                {
                    columnNum = 0;
                    rowNum++;
                }
                float x = (columnNum * buttonsWidth);
                float y = rowNum * lineHeight;
                columnNum++;
                string buttonText = (valueLabelPrefix + name).Translate();
                var enumValue = (T)Enum.Parse(value.GetType(), name);
                GUI.color = value.Equals(enumValue) ? SelectedButtonColor : Color.white;
                var buttonRect = new Rect(buttonsRect.x + x, buttonsRect.y + y, buttonsWidth, lineHeight);
                if (valueTooltipPostfix != null)
                    TooltipHandler.TipRegion(buttonRect, (valueLabelPrefix + name + valueTooltipPostfix).Translate());
                bool clicked = Widgets.ButtonText(buttonRect, buttonText);
                if (clicked && !value.Equals(enumValue))
                {
                    value = enumValue;
                    onChange?.Invoke();
                }
            }

            listing.Gap(listing.verticalSpacing);
            GUI.color = Color.white;
        }

        public static void CurveEditor(this Listing_Standard instance, ref SimpleCurve curve) 
        {
            var rect = instance.GetRect(100f);
            Widgets.DrawBoxSolid(rect, Color.black);
            var innerRect = rect.ContractedBy(2f);
            //SimpleCurveDrawer.DrawCurve(innerRect, curve);
            Widgets.ButtonInvisibleDraggable(innerRect);
        }

        public static Listing_Standard BeginHiddenSection(this Listing_Standard instance, out float maxHeightAccumulator)
        {
            Rect rect = instance.GetRect(0);
            rect.height = 10000f; 
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect);
            maxHeightAccumulator = 0f;
            return listing_Standard;
        }

        public static void NewHiddenColumn(this Listing_Standard instance, ref float maxHeightAccumulator)
        {
            if (maxHeightAccumulator < instance.CurHeight)
                maxHeightAccumulator = instance.CurHeight;
            instance.NewColumn();
        }

        public static void EndHiddenSection(this Listing_Standard instance, Listing_Standard section, float maxHeightAccumulator)
        {
            instance.GetRect(Mathf.Max(section.CurHeight, maxHeightAccumulator));
            section.End();
        }
    }
}
