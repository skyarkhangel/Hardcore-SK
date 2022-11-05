using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine.Video;

namespace aRandomKiwi.RimThemes
{
    [StaticConstructorOnStartup]
    static class Utils
    {
        public static bool tempDisableDynColor= false;
        public static bool tempDisableNoTransparentText = false;
        public static bool tempDisableButtonsBackground = false;
        public static bool textFontSetterLock = false;
        public static int squeezedDrawOptionListingIndex = 0;
        public static float squeezedDrawOptionListingIndexReturnVal = 0;

        public static VideoPlayer CurrentMainAnimatedBg = null;
        public static bool CurrentMainAnimatedBgPlaying = false;
        public static bool CurrentMainAnimatedBgSourceSet = false;

        public static string sponsor = "M";

        //static public bool firstOpenedConsole = false;
        //static public int vipWindowID = -1;
        static public List<WDESC> lastShowedWin = new List<WDESC>();
        static public bool needRefresh = false;
        static public ModContentPack currentMod;
        static public Mod currentModInst;
        static public Settings modSettings;
        static public string releaseInfo = "RimThemes NX";
        static public string releaseDesc = "Changes :" + Environment.NewLine
            + "-Added the name of the current main donor in the themes selection menu" + Environment.NewLine
            + "-Improved all default themes" + Environment.NewLine
            + "-Added new setting allowing to adjust all windows opacity level" + Environment.NewLine
            + "-Added new setting allowing to hide the RimThemes logo in the main menu" + Environment.NewLine
            + "-Added new settings allowing to hide the main menu expansions icons, info corner and more" + Environment.NewLine
            + "-Added new setting allowing to hide windows shadows" + Environment.NewLine
            + "-Added new default theme 'Rim-Life 2' and 'Mechanoid cluster'" + Environment.NewLine
            + "-Fixed the overlapping issue with the expansions icons buttons (in the bottom left)" + Environment.NewLine
            + "-Fixed confirm button texture issue (vanilla texture applied instead of the current theme)" + Environment.NewLine
            + "-Few others minors improvements" + Environment.NewLine + Environment.NewLine
            + "For themes makers :" + Environment.NewLine
            + "-Fixed tapestry border color tag bug (color was never applied in themes)" + Environment.NewLine
            + "-Added support for custom APNG loader FPS with the new tag 'loaderFPS'" + Environment.NewLine
            + "-Few others new tags (download the Theme example package for more details)" + Environment.NewLine + Environment.NewLine
            + "/!\\ Notice : Support for 1.0 is dropped, only RimThemes 2020R1 is compatible with Rimworld 1.0." + Environment.NewLine;

        private static Traverse cachedLabelWidthCache = null;
        static private bool initCachedLabelWidthCache = false;

        public static void resetCachedLabelWidthCache()
        {
            if (!initCachedLabelWidthCache)
            {
                initCachedLabelWidthCache = true;
                cachedLabelWidthCache = Traverse.CreateWithType("Verse.GenUI").Field("labelWidthCache");
            }

            //Reset label width caches
            Dictionary<string, Vector2> labelWidthCache = (Dictionary<string, Vector2>)cachedLabelWidthCache.GetValue();
            labelWidthCache.Clear();
            //GenUI.labelWidthCache.Clear();
        }

        public static void startLoadingTheme()
        {
            AssetBundle cab = null;
            LoaderGM.curStep = LoaderSteps.loadingTheme;
            //Loading all dependances from the themes THEN generating theme textures inside LoaderGM
            Themes.startInit();

            //We notify the loader to load the preloaded textures
            LoaderGM.themeLoadMode = 1;
            Thread.Sleep(250);
            LoaderGM.texThemesToLoad = true;

            //We notify the loader to load the preloaded songs
            LoaderGM.themeLoadMode = 2;
            Thread.Sleep(250);
            LoaderGM.songsToLoad = true;

            //Loading the font asset bundle
            try
            {
                LoaderGM.themeLoadMode = 3;
                Thread.Sleep(250);
                //Loading the encoded font bundle into memory
                Themes.fontsPackage.Add(AssetBundle.LoadFromMemory(FontsPackage.fonts)); //LoadFromFile(Utils.currentMod.RootDir + Path.DirectorySeparatorChar + "fontspackage");
                Themes.LogMsg("Load main fonts package OK");
            }
            catch (Exception e)
            {
                Themes.fontsPackage = null;
                Themes.LogError("Loading fonts package : " + e.Message);
            }

            //Loading of potential font assetsbundle provided by mods
            foreach (string fbPath in Themes.DBfontsBundleToLoad)
            {
                try
                {
                    cab = AssetBundle.LoadFromFile(fbPath);
                    if (cab == null)
                        throw new Exception("Invalid font package "+fbPath);

                    Themes.fontsPackage.Add(cab);
                    Themes.LogMsg("Load external fonts package "+fbPath+" OK");
                }
                catch (Exception e)
                {
                    Themes.LogError("Loading external fonts package : " + e.Message);
                }
            }


            //Loading Fonts
            LoaderGM.themeLoadMode = 4;
            Thread.Sleep(250);
            LoaderGM.fontsToLoad = true;

            //If enabled we try to change the background of the main menu
            if (!Settings.disableRandomBg)
                Themes.setNewRandomBg();
        }


        /*
         * Check if an image whose path is passed as a parameter exists (without the extension, the test function exists for this image in .png and .jpg format)
         */
        static public bool texFileExist(string path)
        {
            if (File.Exists(path + ".png"))
                return true;
            else if (File.Exists(path + ".jpg"))
                return true;
            else
                return false;
        }

        static public byte[] readAllBytesTexFile(string path)
        {
            if (File.Exists(path + ".png"))
                return File.ReadAllBytes(path + ".png");
            else if (File.Exists(path + ".jpg"))
                return File.ReadAllBytes(path + ".jpg");
            else
                return null;
        }


        static public bool isValidImgExt(string ext)
        {
            if (ext == ".png" || ext == ".jpg")
                return true;
            else
                return false;
        }


        static public bool isNSBlacklistedWindowsType(Window win)
        {
            string type = win.GetType().FullName;
            if (type == "DubsMintMinimap.MainTabWindow_MiniMap" || type == "DubsMintMinimap.MainTabWindow_MiniMapSetting")
                return true;
            else
                return false;
        }

        static public string ReplaceLastOccurrence(string str, string toReplace, string replacement)
        {
            return Regex.Replace(str, $@"^(.*){toReplace}(.*?)$", $"$1{replacement}$2");
        }

        public static string RWBaseFolderPath
        {
            get
            {
                return new DirectoryInfo(UnityData.dataPath).Parent.FullName;
            }
        }

        public static void applyWindowFillColorOpacityOverride(string newTheme)
        {
            if (Settings.disabledOverrideThemeWindowFillColorAlpha)
                return;

            Type classType = typeof(FloatMenuOption).Assembly.GetType("Verse.Widgets");
            if (classType == null)
                return;

            Color cColor=Color.black;

            //Change of color component filling of the current theme
            if (Themes.DBColor.ContainsKey(newTheme) && Themes.DBColor[newTheme].ContainsKey("Widgets") && Themes.DBColor[newTheme]["Widgets"].ContainsKey("WindowBGFillColor"))
            {
                cColor = Themes.DBColor[newTheme]["Widgets"]["WindowBGFillColor"];
                cColor.a = Settings.overrideThemeWindowFillColorAlphaLevel;
                Themes.DBColor[newTheme]["Widgets"]["WindowBGFillColor"] = cColor;


            }
            else
            {
                if (Themes.DBColor.ContainsKey("-1§Vanilla") && Themes.DBColor["-1§Vanilla"].ContainsKey("Widgets") && Themes.DBColor["-1§Vanilla"]["Widgets"].ContainsKey("WindowBGFillColor"))
                {
                    //Change alpha component of the vanilla theme
                    cColor = Themes.DBColor["-1§Vanilla"]["Widgets"]["WindowBGFillColor"];
                    cColor.a = Settings.overrideThemeWindowFillColorAlphaLevel;
                    Themes.DBColor["-1§Vanilla"]["Widgets"]["WindowBGFillColor"] = cColor;
                }
                else
                    return;
            }

            classType.GetField("WindowBGFillColor", (BindingFlags)(BindingFlags.Public | BindingFlags.Static)).SetValue(null, cColor);
        }
    }
}
