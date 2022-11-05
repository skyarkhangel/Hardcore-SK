using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;
using HarmonyLib;

namespace aRandomKiwi.RimThemes
{
    [HarmonyPatch(typeof(Widgets), "ButtonTextWorker", new Type[] { typeof(Rect), typeof(string), typeof(bool), typeof(bool), typeof(Color), typeof(bool), typeof(bool), typeof(TextAnchor) }), StaticConstructorOnStartup]
    class ButtonTextWorker_Patch
    {
        private static Color prevGUIColor;

        [HarmonyPrefix]
        static bool Prefix(ref Widgets.DraggableResult __result, Rect rect, string label, bool drawBackground, bool doMouseoverSound, Color textColor, bool active, bool draggable, TextAnchor? overrideTextAnchor = null)
        {
            if (!(Themes.initialized && Themes.vanillaThemeSaved))
                return true;
            try
            {
                if (Themes.getButtonNoBG())
                    drawBackground = false;

                TextAnchor anchor = Text.Anchor;
                Color color = GUI.color;
                prevGUIColor = color;
                GUI.color = ColorsSubstitution.getTextureSubstitutionColor(color);

                if (drawBackground)
                {
                    string buttonNoTex = Themes.getText("buttonnotex");
                    if (buttonNoTex == null || buttonNoTex != "true")
                    {
                        Texture2D atlas;
                        //if (color != Color.white)
                            // atlas = Themes.DBTex[Themes.VanillaThemeID]["Widgets"]["ButtonBGAtlas"];
                        //else
                            atlas = Themes.getThemeTex("Widgets", "ButtonBGAtlas");
                        if (Mouse.IsOver(rect))
                        {
                            //if (color != Color.white)
                                //atlas = Themes.DBTex[Themes.VanillaThemeID]["Widgets"]["ButtonBGAtlasMouseover"];
                            //else
                                atlas = Themes.getThemeTex("Widgets", "ButtonBGAtlasMouseover");
                            if (Input.GetMouseButton(0))
                            {
                                //if (color != Color.white)
                                //    atlas = Themes.DBTex[Themes.VanillaThemeID]["Widgets"]["ButtonBGAtlasClick"];
                                //else
                                    atlas = Themes.getThemeTex("Widgets", "ButtonBGAtlasClick");
                            }
                        }
                        Widgets.DrawAtlas(rect, atlas);
                    }
                    else
                    {
                        string value = Themes.getText("buttonborderwidth");
                        int bbw = 1;
                        if (value != null)
                        {
                            int.TryParse(value, out bbw);
                        }

                        //If applicable, drawing fill colors
                        value = Themes.getText("buttonusecolor");
                        if (value == "true")
                        {
                            Color curColor;
                            curColor = Themes.getColorMisc("buttonfillcolor");

                            if (Mouse.IsOver(rect))
                            {
                                curColor = Themes.getColorMisc("buttonhovercolor");
                                if (Input.GetMouseButton(0))
                                {
                                    curColor = Themes.getColorMisc("buttonclickcolor");
                                }
                            }

                            Widgets.DrawRectFast(rect, curColor);
                        }


                        //Border drawing
                        Widgets.DrawBox(rect, bbw);
                    }
                    //If necessary, we draw the Particle
                    Themes.drawParticle(rect);

                }
                if (doMouseoverSound)
                {
                    MouseoverSounds.DoRegion(rect);
                }
                if (!drawBackground)
                {
                    GUI.color = textColor;
                    if (Mouse.IsOver(rect))
                    {
                        GUI.color = Widgets.MouseoverOptionColor;
                    }
                }

                if (overrideTextAnchor != null)
                {
                    Text.Anchor = overrideTextAnchor.Value;
                }
                else if (drawBackground)
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                }
                else
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                }
                bool wordWrap = Text.WordWrap;
                if (rect.height < Text.LineHeight * 2f)
                {
                    Text.WordWrap = false;
                }
                Widgets.Label(rect, label);
                Text.Anchor = anchor;
                GUI.color = color;
                Text.WordWrap = wordWrap;
                if (active && draggable)
                {
                    __result = Widgets.ButtonInvisibleDraggable(rect, false);
                    return false;
                }
                if (active)
                {
                    __result = (!Widgets.ButtonInvisible(rect, false)) ? Widgets.DraggableResult.Idle : Widgets.DraggableResult.Pressed;
                    return false;
                }
                __result = Widgets.DraggableResult.Idle;

                return false;
            }
            catch (Exception e)
            {
                Themes.LogError("Patch failed : ButtonTextWorker : " + e.Message);
                return true;
            }
        }

        [HarmonyPostfix]
        static void Postfix(ref Widgets.DraggableResult __result, Rect rect, string label, bool drawBackground, bool doMouseoverSound, Color textColor, bool active, bool draggable)
        {
            GUI.color = prevGUIColor;
        }
    }



    [HarmonyPatch(typeof(Widgets), "ButtonTextSubtle"), StaticConstructorOnStartup]
    class ButtonTextSubtle_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(ref bool __result, Rect rect, string label, float barPercent = 0f, float textLeftMargin = -1f, SoundDef mouseoverSound = null, Vector2 functionalSizeOffset = default(Vector2), Color? labelColor = null, bool highlight = false)
        {
            if (!(Themes.initialized && Themes.vanillaThemeSaved) || textLeftMargin == 8f || Settings.disableParticle)
                return true;

            try
            {
                Rect rect2 = rect;
                rect2.width += functionalSizeOffset.x;
                rect2.height += functionalSizeOffset.y;
                bool flag = false;
                if (Mouse.IsOver(rect2))
                {
                    flag = true;
                    GUI.color = GenUI.MouseoverColor;
                }
                if (mouseoverSound != null)
                {
                    MouseoverSounds.DoRegion(rect2, mouseoverSound);
                }
                Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
                if (highlight)
                {
                    GUI.color = Color.grey;
                    Widgets.DrawBox(rect, 2, null);
                }
                GUI.color = Color.white;
                if (barPercent > 0.001f)
                {
                    Rect rect3 = rect.ContractedBy(1f);
                    Widgets.FillableBar(rect3, barPercent, Themes.getThemeTex("Widgets", "ButtonBarTex"), null, false);
                }
                Rect rect4 = new Rect(rect);
                if (textLeftMargin < 0f)
                {
                    textLeftMargin = rect.width * 0.15f;
                }
                rect4.x += textLeftMargin;
                if (flag)
                {
                    rect4.x += 2f;
                    rect4.y -= 2f;
                }
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.WordWrap = false;
                Text.Font = GameFont.Small;
                GUI.color = (labelColor ?? Color.white);
                //If there is a Particle to draw we center the text in the middle otherwise basic behavior
                Texture2D tex = Themes.getThemeParticle();
                if (Settings.disableParticle || tex == null || rect.width < 80f)
                    Widgets.Label(rect4, label);
                else
                    Widgets.Label(rect4, "  "+label);
                Text.Anchor = TextAnchor.UpperLeft;
                Text.WordWrap = true;

                Themes.drawParticle(rect);
                __result = Widgets.ButtonInvisible(rect2, false);
                return false;
            }
            catch(Exception e)
            {
                Themes.LogError("Widgets.ButtonTextSubtle patch failed : "+e.Message);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Widgets), "DrawWindowBackground", new Type[] { typeof(Rect) }), StaticConstructorOnStartup]
    class DrawWindowBackground_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(Rect rect)
        {
            if (!(Themes.initialized && Themes.vanillaThemeSaved))
                return true;
            else
                return DrawWindowBackgroundPatch(rect, default(Color));
        }

        static public bool DrawWindowBackgroundPatch(Rect rect, Color colorFactor)
        {
            try
            {
                //If existing upholstery
                if (!Settings.disableTapestry && Themes.DBTexTapestry.ContainsKey(Settings.curTheme) && Themes.DBTexTapestry[Settings.curTheme] != null)
                {
                    bool colorFactorRequired = colorFactor != default(Color);
                    Color prevColor = GUI.color;
                    ScaleMode csm = ScaleMode.StretchToFill;
                    string currentScaleMode = Themes.getText("tapestryscalemode");

                    if (currentScaleMode == "scaletofit")
                        csm = ScaleMode.ScaleToFit;
                    else if (currentScaleMode == "scaleandcrop")
                        csm = ScaleMode.ScaleAndCrop;

                    Color cc = Color.white;
                    if (!Settings.disabledOverrideThemeWindowFillColorAlpha)
                        cc.a = Settings.overrideThemeWindowFillColorAlphaLevel;
                    if (colorFactorRequired)
                        GUI.color = cc * colorFactor;
                    else
                        GUI.color = cc;

                    //Widgets.ButtonImage(rect, Themes.DBTexTapestry[Settings.curTheme], Color.white, Color.white);
                    GUI.DrawTexture(rect, Themes.DBTexTapestry[Settings.curTheme], csm);
                    if (colorFactorRequired)
                        GUI.color = Themes.getColorMisc("tapestrybordercolor") * colorFactor;
                    else
                        GUI.color = Themes.getColorMisc("tapestrybordercolor");
                    Widgets.DrawBox(rect, 1, null);
                    if (colorFactorRequired)
                        GUI.color = prevColor;
                    else
                        GUI.color = Color.white;

                    return false;
                }
                else
                    return true;
            }
            catch (Exception e)
            {
                Themes.LogError("Widgets.DrawWindowBackground patch failed : " + e.Message);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Widgets), "DrawWindowBackground", new Type[] { typeof(Rect), typeof(Color) }), StaticConstructorOnStartup]
    class DrawWindowBackground2_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(Rect rect, Color colorFactor)
        {
            if (!(Themes.initialized && Themes.vanillaThemeSaved))
                return true;
            else
                return DrawWindowBackground_Patch.DrawWindowBackgroundPatch(rect, colorFactor);
        }
    }


    [HarmonyPatch(typeof(Widgets), "ButtonImage", new Type[] { typeof(Rect), typeof(Texture2D), typeof(Color), typeof(Color), typeof(bool) }), StaticConstructorOnStartup]
    class Widgets_ButtonImage_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(Rect butRect, Texture2D tex, Color baseColor, Color mouseoverColor, bool doMouseoverSound, ref bool __result)
        {
            if (!(Themes.initialized && Themes.vanillaThemeSaved))
                return true;

            try
            {
                Color prev = GUI.color;
                if (Mouse.IsOver(butRect))
                {
                    GUI.color = ColorsSubstitution.getTextureSubstitutionColor(mouseoverColor);
                    //GUI.color = mouseoverColor;
                }
                else
                {
                    GUI.color = baseColor;
                }
                GUI.DrawTexture(butRect, tex);
                GUI.color = baseColor;
                __result = Widgets.ButtonInvisible(butRect, false);
                GUI.color = prev;
                return false;
            }
            catch (Exception e)
            {
                Themes.LogError("Widgets.ButtonImage patch failed : " + e.Message);
                return true;
            }
        }
    }





    [HarmonyPatch(typeof(Widgets), "Label", new Type[] { typeof(Rect), typeof(GUIContent) }), StaticConstructorOnStartup]
    class Widgets_Label1_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(Rect rect, GUIContent content)
        {
            if (!(Themes.initialized && Themes.vanillaThemeSaved) || !LoaderGM.reachedMainMenu)
                return true;

            try
            {
                if (!Utils.tempDisableNoTransparentText && Themes.getVal("disabletransparenttext") == 1)
                {
                    Color x = GUI.color;
                    x.a = 1.0f;
                    GUI.color = x;
                }

                Color prev = GUI.color;
                GUI.color = ColorsSubstitution.getTextSubstitutionColor(prev);
                GUI.Label(rect, content, Text.CurFontStyle);
                GUI.color = prev;
                return false;
            }
            catch (Exception e)
            {
                Themes.LogError("Widgets.Label1 patch failed : " + e.Message);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Widgets), "Label", new Type[] { typeof(Rect), typeof(string) }), StaticConstructorOnStartup]
    class Widgets_Label2_Patch
    {
        static Color prevColor;

        [HarmonyPrefix]
        static bool Prefix(Rect rect, string label)
        {
            if (!(Themes.initialized && Themes.vanillaThemeSaved) || !LoaderGM.reachedMainMenu)
                return true;

            try
            {
                if (!Utils.tempDisableNoTransparentText && Themes.getVal("disabletransparenttext") == 1)
                {
                    Color x = GUI.color;
                    x.a = 1.0f;
                    GUI.color = x;
                }

                Color prev = GUI.color;
                prevColor = prev;
                GUI.color = ColorsSubstitution.getTextSubstitutionColor(prev);

                return true;
                /*Rect position = rect;
                float num = Prefs.UIScale / 2f;
                if (Prefs.UIScale > 1f && Math.Abs(num - Mathf.Floor(num)) > 1E-45f)
                {
                    position.xMin = Widgets.AdjustCoordToUIScalingFloor(rect.xMin);
                    position.yMin = Widgets.AdjustCoordToUIScalingFloor(rect.yMin);
                    position.xMax = Widgets.AdjustCoordToUIScalingCeil(rect.xMax + 1E-05f);
                    position.yMax = Widgets.AdjustCoordToUIScalingCeil(rect.yMax + 1E-05f);
                }
                //GUI.Label(rect, label, Text.CurFontStyle);
                GUI.Label(position, label, Text.CurFontStyle);
                GUI.color = prev;
                return false;*/
            }
            catch (Exception e)
            {
                Themes.LogError("Widgets.Label2 patch failed : " + e.Message);
                return true;
            }
        }

        [HarmonyPostfix]
        static void Postfix(Rect rect, string label)
        {
            if (!(Themes.initialized && Themes.vanillaThemeSaved) || !LoaderGM.reachedMainMenu)
                return;

            GUI.color = prevColor;
        }
    }


    [HarmonyPatch(typeof(Widgets), "TextField", new Type[] { typeof(Rect), typeof(string) }), StaticConstructorOnStartup]
    class Widgets_TextField_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(Rect rect, string text, ref string __result)
        {
            if (!(Themes.initialized && Themes.vanillaThemeSaved))
                return true;

            try
            {
                Color prev = GUI.color;
                GUI.color = ColorsSubstitution.getTextSubstitutionColor(prev);

                if (text == null)
                {
                    text = string.Empty;
                }
                __result = GUI.TextField(rect, text, Text.CurTextFieldStyle);

                GUI.color = prev;
                return false;
            }
            catch (Exception e)
            {
                Themes.LogError("Widgets.TextField patch failed : " + e.Message);
                return true;
            }
        }
    }


    [HarmonyPatch(typeof(Widgets), "TextArea", new Type[] { typeof(Rect), typeof(string),typeof(bool) }), StaticConstructorOnStartup]
    class Widgets_TextArea_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(ref string __result,Rect rect, string text, bool readOnly = false)
        {
            if (!(Themes.initialized && Themes.vanillaThemeSaved))
                return true;

            try
            {
                Color prev = GUI.color;
                GUI.color = ColorsSubstitution.getTextSubstitutionColor(prev); 

                if (text == null)
                {
                    text = string.Empty;
                }
                __result = GUI.TextArea(rect, text, (!readOnly) ? Text.CurTextAreaStyle : Text.CurTextAreaReadOnlyStyle);

                GUI.color = prev;
                return false;
            }
            catch (Exception e)
            {
                Themes.LogError("Widgets.TextField patch failed : " + e.Message);
                return true;
            }
        }
    }
}
