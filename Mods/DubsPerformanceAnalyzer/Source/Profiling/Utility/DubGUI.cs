using System;
using System.Reflection;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Analyzer.Profiling
{
    [StaticConstructorOnStartup]
    public static class DubGUI
    {
        public static Texture2D MintSearch = ContentFinder<Texture2D>.Get("DPA/UI/MintSearch", false);
        public static Texture2D DropDown = ContentFinder<Texture2D>.Get("DPA/UI/dropdown", false);
        public static Texture2D FoldUp = ContentFinder<Texture2D>.Get("DPA/UI/foldup", false);
        private static Texture2D aaLineTex;
        private static Texture2D lineTex;
        private static Material blitMaterial;
        private static Material blendMaterial;
        private static readonly Rect lineRect = new Rect(0, 0, 1, 1);

        static DubGUI()
        {
            Initialize();
        }

        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width, bool drawDouble = false)
        {
            if (Prefs.UIScale > 1)
            {
                Widgets.DrawLine(pointA, pointB, color, width);
                return;
            }

            var dx = pointB.x - pointA.x;
            var dy = pointB.y - pointA.y;
            var len = Mathf.Sqrt(dx * dx + dy * dy);

            if (len < 0.001f)
            {
                return;
            }

            width *= 3.0f;
            var wdx = width * dy / len;
            var wdy = width * dx / len;

            var matrix = Matrix4x4.identity;
            matrix.m00 = dx;
            matrix.m01 = -wdx;
            matrix.m03 = pointA.x + 0.5f * wdx;
            matrix.m10 = dy;
            matrix.m11 = wdy;
            matrix.m13 = pointA.y - 0.5f * wdy;

            GL.PushMatrix();
            GL.MultMatrix(matrix);
            for (int i = 0; i < (drawDouble ? 2 : 1); i++)
                Graphics.DrawTexture(lineRect, aaLineTex, lineRect, 0, 0, 0, 0, color, blendMaterial);
            GL.PopMatrix();
        }

        public static void DrawBezierLine(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent,
            Color color, float width, int segments)
        {
            var lastV = CubeBezier(start, startTangent, end, endTangent, 0);
            for (var i = 1; i < segments; ++i)
            {
                var v = CubeBezier(start, startTangent, end, endTangent, i / (float)segments);
                DrawLine(lastV, v, color, width);
                lastV = v;
            }
        }

        private static Vector2 CubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
        {
            var rt = 1 - t;
            return rt * rt * rt * s + 3 * rt * rt * t * st + 3 * rt * t * t * et + t * t * t * e;
        }

        private static void Initialize()
        {
            if (lineTex == null)
            {
                lineTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                lineTex.SetPixel(0, 1, Color.white);
                lineTex.Apply();
            }

            if (aaLineTex == null)
            {
                aaLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, false);
                aaLineTex.SetPixel(0, 0, new Color(1, 1, 1, 0));
                aaLineTex.SetPixel(0, 1, Color.white);
                aaLineTex.SetPixel(0, 2, new Color(1, 1, 1, 0));
                aaLineTex.Apply();
            }

            blitMaterial = (Material)typeof(GUI)
                .GetMethod("get_blitMaterial", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            blendMaterial = (Material)typeof(GUI)
                .GetMethod("get_blendMaterial", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
        }

        public static float ToMb(this long l)
        {
            return l / 1024f / 1024f;
        }

        public static bool Has(this string source, string toCheck,
            StringComparison comp = StringComparison.OrdinalIgnoreCase)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static void Clear(this StringBuilder value)
        {
            value.Length = 0;
            value.Capacity = 0;
        }

        public static void InlineTripleMessage(string left, string middle, string right, Listing_Standard listing,
            bool capOff)
        {
            left.Insert(0, " ");
            right.Insert(0, " ");

            var grongo = Text.CalcHeight(left, listing.ColumnWidth / 3f);
            var gronk = Text.CalcHeight(middle, listing.ColumnWidth / 3f - 5);
            var shiela = Text.CalcHeight(right, listing.ColumnWidth / 3f - 5);


            var rect = listing.GetRect(Mathf.Max(Mathf.Max(grongo, gronk), shiela));

            var leftRect = rect.LeftPart(.3f);
            var rightRect = rect.RightPart(.3f);
            var middleRect = rect.LeftPart(.3f);
            middleRect.x += rect.width / 3f + 5;
            rightRect.x += 5;

            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;

            Widgets.Label(leftRect, left);
            Widgets.Label(rightRect, right);
            Widgets.Label(middleRect, middle);
            Text.Anchor = anchor;

            var color = GUI.color;
            GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
            Widgets.DrawLineVertical(rect.width / 3, rect.y, rect.height);
            Widgets.DrawLineVertical(2 * (rect.width / 3), rect.y, rect.height);
            if (capOff)
                Widgets.DrawLineHorizontal(rect.x, rect.y + rect.height, rect.width);
            GUI.color = color;
        }

        public static Rect InlineDoubleMessage(string left, string right, Listing_Standard listing, bool capOff)
        {
            left.Insert(0, " ");
            right.Insert(0, " ");

            var grongo = Text.CalcHeight(left, listing.ColumnWidth / 2);
            var gronk = Text.CalcHeight(right, listing.ColumnWidth / 2 - 5f);

            var rect = listing.GetRect(Mathf.Max(grongo, gronk));
            var rr = rect;

            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;

            var leftRect = rect.LeftPart(.5f);
            Widgets.Label(leftRect, left);
            var rightRect = rect.RightPart(.5f);
            rightRect.x += 5;
            Widgets.Label(rightRect, right);

            Text.Anchor = anchor;

            var color = GUI.color;
            GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
            Widgets.DrawLineVertical(rect.center.x, rect.y, rect.height);
            if (capOff)
                Widgets.DrawLineHorizontal(rect.x, rect.y + rect.height, rect.width);
            GUI.color = color;

            return rr;
        }

        public static void InlineDoubleMessageNC(string left, string right, Listing_Standard listing, bool capOff)
        {
            left.Insert(0, " ");
            right.Insert(0, " ");

            var grongo = Text.CalcHeight(left, listing.ColumnWidth / 2);
            var gronk = Text.CalcHeight(right, listing.ColumnWidth / 2 - 5f);

            var rect = listing.GetRect(Mathf.Max(grongo, gronk));

            var leftRect = rect.LeftPart(.5f);
            Widgets.Label(leftRect, left);
            var rightRect = rect.RightPart(.5f);
            rightRect.x += 5;
            Widgets.Label(rightRect, right);

            var color = GUI.color;
            GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
            Widgets.DrawLineVertical(rect.center.x, rect.y, rect.height);
            if (capOff)
                Widgets.DrawLineHorizontal(rect.x, rect.y + rect.height, rect.width);
            GUI.color = color;
        }

        public static Rect Scale(this Rect rect, float w, float h)
        {
            var biff = new Rect(rect);
            rect.width = w;
            rect.height = h;
            return biff.CenteredOnXIn(rect);
        }

        public static Rect Morph(this Rect rect, float x = 0, float y = 0, float w = 0, float h = 0)
        {
            return rect = new Rect(rect.x + x, rect.y + y, rect.width + w, rect.height + h);
        }

        public static void CopyToClipboard(this string s)
        {
            var te = new TextEditor { text = s };
            te.SelectAll();
            te.Copy();
        }

        public static float SliderLabel(this Listing_Standard listing, string labia, float val, float min, float max)
        {
            var lineHeight = Text.LineHeight;
            var rect = listing.GetRect(lineHeight);

            Text.Font = GameFont.Tiny;
            Widgets.Label(rect.LeftHalf(), labia);
            var valkilmer = Widgets.HorizontalSlider(rect.RightHalf(), val, min, max);
            Text.Font = GameFont.Small;
            listing.Gap(listing.verticalSpacing);
            return valkilmer;
        }

        public static void LabeledSliderFloat(Listing_Standard listing, string label, ref float value, float min,
            float max, bool percent = false)
        {
            var anchor = Text.Anchor;
            var font = Text.Font;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;

            var rect = listing.GetRect(Text.LineHeight);
            if (percent) Widgets.Label(rect, $"{label}: {value * 100}%");
            else Widgets.Label(rect, $"{label}: {value:0.00}");
            rect = listing.GetRect(Text.LineHeight);

            var minWidth = min.ToString().GetWidthCached();
            var maxWidth = max.ToString().GetWidthCached();

            Widgets.Label(rect.LeftPartPixels(minWidth), min.ToString());
            Widgets.Label(rect.RightPartPixels(maxWidth), max.ToString());

            rect.x += minWidth + 5;
            rect.width -= minWidth + 5;

            value = Widgets.HorizontalSlider(rect.LeftPartPixels(rect.width - (maxWidth + 5)), value, min, max,
                roundTo: 0.05f);

            Text.Font = font;
            Text.Anchor = anchor;

            listing.Gap(listing.verticalSpacing);
        }

        public static bool Checkbox(Rect rect, string s, ref bool checkOn)
        {
            bool br = checkOn;
            if (Widgets.ButtonInvisible(rect))
            {
                checkOn = !checkOn;
                if (checkOn)
                {
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                }
                else
                {
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                }
            }

            var anchor = Text.Anchor;
            var font = Text.Font;
            Text.Anchor = TextAnchor.MiddleLeft;


            Text.Font = GameFont.Medium;
            var lineHeight = Text.LineHeight;
            Text.Font = font;
            var textureRect = rect.LeftPartPixels(30f);
            var height = rect.height;

            if (height > lineHeight)
            {
                textureRect = textureRect.TopPartPixels(lineHeight);
                textureRect.y += (height - lineHeight)/2;
            }

            Widgets.DrawTextureFitted(textureRect, checkOn ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex, 0.5f);
            rect.x += 30;
            rect.width -= 30;
            Widgets.Label(rect, s);
            Text.Anchor = anchor;
            if (checkOn != br)
            {
                return true;
            }

            return false;
        }

        public static bool Checkbox(string s, Listing_Standard listing, ref bool checkOn)
        {
            Rect rect = listing.GetRect(Mathf.CeilToInt(s.GetWidthCached() / listing.ColumnWidth) * Text.LineHeight);
            return Checkbox(rect, s, ref checkOn);
        }

        public static bool HeadingCheckBox(Listing_Standard listing, string label, ref bool active)	
        {	
            var rect = listing.GetRect(30f);	
            Heading(rect.LeftPartPixels(rect.width - 30f), label);	
            return Checkbox(rect.RightPartPixels(30f), "", ref active);	
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static void CenterText(Action action)
        {
            var anch = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            action();
            Text.Anchor = anch;
        }

        public static bool InputField(Rect rect, string name, ref string buff, Texture2D icon = null, int max = 999,
            bool readOnly = false, bool forceFocus = false, bool ShowName = false)
        {
            if (buff == null)
            {
                buff = "";
            }

            var rect2 = rect;

            if (icon != null)
            {
                var icoRect = rect;
                icoRect.width = icoRect.height;
                Widgets.DrawTextureFitted(icoRect, icon, 1f);
                rect2.width -= icoRect.width;
                rect2.x += icoRect.width;
            }

            if (ShowName)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect.LeftPartPixels(name.GetWidthCached()), name);
                Text.Anchor = TextAnchor.UpperLeft;

                rect2 = rect.RightPartPixels(rect.width - (name.GetWidthCached() + 3));
            }

            GUI.SetNextControlName(name);

            buff = GUI.TextField(rect2, buff, max, Text.CurTextAreaStyle);

            var InFocus = GUI.GetNameOfFocusedControl() == name;

            if (!InFocus && forceFocus)
            {
                GUI.FocusControl(name);
            }

            if (Input.GetMouseButtonDown(0) && !Mouse.IsOver(rect2) && InFocus)
            {
                GUI.FocusControl(null);
            }

            return InFocus;
        }

        public static void ResetFont()
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static void Heading(Listing_Standard listing, string label)
        {
            Heading(listing.GetRect(30), label);
        }

        public static void OptionalBox(Rect rect, string value, Action action, bool active)
        {
            if (Widgets.RadioButtonLabeled(rect, value, active))
            {
                action();
            }
        }

        public static void CollapsableHeading(Listing_Standard listing, string label, ref bool Collapsed)
        {
            var Rect = listing.GetRect(30);
            Heading(Rect, label);

            var loc = new Vector2(Rect.x + Rect.width - Rect.height, Rect.y);
            var len = new Vector2(Rect.height, Rect.height);
            var rect = new Rect(loc, len);

            if (Widgets.ButtonImage(rect, Collapsed ? DropDown : FoldUp))
                Collapsed = !Collapsed;
        }

        public static void Heading(Rect rect, string label)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, label);
            ResetFont();
        }
    }
}