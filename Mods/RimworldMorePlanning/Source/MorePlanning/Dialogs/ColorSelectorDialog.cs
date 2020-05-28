using System;
using MorePlanning.Common;
using MorePlanning.Plan;
using UnityEngine;
using Verse;
using Resources = MorePlanning.Common.Resources;

namespace MorePlanning.Dialogs
{
    public class ColorSelectorDialog : Window
    {
        protected AdaptableColor Color;

        protected string InputColorHex;

        protected string HexColorBefore;

        protected float Slider;

        protected float S;
        protected float V;

        protected int NumColor;

        protected bool AcceptColor;

        public override Vector2 InitialSize => new Vector2(416f, 292f);

        public ColorSelectorDialog(int numColor)
        {
            NumColor = numColor;
            Color = new AdaptableColor(PlanColorManager.PlanColor[numColor]);

            InputColorHex = Color.HexColor;
            Slider = Color.H;
            S = Color.S;
            V = Color.V;

            HexColorBefore = InputColorHex;
            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override void PreClose()
        {
            if (AcceptColor == false)
            {
                PlanColorManager.ChangeColor(NumColor, HexColorBefore);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            var beforeColor = Color.Color;

            var colorSb = new Rect(0, 0, 10, 10)
            {
                center = new Vector2(256 * S, 256 - 256 * V)
            };

            if (GUI.RepeatButton(new Rect(0, 0, 256, 256), ""))
            {
                Color.S = (Event.current.mousePosition.x - inRect.x) / 256f;
                Color.V = 1f - (Event.current.mousePosition.y - inRect.y) / 256f;
            }
            Widgets.DrawBoxSolid(new Rect(0, 0, 256, 256), Color.ObjColorH);
            GUI.DrawTexture(new Rect(0, 0, 256, 256), Resources.ColorPickerOverlay);
            GUI.DrawTexture(colorSb, Resources.ColorPickerSelect);

            GUI.DrawTexture(new Rect(275, 0, 19, 256), Resources.HsvSlider);

            var newSlider = GUI.VerticalSlider(new Rect(264f, 0f, 11, 256f), Slider, 1, 0);

            Widgets.DrawBoxSolid(new Rect(305, 0, 76, 76), Color.Color);

            var textHex = Widgets.TextField(new Rect(305f, 91f, 76f, 23f), InputColorHex);

            var defaultColorClicked = Widgets.ButtonText(new Rect(305f, 128f, 76f, 50f), "MorePlanning.DefaultColor".Translate());
            var okClicked = Widgets.ButtonText(new Rect(305f, 234f, 76f, 23f), "MorePlanning.Ok".Translate());

            if (Math.Abs(newSlider - Slider) > 0.01)
            {
                Color.H = newSlider;
                Slider = newSlider;
            }

            var colorHexChanged = false;
            if (textHex != InputColorHex)
            {
                Color.HexColor = "#" + textHex;
                InputColorHex = textHex;
                colorHexChanged = true;
            }

            if (defaultColorClicked)
            {
                Color.HexColor = "#" + PlanColorManager.DefaultColors[NumColor];
            }

            if (okClicked)
            {
                AcceptColor = true;
                Close();
            }

            if (!beforeColor.Equals(Color.Color))
            {
                Slider = Color.H;
                S = Color.S;
                V = Color.V;
                if (colorHexChanged == false)
                {
                    InputColorHex = Color.HexColor;
                }
                PlanColorManager.ChangeColor(NumColor, Color.HexColor);
            }
        }
    }
}
