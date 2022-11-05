using UnityEngine;
using Verse;
using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace aRandomKiwi.RimThemes
{
    class Settings : ModSettings
    {
        public Settings(){
            Utils.modSettings = this;
        }

        public static bool disabledOverrideThemeWindowFillColorAlpha = true;
        public static float overrideThemeWindowFillColorAlphaLevel = 0.75f;
        public static int rimthemesLogoMode = 3;
        public static int windowsShadowMode = 3;
        public static bool keepCurrentBg = false;
        public static bool disableRandomBg = true;
        public static bool disableVideoBg = true;
        public static bool disableCustomFontsInConsole = true;
        public static bool disableWallpaper = false;
        public static bool disableCustomFonts = false;
        public static bool disableCustomCursors = false;
        public static bool disableParticle = true;
        public static bool modManagerAsOptionList = false;
        public static bool disableCustomLoader = false;
        public static bool disableCustomSounds = false;
        public static bool disableCustomSongs = false;
        public static int dialogStacking = 1;
        public static int menuAlignment = 3;
        public static bool disableButtonBG = false;
        public static int windowAnimation = (int)WindowAnim.Theme;
        public static bool disableTapestry = false;
        public static bool hideLoadingText = false;
        public static bool verboseMode = false;
        public static string curRandomBg = null;
        public static string lastVersionInfo = "";
        public static bool disableFontFilterModePoint = true;
        public static bool disableTinyCustomFont = false;
        public static int expansionsIconsMode = 3;

        public static bool disableTinyCustomFontPrev = false;
        public static bool disableCustomSongsPrev = false;
        public static bool disableCustomSoundsPrev = false;
        public static bool disableCustomFontsPrev = false;
        public static bool disableCustomCursorsPrev = false;
        public static bool disableRandomBgPrev = true;
        public static bool resetLabelWidth = false;
        public static bool disableFontFilterModePointPrev = true;
        public static int ludeonLogoMode = 1;
        public static int cornerInfoMode = 1;


        public static Vector2 scrollPosition = Vector2.zero;

        public static string curTheme = "-HSK";

        public static void DoSettingsWindowContents(Rect inRect)
        {
            inRect.yMin += 15f;
            inRect.yMax -= 15f;

            var defaultColumnWidth = (inRect.width - 50);
            Listing_Standard list = new Listing_Standard() { ColumnWidth = defaultColumnWidth };

            //Image logo
            if( Widgets.ButtonImage(new Rect((inRect.width / 2) - 90, inRect.y, 180, 144), Loader.logoTex, Color.white, Color.green))
                Find.WindowStack.Add(new Dialog_ThemesList());

            var outRect = new Rect(inRect.x, inRect.y+150, inRect.width, inRect.height-150);
            var scrollRect = new Rect(0f, 150f, inRect.width - 16f, inRect.height * 3f + 50 );
            Widgets.BeginScrollView(outRect, ref scrollPosition, scrollRect, true);

            list.Begin(scrollRect);

            list.GapLine();
            list.Label("RimTheme_SettingsAlphaSection".Translate());
            list.GapLine();

            bool disabledOverrideThemeWindowFillColorAlphaPrev = disabledOverrideThemeWindowFillColorAlpha;
            list.CheckboxLabeled("RimTheme_SettingsDisableOverrideWindowFillColorAlphaLevel".Translate(), ref disabledOverrideThemeWindowFillColorAlpha);

            if (!disabledOverrideThemeWindowFillColorAlpha)
            {
                list.Label("RimTheme_SettingsOverrideWindowFillColorAlphaLevel".Translate((int)(overrideThemeWindowFillColorAlphaLevel * 100)));
                float overrideThemeWindowFillColorAlphaLevelPrev = overrideThemeWindowFillColorAlphaLevel;
                overrideThemeWindowFillColorAlphaLevel = list.Slider(overrideThemeWindowFillColorAlphaLevel, 0.0f, 1.0f);

                //Opacity level change => apply 
                if(overrideThemeWindowFillColorAlphaLevel != overrideThemeWindowFillColorAlphaLevelPrev || disabledOverrideThemeWindowFillColorAlpha != disabledOverrideThemeWindowFillColorAlphaPrev)
                {
                    Utils.applyWindowFillColorOpacityOverride(Settings.curTheme);
                }
            }

            list.GapLine();
            list.Label("RimTheme_SettingsGlobalSection".Translate());
            list.GapLine();

            list.CheckboxLabeled("RimTheme_SettingsDisableTinyCustomFont".Translate(), ref disableTinyCustomFont);
            list.CheckboxLabeled("RimTheme_SettingsDisableVideoBackground".Translate(), ref disableVideoBg);
            list.CheckboxLabeled("RimTheme_SettingsDisableWallpaper".Translate(), ref disableWallpaper);
            list.CheckboxLabeled("RimTheme_SettingsDisableFont".Translate(), ref disableCustomFonts);
            list.CheckboxLabeled("RimTheme_SettingsDisableCustomFontConsole".Translate(), ref disableCustomFontsInConsole);
            list.CheckboxLabeled("RimTheme_SettingsDisableCursor".Translate(), ref disableCustomCursors);
            list.CheckboxLabeled("RimTheme_SettingsDisableParticle".Translate(), ref disableParticle);
            list.CheckboxLabeled("RimTheme_SettingsShowThemeManagerAsList".Translate(), ref modManagerAsOptionList);
            list.CheckboxLabeled("RimTheme_SettingsDisableCustomSounds".Translate(), ref disableCustomSounds);
            list.CheckboxLabeled("RimTheme_SettingsDisableCustomSongs".Translate(), ref disableCustomSongs);
            list.CheckboxLabeled("RimTheme_SettingsDisableButtonBG".Translate(), ref disableButtonBG);
            list.CheckboxLabeled("RimTheme_SettingsDisableTapestry".Translate(), ref disableTapestry);
            list.CheckboxLabeled("RimTheme_SettingsHideLoadingText".Translate(), ref hideLoadingText);
            list.CheckboxLabeled("RimTheme_SettingsDisableCustomLoader".Translate(), ref disableCustomLoader);
            list.CheckboxLabeled("RimTheme_SettingsVerboseLogs".Translate(), ref verboseMode);
            list.CheckboxLabeled("RimTheme_SettingsDisableFontFilterModePoint".Translate(), ref disableFontFilterModePoint);

            list.GapLine();
            list.Label("RimTheme_SettingsRandomBackgroundSection".Translate());
            list.GapLine();

            list.CheckboxLabeled("RimTheme_SettingsDisableRandomBg".Translate(), ref disableRandomBg);
            list.CheckboxLabeled("RimTheme_SettingsKeepCurrentRandomBg".Translate(), ref keepCurrentBg);
            if (keepCurrentBg && disableRandomBg)
                keepCurrentBg = false;

            list.GapLine();
            list.Label("RimTheme_SettingsDialogStacking".Translate());
            list.GapLine();

            if (list.RadioButton("RimTheme_SettingsDeterminedByTheme".Translate(), (dialogStacking == 1)))
                dialogStacking = 1;
            if (list.RadioButton("RimTheme_SettingsDialogStackingEnabled".Translate(), (dialogStacking == 2)))
                dialogStacking = 2;
            if (list.RadioButton("RimTheme_SettingsDialogStackingDisabled".Translate(), (dialogStacking == 3)))
                dialogStacking = 3;

            list.GapLine();
            list.Label("RimTheme_SettingsMenuAlignment".Translate());
            list.GapLine();

            if (list.RadioButton("RimTheme_SettingsMenuAlignmentLeft".Translate(), (menuAlignment == 0)))
                menuAlignment = 0;
            if (list.RadioButton("RimTheme_SettingsMenuAlignmentMiddle".Translate(), (menuAlignment == 1)))
                menuAlignment = 1;
            if (list.RadioButton("RimTheme_SettingsMenuAlignmentRight".Translate(), (menuAlignment == 2)))
                menuAlignment = 2;
            if (list.RadioButton("RimTheme_SettingsDeterminedByTheme".Translate(), (menuAlignment == 3)))
                menuAlignment = 3;

            list.GapLine();
            list.Label("RimTheme_SettingsWindowAnimation".Translate());
            list.GapLine();

            if (list.RadioButton("RimTheme_SettingsWindowAnimationNone".Translate(), (windowAnimation == (int)WindowAnim.None)))
                windowAnimation = (int)WindowAnim.None;
            if (list.RadioButton("RimTheme_SettingsDeterminedByTheme".Translate(), (windowAnimation == (int)WindowAnim.Theme)))
                windowAnimation = (int)WindowAnim.Theme;
            if (list.RadioButton("RimTheme_SettingsWindowAnimationClip".Translate(), (windowAnimation == (int)WindowAnim.Clip)))
                windowAnimation = (int)WindowAnim.Clip;
            if (list.RadioButton("RimTheme_SettingsWindowAnimationSlideLeft".Translate(), (windowAnimation == (int)WindowAnim.SlideLeft)))
                windowAnimation = (int)WindowAnim.SlideLeft;
            if (list.RadioButton("RimTheme_SettingsWindowAnimationSlideRight".Translate(), (windowAnimation == (int)WindowAnim.SlideRight)))
                windowAnimation = (int)WindowAnim.SlideRight;
            if (list.RadioButton("RimTheme_SettingsWindowAnimationSlideTop".Translate(), (windowAnimation == (int)WindowAnim.SlideTop)))
                windowAnimation = (int)WindowAnim.SlideTop;
            if (list.RadioButton("RimTheme_SettingsWindowAnimationSlideBottom".Translate(), (windowAnimation == (int)WindowAnim.SlideBottom)))
                windowAnimation = (int)WindowAnim.SlideBottom;

            list.GapLine();
            list.Label("RimTheme_SettingsMenuRTLogo".Translate());
            list.GapLine();

            if (list.RadioButton("RimTheme_SettingsMenuRTLogoShow".Translate(), (rimthemesLogoMode == 1)))
                rimthemesLogoMode = 1;
            if (list.RadioButton("RimTheme_SettingsMenuRTLogoHide".Translate(), (rimthemesLogoMode == 2)))
                rimthemesLogoMode = 2;
            if (list.RadioButton("RimTheme_SettingsDeterminedByTheme".Translate(), (rimthemesLogoMode == 3)))
                rimthemesLogoMode = 3;

            if (disableCustomCursorsPrev != disableCustomCursors)
            {
                CustomCursor.Deactivate();
                CustomCursor.Activate();
                disableCustomCursorsPrev = disableCustomCursors;
            }

            list.GapLine();
            list.Label("RimTheme_SettingsWindowsShadow".Translate());
            list.GapLine();

            if (list.RadioButton("RimTheme_SettingsMenuRTLogoShow".Translate(), (windowsShadowMode == 1)))
                windowsShadowMode = 1;
            if (list.RadioButton("RimTheme_SettingsMenuRTLogoHide".Translate(), (windowsShadowMode == 2)))
                windowsShadowMode = 2;
            if (list.RadioButton("RimTheme_SettingsDeterminedByTheme".Translate(), (windowsShadowMode == 3)))
                windowsShadowMode = 3;

            list.GapLine();
            list.Label("RimTheme_SettingsExpansionsIcons".Translate());
            list.GapLine();

            if (list.RadioButton("RimTheme_SettingsMenuRTLogoShow".Translate(), (expansionsIconsMode == 1)))
                expansionsIconsMode = 1;
            if (list.RadioButton("RimTheme_SettingsMenuRTLogoHide".Translate(), (expansionsIconsMode == 2)))
                expansionsIconsMode = 2;
            if (list.RadioButton("RimTheme_SettingsDeterminedByTheme".Translate(), (expansionsIconsMode == 3)))
                expansionsIconsMode = 3;

            list.GapLine();
            list.Label("RimTheme_LudeonLogo".Translate());
            list.GapLine();

            if (list.RadioButton("RimTheme_SettingsMenuRTLogoShow".Translate(), (ludeonLogoMode == 1)))
                ludeonLogoMode = 1;
            if (list.RadioButton("RimTheme_SettingsMenuRTLogoHide".Translate(), (ludeonLogoMode == 2)))
                ludeonLogoMode = 2;

            list.GapLine();
            list.Label("RimTheme_InfoCorner".Translate());
            list.GapLine();

            if (list.RadioButton("RimTheme_SettingsMenuRTLogoShow".Translate(), (cornerInfoMode == 1)))
                cornerInfoMode = 1;
            if (list.RadioButton("RimTheme_SettingsMenuRTLogoHide".Translate(), (cornerInfoMode == 2)))
                cornerInfoMode = 2;


            //Change in disable custom font
            if (disableCustomFontsPrev != disableCustomFonts)
            {
                Utils.resetCachedLabelWidthCache();
                disableCustomFontsPrev = disableCustomFonts;
            }

            //Change in the disable custom sounds
            if (disableCustomSoundsPrev != disableCustomSounds)
            {
                Themes.changeSoundTheme();
                disableCustomSoundsPrev = disableCustomSounds;
            }

            //Change in the disable custom songs
            if (disableCustomSongsPrev != disableCustomSongs)
            {
                Themes.changeSongTheme();
                disableCustomSongsPrev = disableCustomSongs;
            }

            //Change in disable random bg
            if (disableRandomBgPrev != disableRandomBg)
            {
                //Current video bg stop if applicable
                Themes.stopCurrentAnimatedBackground();
                Themes.setNewRandomBg();
                disableRandomBgPrev = disableRandomBg;
            }

            if(disableFontFilterModePointPrev != disableFontFilterModePoint)
            {
                try
                {
                    //Impact of the change on loaded fonts
                    foreach (var entry in Themes.DBGUIStyle)
                    {
                        try
                        {
                            foreach (var entry2 in entry.Value)
                            {
                                if (disableFontFilterModePoint)
                                {
                                    entry2.Value.font.material.mainTexture.filterMode = FilterMode.Trilinear;
                                }
                                else
                                {
                                    entry2.Value.font.material.mainTexture.filterMode = FilterMode.Point;
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            Themes.LogError("Error while trying to change setting filterMode : " + e.Message);
                        }
                    }
                }
                catch(Exception e)
                {
                    Themes.LogError("Fatal error while trying to change setting filterMode : " + e.Message);
                }
                disableFontFilterModePointPrev = disableFontFilterModePoint;
            }

            if (disableTinyCustomFontPrev != disableTinyCustomFont)
            {
                try
                {
                    //Impact of the change on loaded fonts
                    foreach (var entry in Themes.DBGUIStyle)
                    {
                        try
                        {
                            if (disableTinyCustomFont)
                            {
                                Themes.DBGUIStyle[entry.Key][GameFont.Tiny].font = Text.fontStyles[0].font;
                                Themes.DBGUIStyle[entry.Key][GameFont.Tiny].fontSize = Text.fontStyles[0].fontSize;
                            }
                            else
                            {
                                //Si contient une police d'écriture petite on la définie
                                if (Themes.DBTinyFontByTheme.ContainsKey(entry.Key))
                                {
                                    Themes.DBGUIStyle[entry.Key][GameFont.Tiny].font = Themes.DBTinyFontByTheme[entry.Key];
                                    Themes.DBGUIStyle[entry.Key][GameFont.Tiny].fontSize = Themes.DBTinyFontSizeByTheme[entry.Key];

                                    //On applique la politique actuelle de filtrage des points
                                    if (disableFontFilterModePoint)
                                        Themes.DBGUIStyle[entry.Key][GameFont.Tiny].font.material.mainTexture.filterMode = FilterMode.Trilinear;
                                    else
                                        Themes.DBGUIStyle[entry.Key][GameFont.Tiny].font.material.mainTexture.filterMode = FilterMode.Point;
                                }
                                else
                                {
                                    Themes.DBGUIStyle[entry.Key][GameFont.Tiny].font = Text.fontStyles[0].font;
                                    Themes.DBGUIStyle[entry.Key][GameFont.Tiny].fontSize = Text.fontStyles[0].fontSize;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Themes.LogError("Error while trying to toggle tinyFont : " + e.Message);
                        }
                    }
                }
                catch (Exception e)
                {
                    Themes.LogError("Fatal error while trying to change setting tinyFont toggling : " + e.Message);
                }
                Utils.resetCachedLabelWidthCache();
                disableTinyCustomFontPrev = disableTinyCustomFont;
            }

            list.End();
            Widgets.EndScrollView();
            //settings.Write();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref disabledOverrideThemeWindowFillColorAlpha, "disabledOverrideThemeWindowFillColorAlpha", true);
            Scribe_Values.Look<float>(ref overrideThemeWindowFillColorAlphaLevel, "overrideThemeWindowFillColorAlpha", 0.75f);


            Scribe_Values.Look<int>(ref cornerInfoMode, "cornerInfoMode", 1);
            Scribe_Values.Look<int>(ref ludeonLogoMode, "ludeonLogoMode", 1);
            Scribe_Values.Look<int>(ref rimthemesLogoMode, "rimthemesLogoMode", 3);
            Scribe_Values.Look<int>(ref windowsShadowMode, "windowsShadowMode", 3);
            Scribe_Values.Look<int>(ref expansionsIconsMode, "expansionsIconsMode", 3);
            Scribe_Values.Look<string>(ref curTheme, "curTheme", "-1§Vanilla");
            Scribe_Values.Look<bool>(ref disableWallpaper, "disableWallpaper", false);
            Scribe_Values.Look<bool>(ref disableCustomFonts, "disableCustomFonts", false);
            Scribe_Values.Look<bool>(ref disableCustomFontsInConsole, "disableCustomFontsInConsole", true);
            Scribe_Values.Look<bool>(ref disableCustomCursors, "disableCustomCursors", false);
            Scribe_Values.Look<bool>(ref disableParticle, "disableParticle", true);
            Scribe_Values.Look<bool>(ref modManagerAsOptionList, "modManagerAsOptionList", false);
            Scribe_Values.Look<bool>(ref disableCustomLoader, "disableCustomLoader", false);
            Scribe_Values.Look<bool>(ref disableCustomSounds, "disableCustomSounds", false);
            Scribe_Values.Look<bool>(ref disableCustomSongs, "disableCustomSongs", false);
            Scribe_Values.Look<int>(ref dialogStacking, "dialogStacking", 1);
            Scribe_Values.Look<bool>(ref disableButtonBG, "disableButtonBG", false);
            Scribe_Values.Look<bool>(ref disableTapestry, "disableTapestry2", false);            
            Scribe_Values.Look<int>(ref menuAlignment, "menuAlignment", 3);
            Scribe_Values.Look<int>(ref windowAnimation, "windowAnimation", (int)WindowAnim.Theme);
            Scribe_Values.Look<bool>(ref hideLoadingText, "hideLoadingText", false);
            Scribe_Values.Look<bool>(ref verboseMode, "verboseMode", false);
            Scribe_Values.Look<bool>(ref disableVideoBg, "disableVideoBg", true);
            Scribe_Values.Look<bool>(ref disableRandomBg, "disableRandomBg", true);
            Scribe_Values.Look<string>(ref curRandomBg, "curRandomBg",null);
            Scribe_Values.Look<bool>(ref keepCurrentBg, "keepCurrentBg", false);
            Scribe_Values.Look<bool>(ref disableFontFilterModePoint, "disableFontFilterModePoint", true);
            Scribe_Values.Look<string>(ref lastVersionInfo, "lastVersionInfo", "");
            Scribe_Values.Look<bool>(ref disableTinyCustomFont, "disableTinyCustomFont", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                //Used to track changes in parameters for certain variables
                disableTinyCustomFontPrev = disableTinyCustomFont;
                disableCustomFontsPrev = disableCustomFonts;
                disableCustomCursorsPrev = disableCustomCursors;
                disableCustomSoundsPrev = disableCustomSounds;
                disableCustomSongsPrev = disableCustomSongs;
                disableRandomBgPrev = disableRandomBg;
                disableFontFilterModePointPrev = disableFontFilterModePoint;
    }
        }
    }
}