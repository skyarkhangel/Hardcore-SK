using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace ColourPicker
{
    [StaticConstructorOnStartup]
    public class colourPicker : Window
    {
        public static readonly Texture2D hueMark = ContentFinder<Texture2D>.Get("DPA/UI/hueMark");
        public static readonly Texture2D hsbMark = ContentFinder<Texture2D>.Get("DPA/UI/hsbMark");

        private Vector2 HueSlider;

        public Rect StaticRect = new Rect(0, 0, 340, 280);

        public Texture2D HSBPicker = new Texture2D(255, 255);

        public Texture2D HuePicker = new Texture2D(1, 255);
        public bool draggingHue;
        public bool draggingHSB;

        public float paletteHeight = 0;

        public float hue;
        public float sat;
        public float val;

        public Color Blackist = new Color(0.06f, 0.06f, 0.06f);
        public Color Greyist = new Color(0.2f, 0.2f, 0.2f);

        public ColorInt InputRGB = new ColorInt();

        public string Rbuff = "";
        public string Gbuff = "";
        public string Bbuff = "";
        public string Hbuff = "";

        public static ColorInt CurrentColInt;

        public Action Setcol;

        public colourPicker()
        {
            for (int p = 0; p < 255; p++)
            {
                HuePicker.SetPixel(0, p, Color.HSVToRGB(Mathf.InverseLerp(0, 255, p), 1f, 1f));
            }

            HuePicker.Apply(false);

            for (int x = 0; x < 255; x++)
            {
                for (int y = 0; y < 255; y++)
                {
                    Color col = Color.clear;
                    Color col3 = Color.Lerp(col, Color.white, Mathf.InverseLerp(0, 255, x));
                    col = Color32.Lerp(Color.black, col3, Mathf.InverseLerp(0, 255, y));
                    HSBPicker.SetPixel(x, y, col);
                }
            }

            HSBPicker.Apply(false);
            layer = WindowLayer.Super;
            forcePause = false;
            absorbInputAroundWindow = false;
            closeOnCancel = true;
            soundAppear = SoundDefOf.CommsWindow_Open;
            soundClose = SoundDefOf.CommsWindow_Close;
            doCloseButton = false;
            doCloseX = true;
            draggable = false;
            drawShadow = true;
            preventCameraMotion = false;
            onlyOneOfTypeAllowed = true;
            resizeable = false;
        }

        public static Color CurrentCol => CurrentColInt.ToColor;

        //  public override float Margin => 0f;


        public override Vector2 InitialSize => new Vector2(StaticRect.width, StaticRect.height);

        public Color HexToColor(string hexColor)
        {
            ColorUtility.TryParseHtmlString("#" + hexColor, out Color col);
            return col;
        }

        public void SetColor(Color col, bool refresh = true)
        {
            CurrentColInt = new ColorInt(col);

            if (refresh)
            {
                Color.RGBToHSV(CurrentCol, out hue, out sat, out val);
            }
        }

        public void SetColor(string hex, bool refresh = true)
        {
            CurrentColInt = new ColorInt(HexToColor(hex));

            if (refresh)
            {
                Color.RGBToHSV(CurrentCol, out hue, out sat, out val);
            }
        }

        public void SetColor(float h, float s, float b, bool refresh = true)
        {
            CurrentColInt = new ColorInt(Color.HSVToRGB(h, s, b));

            try
            {
                Setcol();
            }
            catch (Exception)
            { }


            if (refresh)
            {
                Color.RGBToHSV(CurrentCol, out hue, out sat, out val);
            }
        }

        public override void PostClose()
        {
            base.PostClose();
            draggingHue = false;
            draggingHSB = false;
        }

        public override void PreClose()
        {
            base.PreClose();
            StaticRect = windowRect;
        }

        public override void SetInitialSizeAndPosition()
        {
            windowRect = new Rect(UI.MousePositionOnUI.x - InitialSize.x, UI.MousePositionOnUI.y - InitialSize.y,
                InitialSize.x, InitialSize.y);
            windowRect = windowRect.Rounded();
        }


        public override void DoWindowContents(Rect inRect)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Tiny;

            Rect DragRect = new Rect(inRect.x, inRect.y, inRect.width - 50f, 25f);
            GUI.DragWindow(DragRect);

            Widgets.DrawLine(new Vector2(DragRect.x, DragRect.y + DragRect.height * 0.25f),
                new Vector2(DragRect.xMax, DragRect.y + DragRect.height * 0.25f), Color.gray, 1f);
            Widgets.DrawLine(new Vector2(DragRect.x, DragRect.y + DragRect.height * 0.75f),
                new Vector2(DragRect.xMax, DragRect.y + DragRect.height * 0.75f), Color.gray, 1f);

            GUI.color = Color.grey;
            Widgets.Label(DragRect, "Drag");
            GUI.color = Color.white;

            DragRect.x = DragRect.xMax;
            DragRect.width = 25f;

            inRect = inRect.ContractedBy(10f);
            Rect MenuSection = inRect;
            MenuSection.y += 10f;
            HSB(ref MenuSection);

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        public void HSB(ref Rect MenuSection)
        {
            // Widgets.DrawBoxSolid(MenuSection, Greyist);
            Rect rekt = MenuSection.ContractedBy(10f);
            rekt.x = 40f;
            rekt.width = 15f;
            rekt.height = 220f;

            if (Input.GetMouseButtonDown(0) && Mouse.IsOver(rekt) && draggingHue == false)
            {
                draggingHue = true;
            }

            if (draggingHue)
            {
                float lastSlide = HueSlider.y;

                HueSlider.y = Mathf.InverseLerp(rekt.height, 0, Event.current.mousePosition.y - rekt.y);

                if (HueSlider.y != lastSlide)
                {
                    SetColor(HueSlider.y, sat, val);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                draggingHue = false;
            }


            Widgets.DrawBoxSolid(rekt.ExpandedBy(1f), Color.grey);
            Widgets.DrawTexturePart(rekt, new Rect(0, 0, 1, 1), HuePicker);

            Rect mark = new Rect(0, 0, 56, 16);
            mark.center = new Vector2(rekt.center.x, rekt.height * (1f - HueSlider.y) + rekt.y).Rounded();
            Widgets.DrawTextureRotated(mark, hueMark, 0f);


            rekt = MenuSection.ContractedBy(10f);
            rekt.height = 220f;
            rekt.x = rekt.xMax - rekt.height;
            rekt.width = rekt.height;

            if (Input.GetMouseButtonDown(0) && Mouse.IsOver(rekt) && draggingHSB == false)
            {
                draggingHSB = true;
            }

            if (draggingHSB)
            {
                sat = Mathf.InverseLerp(0, rekt.width, Event.current.mousePosition.x - rekt.x);
                val = Mathf.InverseLerp(rekt.width, 0, Event.current.mousePosition.y - rekt.y);

                //  if (LastMousePos != Event.current.mousePosition)
                //  {
                SetColor(hue, sat, val, false);
                // }
            }

            if (Input.GetMouseButtonUp(0))
            {
                draggingHSB = false;
            }

            Widgets.DrawBoxSolid(rekt.ExpandedBy(1f), Color.grey);
            Widgets.DrawBoxSolid(rekt, Color.white);

            Color col = Color.HSVToRGB(hue, 1f, 1f);
            GUI.color = col;
            Widgets.DrawTextureFitted(rekt, HSBPicker, 1f);
            GUI.color = Color.white;

            GUI.BeginClip(rekt);
            mark.center = new Vector2(rekt.width * sat, rekt.width * (1f - val));
            if (val < 0.4f || hue > 0.5f && sat > 0.5f)
            {
                ///
            }
            else
            {
                GUI.color = Blackist;
            }

            Widgets.DrawTextureFitted(mark, hsbMark, 1f);
            GUI.color = Color.white;

            GUI.EndClip();
        }
    }
}