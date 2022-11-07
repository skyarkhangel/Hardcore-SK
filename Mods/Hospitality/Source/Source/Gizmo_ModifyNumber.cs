using System;
using System.Linq;
using Hospitality.Utilities;
using UnityEngine;
using Verse;

namespace Hospitality
{
    public abstract class Gizmo_ModifyNumber<T> : Gizmo
    {
        protected readonly T[] selection;

        private const int LabelRowX = 110;
        private const int MainRectWidth = 175;
        protected abstract string Title { get; }
        protected abstract Color ButtonColor { get; }
        protected abstract void ButtonDown();
        protected abstract void ButtonUp();
        protected abstract void ButtonCenter();
        protected abstract void DrawInfoRect(Rect rect);
        public override float GetWidth(float maxWidth) => 200;

        protected Gizmo_ModifyNumber(T[] selection)
        {
            this.selection = selection;
            order = -25;
        }

        public override bool GroupsWith(Gizmo other)
        {
            return other is Gizmo_ModifyNumber<T>;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var totalRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Widgets.DrawWindowBackground(totalRect);

            var mainRect = totalRect.LeftPartPixels(MainRectWidth).ContractedBy(5);

            //Begin Group For Price Labels
            GUI.BeginGroup(mainRect);
            var rect = new Rect(0, 0, mainRect.width, mainRect.height);
            float curY = 0;

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            var title = Title;
            var size = Text.CalcSize(title);
            var labelRect = new Rect(0, curY, rect.width, size.y);
            curY += size.y;
            Widgets.Label(labelRect, title);

            //Draw Selling Config
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            var labelRowRect = new Rect(0, curY, mainRect.width, 20);
            DrawInfoRect(labelRowRect);

            Text.Anchor = default;
            Text.Font = default;

            GUI.EndGroup();

            var buttonsRect = totalRect.RightPartPixels((totalRect.height-10)/2.25f).ContractedBy(5);
            DrawButtons(buttonsRect);

            GenUI.AbsorbClicksInRect(totalRect);

            DrawTooltipBox(totalRect);
            
            return new GizmoResult(GizmoState.Mouseover);
        }

        protected abstract void DrawTooltipBox(Rect totalRect);

        private void DrawButtons(Rect rect)
        {
            var size = rect.width;
            var middleSetting = new Rect(rect.x, rect.center.y - size/2f, size, size);
            var upperSetting = new Rect(rect.x, middleSetting.y - size, size, size);
            var lowerSettings = new Rect(rect.x, middleSetting.yMax, size, size);

            if (Widgets.ButtonImage(middleSetting, HospitalityContent.ButtonNumberAuto, ButtonColor))
            {
                ButtonCenter();
            }

            if (Widgets.ButtonImage(upperSetting, HospitalityContent.ButtonNumberUp, ButtonColor))
            {
                ButtonUp();
            }

            if (Widgets.ButtonImage(lowerSettings, HospitalityContent.ButtonNumberDown, ButtonColor))
            {
                ButtonDown();
            }
        }

        protected static void LabelRow(ref Rect inRect, string title, string label, GameFont font = GameFont.Tiny)
        {
            Text.Font = font;
            var titleSize = Text.CalcSize(title);
            var labelSize = Text.CalcSize(label);

            var titleRect = new Rect(inRect.x, inRect.y, titleSize.x, titleSize.y);
            var labelRect = new Rect(inRect.x + LabelRowX, inRect.y, labelSize.x, labelSize.y);

            Widgets.Label(titleRect, title);
            Widgets.Label(labelRect, label);

            Text.Font = default;
            inRect.y += inRect.height;
        }

        protected string ToFromToString<TValue>(Func<T, TValue> getValue, Func<TValue, string> format)
        {
            var min = selection.Min(getValue);
            var max = selection.Max(getValue);

            if (min.Equals(max)) return format(min);
            return $"{format(min)} - {format(max)}";
        }
    }
}