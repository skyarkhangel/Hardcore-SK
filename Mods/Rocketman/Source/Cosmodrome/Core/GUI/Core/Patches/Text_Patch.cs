using System;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public static class Text_Patch
    {
        [RocketPatch(typeof(Text), nameof(Text.Anchor), methodType: MethodType.Getter)]
        public static class Text_Anchor_Getter_Patch
        {
            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(ref TextAnchor __result)
            {
                if (GUIFont.UseCustomFonts)
                {
                    __result = GUIFont.Anchor;
                    return false;
                }
                return true;
            }
        }

        [RocketPatch(typeof(Text), nameof(Text.Anchor), methodType: MethodType.Setter)]
        public static class Text_Anchor_Setter_Patch
        {
            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(TextAnchor value)
            {
                if (GUIFont.UseCustomFonts)
                {
                    GUIFont.Anchor = value;
                    return false;
                }
                return true;
            }
        }

        [RocketPatch(typeof(Text), nameof(Text.WordWrap), methodType: MethodType.Getter)]
        public static class Text_WordWrap_Getter_Patch
        {
            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(ref bool __result)
            {
                if (GUIFont.UseCustomFonts)
                {
                    __result = GUIFont.WordWrap;
                    return false;
                }
                return true;
            }
        }

        [RocketPatch(typeof(Text), nameof(Text.WordWrap), methodType: MethodType.Setter)]
        public static class Text_WordWrap_Setter_Patch
        {
            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(bool value)
            {
                if (GUIFont.UseCustomFonts)
                {
                    GUIFont.WordWrap = value;
                    return false;
                }
                return true;
            }
        }

        [RocketPatch(typeof(Text), nameof(Text.Font), methodType: MethodType.Getter)]
        public static class Text_Font_Getter_Patch
        {
            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(ref GameFont __result)
            {
                if (GUIFont.UseCustomFonts)
                {
                    switch (GUIFont.Font)
                    {
                        case GUIFontSize.Tiny:
                        case GUIFontSize.Smaller:
                            __result = GameFont.Tiny;
                            break;
                        case GUIFontSize.Small:
                            __result = GameFont.Small;
                            break;
                        case GUIFontSize.Medium:
                            __result = GameFont.Medium;
                            break;
                    }
                    return false;
                }
                return true;
            }
        }

        [RocketPatch(typeof(Text), nameof(Text.Font), methodType: MethodType.Setter)]
        public static class Text_Font_Setter_Patch
        {
            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(ref GameFont value)
            {
                if (GUIFont.UseCustomFonts)
                {
                    switch (value)
                    {
                        case GameFont.Tiny:
                            GUIFont.Font = GUIFontSize.Tiny;
                            break;
                        case GameFont.Small:
                            GUIFont.Font = GUIFontSize.Small;
                            break;
                        case GameFont.Medium:
                            GUIFont.Font = GUIFontSize.Medium;
                            break;
                    }
                    return false;
                }
                return true;
            }
        }

        [RocketPatch(typeof(Text), nameof(Text.CurFontStyle), methodType: MethodType.Getter)]
        public static class Text_CurFontStyle_Patch
        {
            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(ref GUIStyle __result)
            {
                if (GUIFont.UseCustomFonts)
                {
                    __result = GUIFont.CurFontStyle;

                    return false;
                }
                return true;
            }
        }

        [RocketPatch(typeof(Text), nameof(Text.CurTextAreaReadOnlyStyle), methodType: MethodType.Getter)]
        public static class Text_CurTextAreaReadOnlyStyle_Patch
        {
            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(ref GUIStyle __result)
            {
                if (GUIFont.UseCustomFonts)
                {
                    __result = GUIFont.CurTextAreaReadOnlyStyle;

                    return false;
                }
                return true;
            }
        }

        [RocketPatch(typeof(Text), nameof(Text.CurTextAreaStyle), methodType: MethodType.Getter)]
        public static class Text_CurTextAreaStyle_Patch
        {
            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(ref GUIStyle __result)
            {
                if (GUIFont.UseCustomFonts)
                {
                    __result = GUIFont.CurTextAreaReadOnlyStyle;

                    return false;
                }
                return true;
            }
        }

        [RocketPatch(typeof(Text), nameof(Text.CurTextFieldStyle), methodType: MethodType.Getter)]
        public static class Text_CurTextFieldStyle_Patch
        {
            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(ref GUIStyle __result)
            {
                if (GUIFont.UseCustomFonts)
                {
                    __result = GUIFont.CurTextFieldStyle;

                    return false;
                }
                return true;
            }
        }
    }
}
