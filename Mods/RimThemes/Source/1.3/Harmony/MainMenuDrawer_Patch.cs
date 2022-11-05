using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using HarmonyLib;
using Verse;
using Verse.Sound;
using System.Reflection;

namespace aRandomKiwi.RimThemes
{
    [HarmonyPatch(typeof(MainMenuDrawer), "MainMenuOnGUI"), StaticConstructorOnStartup]
    class MainMenuOnGUI_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(ref Vector2 ___PaneSize, ref Vector2 ___TitleSize, ref bool ___anyMapFiles, Vector2 ___LudeonLogoSize, Texture2D ___TexTitle, Texture2D ___TexLudeonLogo)
        {
            try
            {
                LoaderGM.reachedMainMenu = true;
                float start = 0;
                float start2 = 0;
                float startMPC = 0;
                float sub1 = 0;
                TextAnchor mpcAnchor;
                string menuAlign = Themes.getMenuAlignment();
                if (menuAlign == "left")
                {
                    startMPC = 50;
                    start = 50;
                    mpcAnchor = TextAnchor.UpperLeft;
                    sub1 = 170;
                }
                else if (menuAlign == "middle")
                {
                    start = (UI.screenWidth / 2) - ___PaneSize.x / 2f;
                    mpcAnchor = TextAnchor.UpperCenter;
                    sub1 = 70;
                }
                else
                {
                    start = (float)(UI.screenWidth / 2) - ___PaneSize.x / 2f;
                    mpcAnchor = TextAnchor.UpperRight;
                    sub1 = 85f;
                }

                if (Settings.cornerInfoMode == 1)
                {
                    Utils.tempDisableDynColor = true;
                    Utils.tempDisableNoTransparentText = true;
                    VersionControl.DrawInfoInCorner();

                    //Sponsor Rimtheme info
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    string spText = "RimTheme_SponsorInfo".Translate(Utils.sponsor);
                    Rect spRect = new Rect(10f, 73f, 330f, Text.CalcHeight(spText, 330f));
                    Widgets.Label(spRect, spText);
                    GUI.color = Color.white;
                    Utils.tempDisableDynColor = false;
                    Utils.tempDisableNoTransparentText = false;
                }

                Rect rect = new Rect(start, (float)(UI.screenHeight / 2) - ___PaneSize.y / 2f + 50f, ___PaneSize.x, ___PaneSize.y);
                if (menuAlign == "left")
                    rect.x = 50;
                else if (menuAlign == "middle")
                    rect.x = (float)(UI.screenWidth / 2) - (rect.width / 2) + 40;
                else
                    rect.x = (float)UI.screenWidth - rect.width - 30f;

                Rect rect2 = new Rect(startMPC, rect.y - 30f, (float)UI.screenWidth - sub1, 30f);
                Text.Font = GameFont.Medium;
                Text.Anchor = mpcAnchor;
                string text = "MainPageCredit".Translate();
                if (UI.screenWidth < 990)
                {
                    Rect position = rect2;
                    position.xMin = position.xMax - Text.CalcSize(text).x;
                    position.xMin -= 4f;
                    position.xMax += 4f;
                    GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                    GUI.DrawTexture(position, BaseContent.WhiteTex);
                    GUI.color = Color.white;
                }
                Widgets.Label(rect2, text);
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
                Vector2 a = ___TitleSize;
                if (a.x > (float)UI.screenWidth)
                {
                    a *= (float)UI.screenWidth / a.x;
                }
                a *= 0.7f;

                if (menuAlign == "left")
                    start2 = 50;
                else if (menuAlign == "middle")
                    start2 = (UI.screenWidth / 2) - (a.x / 2) + 10f;
                else
                    start2 = (float)UI.screenWidth - a.x - 50f;

                Rect position2 = new Rect(start2, rect2.y - a.y, a.x, a.y);
                GUI.DrawTexture(position2, ___TexTitle, ScaleMode.StretchToFill, true);
                if (Settings.ludeonLogoMode == 1)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    Rect position3 = new Rect((float)(UI.screenWidth - 8) - ___LudeonLogoSize.x, 8f, ___LudeonLogoSize.x, ___LudeonLogoSize.y);
                    GUI.DrawTexture(position3, ___TexLudeonLogo, ScaleMode.StretchToFill, true);
                    GUI.color = Color.white;
                }
                rect.yMin += 17f;

                int menuSpecialMode = Themes.getVal("menuspecialmode");
                switch(menuSpecialMode)
                {
                    case 1:  
                        Text.Font = GameFont.Medium;
                        Utils.textFontSetterLock = true;
                        Utils.tempDisableButtonsBackground = true;
                        Utils.squeezedDrawOptionListingIndex = 2;
                        Utils.squeezedDrawOptionListingIndexReturnVal = -13f;
                        break;
                }

                MainMenuDrawer.DoMainMenuControls(rect, ___anyMapFiles);
                Utils.tempDisableButtonsBackground = false;
                Utils.textFontSetterLock = false;
                Utils.squeezedDrawOptionListingIndex = 0;
                Text.Font = GameFont.Tiny;
                if (Debug.isDebugBuild)
                {
                    Rect outRect = new Rect(rect.x - 310f, rect.y, 295f, 400f);
                    //SUbstiution fonction DoDevBuildWarningRect(outRect)
                    //MainMenuDrawer.DoDevBuildWarningRect(outRect);
                    Widgets.DrawWindowBackground(outRect);
                    Rect tmpRect = outRect.ContractedBy(17f);
                    Widgets.Label(tmpRect, "DevBuildWarning".Translate());
                }

                Rect outRect2;
                if (menuAlign == "left")
                    outRect2 = new Rect(UI.screenWidth - 240f, (float)(UI.screenHeight - 8 - 400) - 100, 240f, 400f);
                else
                    outRect2 = new Rect(8f, 100f, 300f, 400f);

                MainMenuDrawer.DoTranslationInfoRect(outRect2);
                if(Themes.expansionsIcons())
                    MainMenuDrawer.DoExpansionIcons();

                return false;
            }
            catch(Exception e)
            {
                Themes.LogMsg("MainMenuDrawer patch failed : "+e.Message);
                return true;
            }
        }
    }


    [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls"), StaticConstructorOnStartup]
    class DoMainMenuControls_Patch
    {
        [HarmonyPostfix]
        static void Postfix(Rect rect, bool anyMapFiles)
        {
            if (!(Themes.initialized))
                return;

            try
            {
                //Activation for the first time of the customized cursor when loading
                if (!Themes.cursorFirstSet)
                {
                    Themes.cursorFirstSet = true;
                    CustomCursor.Deactivate();
                    CustomCursor.Activate();

                    //Loading the current theme
                    Themes.changeThemeNow(Settings.curTheme);

                    //Display update information if applicable
                    if (Settings.lastVersionInfo != Utils.releaseInfo)
                    {
                        try
                        {
                            Find.WindowStack.Add(new Dialog_Update());
                            Settings.lastVersionInfo = Utils.releaseInfo;
                            Utils.currentModInst.WriteSettings();
                        }
                        catch(Exception e)
                        {
                            Themes.LogError("Patch MainMenuDrawer.DoWindowContents updateDialog error : " + e.Message);
                        }
                    }
                }

                if (Current.ProgramState == ProgramState.Playing)
                {
                    //Clear selection to avoid passage of the displayed dialog (save, load, ..) under mainTabWindow which have priority
                    if (!Themes.dialogStacking())
                        Find.Selector.ClearSelection();

                    GUI.BeginGroup(rect);
                    if (Widgets.ButtonImage(new Rect(rect.width - 100, 5f, 96f, 96f), Themes.getThemeIcon(), Color.white, Color.green))
                        Find.WindowStack.Add(new Dialog_ThemesList());
                    GUI.EndGroup();
                }
                else
                {
                    Rect rect1 = new Rect(0f, 0f, UI.screenWidth, UI.screenHeight);
                    GUI.BeginGroup(rect1);

                    if (Themes.getText("disablemainthemeselector") != "true" && Current.ProgramState == ProgramState.Entry && Widgets.ButtonImage(new Rect(rect1.width - 110f, rect1.height - 96f, 96f, 96f), Themes.getThemeIcon(), Color.white, Color.green))
                    {
                        if (!Settings.modManagerAsOptionList)
                        {
                            Find.WindowStack.Add(new Dialog_ThemesList());
                        }
                        else
                        {
                            List<FloatMenuOption> list3 = new List<FloatMenuOption>();
                            String lib, libToDisplay;

                            var items = from pair in Themes.DBTex
                                        orderby pair.Key.Split('§')[1] ascending
                                        select pair;

                            foreach (var theme in items)
                            {
                                try
                                {
                                    //Obtaining modID of the theme (key to access the mod descriptor
                                    string[] parts = theme.Key.Split('§');
                                    lib = parts[1];
                                    libToDisplay = lib.Trim();

                                    if (libToDisplay.StartsWith("-"))
                                        libToDisplay = libToDisplay.Substring(1);

                                    list3.Add(new FloatMenuOption(libToDisplay, delegate
                                    {
                                        //New theme application
                                        Themes.changeThemeNow(parts[0], parts[1],true);

                                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                                }
                                catch (Exception)
                                {

                                }
                            }
                            Find.WindowStack.Add(new FloatMenu(list3));
                        }
                    }
                    float mlLeft = 185;

                    if (!Themes.expansionsIcons())
                        mlLeft = 5;
                    //RimTheme logo drawing
                    if ( (Settings.rimthemesLogoMode == 1 || (Settings.rimthemesLogoMode == 3 && Themes.getText("disablemainrimthemeslogo") != "true"))  && Widgets.ButtonImage(new Rect(mlLeft, rect1.height - 85, 280, 58), Loader.rtMainLogoTex, Color.white, Color.green))
                    {
                        var dialog = new Dialog_ModSettings();
                        Traverse.Create(dialog).Field<Mod>("selMod").Value = Utils.currentModInst;
                        Find.WindowStack.Add(dialog);
                    }  
                    GUI.EndGroup();
                }
            }
            catch(Exception e)
            {
                Themes.LogError("Patch failed : MainMenuDrawer.DoWindowContents : " + e.Message);
            }
        }
    }
}
