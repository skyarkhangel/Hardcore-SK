using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using GUILambda = System.Action<UnityEngine.Rect>;

namespace RocketMan
{
    public abstract class IListing_Custom
    {
        public const float ScrollViewWidthDelta = 25f;

        protected Vector2 margins = new Vector2(8, 4);

        protected float inXMin = 0f;

        protected float inXMax = 0f;

        protected float curYMin = 0f;

        protected float inYMin = 0f;

        protected float inYMax = 0f;

        protected float previousHeight = 0f;

        protected bool isOverflowing = false;

        protected Rect contentRect;

        private Rect inRect;

        private bool started = false;

        public readonly bool ScrollViewOnOverflow;

        public Vector2 ScrollPosition = Vector2.zero;

        public Color CollapsibleBGColor = Widgets.MenuSectionBGFillColor;

        public Color CollapsibleBGBorderColor = Widgets.MenuSectionBGBorderColor;

        protected struct RectSlice
        {
            public Rect inside;
            public Rect outside;

            public RectSlice(Rect inside, Rect outside)
            {
                this.outside = outside;
                this.inside = inside;
            }
        }

        protected virtual bool Overflowing
        {
            get => isOverflowing;
        }

        protected virtual float insideWidth
        {
            get => (inXMax - inXMin) - margins.x * 2f;
        }

        public virtual Vector4 Margins
        {
            get => this.margins;
        }

        public Rect Rect
        {
            get => new Rect(inXMin, curYMin, inXMax - inXMin, inYMax - curYMin);
            set
            {
                this.inXMin = value.xMin;
                this.inXMax = value.xMax;
                this.curYMin = value.yMin;
                this.inYMin = value.yMin;
                this.inYMax = value.yMax;
            }
        }

        public IListing_Custom(bool scrollViewOnOverflow = true)
        {
            this.ScrollViewOnOverflow = scrollViewOnOverflow;
        }

        protected virtual void Begin(Rect inRect, bool scrollViewOnOverflow = true)
        {
            this.inRect = inRect;
            if (ScrollViewOnOverflow && started && inRect.height < previousHeight)
            {
                isOverflowing = true;
                GUIUtility.StashGUIState();
                GUI.color = Color.white;
                contentRect = new Rect(0f, 0f, inRect.width - ScrollViewWidthDelta, previousHeight);
                this.inYMin = contentRect.yMin;
                this.Rect = contentRect;
                Widgets.BeginScrollView(inRect, ref ScrollPosition, contentRect);
                GUIUtility.RestoreGUIState();
            }
            else
            {
                this.inYMin = inRect.yMin;
                this.Rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            }
        }

        protected virtual void Start()
        {
            GUIUtility.StashGUIState();
            GUIFont.Font = GUIFontSize.Tiny;
            GUIFont.CurFontStyle.fontStyle = FontStyle.Normal;
        }

        protected virtual void Label(TaggedString text, string tooltip = null, bool hightlightIfMouseOver = true, GUIFontSize fontSize = GUIFontSize.Tiny, FontStyle fontStyle = FontStyle.Normal)
        {
            RectSlice slice = Slice(text.GetTextHeight(this.insideWidth));
            if (hightlightIfMouseOver)
            {
                Widgets.DrawHighlightIfMouseover(slice.outside);
            }
            GUIFont.Font = fontSize;
            GUIFont.CurFontStyle.fontStyle = fontStyle;
            Widgets.Label(slice.inside, text);
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(slice.outside, tooltip);
            }
        }

        protected virtual bool CheckboxLabeled(TaggedString text, ref bool checkOn, string tooltip = null, bool disabled = false, bool hightlightIfMouseOver = true, GUIFontSize fontSize = GUIFontSize.Tiny, FontStyle fontStyle = FontStyle.Normal)
        {
            bool changed = false;
            bool checkOnInt = checkOn;

            GUIFont.Font = fontSize;
            GUIFont.CurFontStyle.fontStyle = fontStyle;
            RectSlice slice = Slice(text.GetTextHeight(insideWidth - 23f));
            if (hightlightIfMouseOver)
            {
                Widgets.DrawHighlightIfMouseover(slice.outside);
            }
            GUIUtility.CheckBoxLabeled(slice.inside, text, ref checkOnInt, disabled: disabled, iconWidth: 23f, drawHighlightIfMouseover: false);
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(slice.outside, tooltip);
            }

            if (checkOnInt != checkOn)
            {
                checkOn = checkOnInt;
                changed = true;
            }
            return changed;
        }

        protected virtual void Columns(float height, IEnumerable<GUILambda> lambdas, float gap = 5, bool useMargins = false, Action fallback = null)
        {
            if (lambdas.Count() == 1)
            {
                Lambda(height, lambdas.First(), useMargins, fallback);
                return;
            }
            Rect rect = useMargins ? Slice(height).inside : Slice(height).outside;
            Rect[] columns = rect.Columns(lambdas.Count(), gap);
            int i = 0;
            foreach (GUILambda lambda in lambdas)
            {
                GUIUtility.ExecuteSafeGUIAction(() =>
                {
                    lambda(columns[i++]);
                }, fallback);
            }
        }

        protected virtual void DropDownMenu<T>(TaggedString text, T selection, Func<T, string> labelLambda, Action<T> selectedLambda, IEnumerable<T> options, bool disabled = false, GUIFontSize fontSize = GUIFontSize.Tiny, FontStyle fontStyle = FontStyle.Normal)
        {
            string selectedText = labelLambda(selection);

            GUIFont.Font = fontSize;
            GUIFont.CurFontStyle.fontStyle = fontStyle;

            Rect rect = Slice(selectedText.GetTextHeight(insideWidth - 23f)).inside;
            Rect[] columns = rect.Columns(2, 5);

            Widgets.Label(columns[0], text);

            if (Widgets.ButtonText(columns[1], selectedText, active: !disabled))
            {
                GUIUtility.DropDownMenu(labelLambda, selectedLambda, options);
            }
        }

        protected virtual void Lambda(float height, GUILambda contentLambda, bool useMargins = false, Action fallback = null)
        {
            RectSlice slice = Slice(height);
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                contentLambda(useMargins ? slice.inside : slice.outside);
            }, fallback);
        }

        protected virtual void Gap(float height = 9f)
        {
            Slice(height, includeMargins: false);
        }

        protected virtual void Line(float thickness)
        {
            Gap(height: 3.5f);
            Widgets.DrawBoxSolid(this.Slice(thickness, includeMargins: false).outside, this.CollapsibleBGBorderColor);
            Gap(height: 3.5f);
        }

        public virtual void End(ref Rect inRect)
        {
            Gap(height: 5);

            GUI.color = this.CollapsibleBGBorderColor;
            Widgets.DrawBox(new Rect(inXMin, inYMin, inXMax - inXMin, curYMin - inYMin));

            started = true;
            previousHeight = Mathf.Abs(inYMin - curYMin);
            if (isOverflowing)
            {
                Widgets.EndScrollView();
                if (started && inRect.height < previousHeight)
                {
                    GUI.color = this.CollapsibleBGBorderColor;
                    Widgets.DrawBox(new Rect(inRect.xMin, inRect.yMin, inRect.width - 25f, 1));
                    Widgets.DrawBox(new Rect(inRect.xMin, inRect.yMax - 1, inRect.width - 25f, 1));
                }
                inRect.yMin = Mathf.Min(curYMin + this.inRect.yMin, this.inRect.yMax);
            }
            else
            {
                inRect.yMin = curYMin;
            }
            isOverflowing = false;
            GUIUtility.RestoreGUIState();
        }

        protected virtual RectSlice Slice(float height, bool includeMargins = true)
        {
            Rect outside = new Rect(inXMin, curYMin, inXMax - inXMin, includeMargins ? height + margins.y : height);
            Rect inside = new Rect(outside);
            if (includeMargins)
            {
                inside.xMin += margins.x * 2;
                inside.xMax -= margins.x;
                inside.yMin += margins.y / 2f;
                inside.yMax -= margins.y / 2f;
            }
            this.curYMin += includeMargins ? height + margins.y : height;
            Widgets.DrawBoxSolid(outside, CollapsibleBGColor);
            return new RectSlice(inside, outside);
        }
    }
}
