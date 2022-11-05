using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;
using System.Collections;

namespace aRandomKiwi.RimThemes
{
    public static class Fonts
    {
        public static void tryParseCustomFontTag(Dictionary<string, string> fontToLoadEntry,string themeID, string tag, string value)
        {
            if (LanguageDatabase.activeLanguage == null)
                return;

            //If tag of font BY size AND LANG
            if (tag.StartsWith("customfontsize"))
            {
                //Search if presence of customFont <CurrentLang>
                string curCustomFontSizeNeeded = "";
                string se = "";
                string defaultCustomfont = "";

                if (tag.StartsWith("customfontsizetiny"))
                {
                    curCustomFontSizeNeeded = "customfontsizetiny" + LanguageDatabase.activeLanguage.folderName.ToLower();
                    se = "tinyFontSize";
                    defaultCustomfont = "customfontsizetinydefault";
                }
                else if (tag.StartsWith("customfontsizesmall"))
                {
                    curCustomFontSizeNeeded = "customfontsizesmall" + LanguageDatabase.activeLanguage.folderName.ToLower();
                    se = "smallFontSize";
                    defaultCustomfont = "customfontsizesmalldefault";
                }
                else if (tag.StartsWith("customfontsizemedium"))
                {
                    curCustomFontSizeNeeded = "customfontsizemedium" + LanguageDatabase.activeLanguage.folderName.ToLower();
                    se = "mediumFontSize";
                    defaultCustomfont = "customfontsizemediumdefault";
                }
                else
                {
                    //Otherwise it means that the tag is misspelled, we do not load the font
                    throw new Exception("Invalid customFontSize tag : " + tag);
                }

                //Match the local language loaded?
                if (curCustomFontSizeNeeded == tag)
                {
                    //We add the custom font loaded with the size info encoded in "se", the language condition being processed at this stage
                    fontToLoadEntry[se] = value;
                    fontToLoadEntry["themeID"] = themeID;

                    Themes.LogMsg("Found customFontSize " + tag + " assigned to " + value);
                }
                else
                {
                    //Otherwise test of the generic customFont relative to the current size of the present tag
                    if (tag == defaultCustomfont)
                    {
                        fontToLoadEntry[se] = value;
                        fontToLoadEntry["themeID"] = themeID;

                        Themes.LogMsg("Fallback to default customFontSize assigned to " + value);
                    }
                    /*else
                        throw new Exception("Not applyable customFont tag for the current game language found : " + tag);*/
                }
            }
            //Treatment of individualized font sizes
            else if (tag.StartsWith("customfont"))
            {
                //Search if presence of customFont <CurrentLang>
                string curCustomFontNeeded = "";
                string se = "";
                string defaultCustomfont = "";

                if (tag.StartsWith("customfonttiny"))
                {
                    curCustomFontNeeded = "customfonttiny" + LanguageDatabase.activeLanguage.folderName.ToLower();
                    se = "tinyFont";
                    defaultCustomfont = "customfonttinydefault";
                }
                else if (tag.StartsWith("customfontsmall"))
                {
                    curCustomFontNeeded = "customfontsmall" + LanguageDatabase.activeLanguage.folderName.ToLower();
                    se = "smallFont";
                    defaultCustomfont = "customfontsmalldefault";
                }
                else if (tag.StartsWith("customfontmedium"))
                {
                    curCustomFontNeeded = "customfontmedium" + LanguageDatabase.activeLanguage.folderName.ToLower();
                    se = "mediumFont";
                    defaultCustomfont = "customfontmediumdefault";
                }
                else
                {
                    //Otherwise it means that the tag is misspelled, we do not load the font
                    throw new Exception("Invalid customFont tag : " + tag);
                }

                //Match the local language loaded?
                if (curCustomFontNeeded == tag)
                {
                    //On ajoute le custom font a chargé avec l'info de taille encodé dans "se", la condition de language étant traité a ce stade
                    fontToLoadEntry[se] = value;
                    fontToLoadEntry["themeID"] = themeID;
                    //Themes.fontsToLoad.Add(d);

                    Themes.LogMsg("Found customFont " + tag + " assigned to " + value);
                }
                else
                {
                    //Sinon test du customFont generique relatif a la taille en cours du tag present
                    if (tag == defaultCustomfont)
                    {
                        fontToLoadEntry[se] = value;
                        fontToLoadEntry["themeID"] = themeID;
                        //Themes.fontsToLoad.Add(d);

                        Themes.LogMsg("Fallback to default customFont assigned to " + value);
                    }
                    /*else
                        throw new Exception("Not applyable customFont tag for the current game language found : " + tag);*/
                }
            }
            
        }


        /*
         * Loading fonts required by themes
         */
        public static void loadFonts()
        {
            //Custom font loading requests for certain themes
            if (Themes.fontsToLoad.Count != 0)
            {
                Themes.LogMsg("Processing " + Themes.fontsToLoad.Count() + " fonts to load");
                try
                {
                    int number;
                    Font font = null;
                    Font tinyFont = null;
                    Font smallFont = null;
                    Font mediumFont = null;

                    foreach (var entry in Themes.fontsToLoad)
                    {
                        string theme = entry["themeID"];
                        try
                        {
                            font = null;
                            tinyFont = null;
                            smallFont = null;
                            mediumFont = null;

                            //Finding the asset bundle storing the current font
                            foreach (AssetBundle cab in Themes.fontsPackage)
                            {
                                if (cab == null)
                                    continue;

                                //General police selector
                                if (entry.ContainsKey("font"))
                                {
                                    font = (Font)cab.LoadAsset(entry["font"]);
                                    //Font found?
                                    if (font != null)
                                        break;
                                }
                                else
                                {
                                    if (entry.ContainsKey("tinyFont") && tinyFont == null)
                                    {
                                        tinyFont = (Font)cab.LoadAsset(entry["tinyFont"]);
                                    }
                                    if (entry.ContainsKey("smallFont") && smallFont == null)
                                    {
                                        smallFont = (Font)cab.LoadAsset(entry["smallFont"]);
                                    }
                                    if (entry.ContainsKey("mediumFont") && mediumFont == null)
                                    {
                                        mediumFont = (Font)cab.LoadAsset(entry["mediumFont"]);
                                    }
                                }
                            }

                            //Check for the presence of fonts according to the case scenario
                            if (entry.ContainsKey("font"))
                            {
                                if (font == null)
                                    throw new Exception("Custom font " + entry["font"] + " not found in fonts packages");

                                Themes.DBTinyFontByTheme[theme] = font;

                                if (Settings.disableTinyCustomFont)
                                {
                                    tinyFont = Text.fontStyles[0].font;
                                }
                                else
                                    tinyFont = font;

                                smallFont = font;
                                mediumFont = font;
                            }
                            else
                            {
                                if (tinyFont != null)
                                    Themes.DBTinyFontByTheme[theme] = tinyFont;

                                if (tinyFont == null || Settings.disableTinyCustomFont)
                                {
                                    tinyFont = Text.fontStyles[0].font;
                                    Themes.LogMsg("Warning tinyFont not found for theme " + theme + " loading default : "+tinyFont);
                                }
                                if (smallFont == null)
                                {
                                    smallFont = Text.fontStyles[1].font;
                                    Themes.LogMsg("Warning smallFont not found for theme " + theme + " loading default : "+smallFont);
                                }
                                if (mediumFont == null)
                                {
                                    mediumFont = Text.fontStyles[2].font;
                                    Themes.LogMsg("Warning mediumFont not found for theme " + theme + " loading default : "+mediumFont);
                                }
                            }

                            //font = (Font)ab.LoadAsset(entry["font"]);
                            int tiny = Text.fontStyles[0].font.fontSize;
                            int small = Text.fontStyles[1].font.fontSize;
                            int medium = Text.fontStyles[2].font.fontSize;

                            string tinyStr = Themes.getText("fontsizetiny", theme);
                            string smallStr = Themes.getText("fontsizesmall", theme);
                            string mediumStr = Themes.getText("fontsizemedium", theme);

                            if (tinyStr == null && entry.ContainsKey("tinyFontSize"))
                            {
                                //Attempt to override sizes from theme files
                                if (int.TryParse(entry["tinyFontSize"], out number))
                                {
                                    //Themes.LogMsg("Custom tiny font size to "+number);
                                    tiny = number;
                                }
                            }
                            else
                            {
                                //Attempt to override sizes from theme files
                                if (int.TryParse(tinyStr, out number))
                                {
                                    //Themes.LogMsg("Custom tiny font size to "+number);
                                    tiny = number;
                                }
                            }

                            if (smallStr == null && entry.ContainsKey("smallFontSize"))
                            {
                                //Tentative override des tailles depuis les fichiers de theme
                                if (int.TryParse(entry["smallFontSize"], out number))
                                {
                                    //Themes.LogMsg("Custom tiny font size to "+number);
                                    small = number;
                                }
                            }
                            else
                            {
                                if (int.TryParse(smallStr, out number))
                                {
                                    //Themes.LogMsg("Custom small font size to " + number);
                                    small = number;
                                }
                            }

                            if (mediumStr == null && entry.ContainsKey("mediumFontSize"))
                            {
                                //Tentative override des tailles depuis les fichiers de theme
                                if (int.TryParse(entry["mediumFontSize"], out number))
                                {
                                    //Themes.LogMsg("Custom tiny font size to "+number);
                                    medium = number;
                                }
                            }
                            else
                            {
                                if (int.TryParse(mediumStr, out number))
                                {
                                    //Themes.LogMsg("Custom medium font size to " + number);
                                    medium = number;
                                }
                            }

                            //Saving in reserve of the small font allowing later to be able to restore it (change in the parameters)
                            Themes.DBTinyFontSizeByTheme[theme] = tiny;

                            //Squeeze the size after calculating it if tinyFont disabled
                            if (Settings.disableTinyCustomFont)
                            {
                                tiny = Text.fontStyles[0].font.fontSize;
                            }

                            //Themes.LogMsg("Loading font " + font.name + " of theme " + theme);

                            //We do this to prevent the font from being blurry when it is changed axis (rotation)
                            if (!Settings.disableFontFilterModePoint)
                            {
                                tinyFont.material.mainTexture.filterMode = FilterMode.Point;
                                smallFont.material.mainTexture.filterMode = FilterMode.Point;
                                mediumFont.material.mainTexture.filterMode = FilterMode.Point;
                            }

                            Themes.DBGUIStyle[theme] = new Dictionary<GameFont, GUIStyle>();
                            Themes.DBGUIStyle[theme][GameFont.Tiny] = new GUIStyle(GUI.skin.label);
                            Themes.DBGUIStyle[theme][GameFont.Tiny].font = tinyFont;
                            Themes.DBGUIStyle[theme][GameFont.Tiny].fontSize = tiny;

                            Themes.DBGUIStyle[theme][GameFont.Small] = new GUIStyle(GUI.skin.label);
                            Themes.DBGUIStyle[theme][GameFont.Small].font = smallFont;
                            Themes.DBGUIStyle[theme][GameFont.Small].fontSize = small;
                            Themes.DBGUIStyle[theme][GameFont.Small].contentOffset = new Vector2(0f, -1f);

                            Themes.DBGUIStyle[theme][GameFont.Medium] = new GUIStyle(GUI.skin.label);
                            Themes.DBGUIStyle[theme][GameFont.Medium].font = mediumFont;
                            Themes.DBGUIStyle[theme][GameFont.Medium].fontSize = medium;



                            Themes.DBGUIStyleTextField[theme] = new Dictionary<GameFont, GUIStyle>();
                            Themes.DBGUIStyleTextField[theme][GameFont.Tiny] = new GUIStyle(GUI.skin.textField);
                            Themes.DBGUIStyleTextField[theme][GameFont.Tiny].alignment = TextAnchor.MiddleLeft;
                            Themes.DBGUIStyleTextField[theme][GameFont.Tiny].font = tinyFont;
                            Themes.DBGUIStyleTextField[theme][GameFont.Tiny].fontSize = tiny;

                            Themes.DBGUIStyleTextField[theme][GameFont.Small] = new GUIStyle(GUI.skin.textField);
                            Themes.DBGUIStyleTextField[theme][GameFont.Small].alignment = TextAnchor.MiddleLeft;
                            Themes.DBGUIStyleTextField[theme][GameFont.Small].font = smallFont;
                            Themes.DBGUIStyleTextField[theme][GameFont.Tiny].fontSize = small;

                            Themes.DBGUIStyleTextField[theme][GameFont.Medium] = new GUIStyle(GUI.skin.textField);
                            Themes.DBGUIStyleTextField[theme][GameFont.Medium].alignment = TextAnchor.MiddleLeft;
                            Themes.DBGUIStyleTextField[theme][GameFont.Medium].font = mediumFont;
                            Themes.DBGUIStyleTextField[theme][GameFont.Tiny].fontSize = medium;




                            Themes.DBGUIStyleTextArea[theme] = new Dictionary<GameFont, GUIStyle>();
                            Themes.DBGUIStyleTextArea[theme][GameFont.Tiny] = new GUIStyle(Themes.DBGUIStyleTextField[theme][GameFont.Tiny]);
                            Themes.DBGUIStyleTextArea[theme][GameFont.Tiny].alignment = TextAnchor.UpperLeft;
                            Themes.DBGUIStyleTextArea[theme][GameFont.Tiny].wordWrap = true;
                            Themes.DBGUIStyleTextArea[theme][GameFont.Tiny].fontSize = tiny;

                            Themes.DBGUIStyleTextArea[theme][GameFont.Small] = new GUIStyle(Themes.DBGUIStyleTextField[theme][GameFont.Small]);
                            Themes.DBGUIStyleTextArea[theme][GameFont.Small].alignment = TextAnchor.UpperLeft;
                            Themes.DBGUIStyleTextArea[theme][GameFont.Small].wordWrap = true;
                            Themes.DBGUIStyleTextArea[theme][GameFont.Small].fontSize = small;

                            Themes.DBGUIStyleTextArea[theme][GameFont.Medium] = new GUIStyle(Themes.DBGUIStyleTextField[theme][GameFont.Medium]);
                            Themes.DBGUIStyleTextArea[theme][GameFont.Medium].alignment = TextAnchor.UpperLeft;
                            Themes.DBGUIStyleTextArea[theme][GameFont.Medium].wordWrap = true;
                            Themes.DBGUIStyleTextArea[theme][GameFont.Medium].fontSize = medium;

                            Themes.DBGUIStyleTextAreaReadOnly[theme] = new Dictionary<GameFont, GUIStyle>();
                            for (int k = 0; k < Text.textAreaReadOnlyStyles.Length; k++)
                            {
                                Themes.DBGUIStyleTextAreaReadOnly[theme][(GameFont)k] = new GUIStyle(Themes.DBGUIStyleTextArea[theme][(GameFont)k]);
                                GUIStyle guistyle = Themes.DBGUIStyleTextAreaReadOnly[theme][(GameFont)k];
                                guistyle.normal.background = null;
                                guistyle.active.background = null;
                                guistyle.onHover.background = null;
                                guistyle.hover.background = null;
                                guistyle.onFocused.background = null;
                                guistyle.focused.background = null;
                            }
                            GUI.skin.settings.doubleClickSelectsWord = true;
                            int num = 0;
                            IEnumerator enumerator = Enum.GetValues(typeof(GameFont)).GetEnumerator();
                            try
                            {
                                Themes.DBGUIStyleLineHeight[theme] = new Dictionary<GameFont, float>();
                                Themes.DBGUIStyleSpaceBetweenLine[theme] = new Dictionary<GameFont, float>();
                                while (enumerator.MoveNext())
                                {
                                    object obj = enumerator.Current;
                                    GameFont font4 = (GameFont)obj;
                                    Text.Font = font4;
                                    Themes.DBGUIStyleLineHeight[theme][(GameFont)num] = Text.CalcHeight("W", 999f);
                                    Themes.DBGUIStyleSpaceBetweenLine[theme][(GameFont)num] = Text.CalcHeight("W\nW", 999f) - Text.CalcHeight("W", 999f) * 2f;
                                    num++;
                                }
                            }
                            finally
                            {
                                IDisposable disposable;
                                if ((disposable = (enumerator as IDisposable)) != null)
                                {
                                    disposable.Dispose();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Themes.LogError("Error : " + e.Message);
                            Themes.DBGUIStyle[theme] = null;
                        }
                    }
                    Themes.fontsToLoad.Clear();
                }
                catch (Exception e)
                {
                    Themes.LogError("fontsToLoad : " + e.Message);
                }
            }
        }
    }
}
