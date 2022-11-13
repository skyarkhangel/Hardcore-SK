using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Video;
using Verse;
using Verse.Sound;
using HarmonyLib;

namespace aRandomKiwi.RimThemes
{
    [HarmonyPatch(typeof(Game), "FinalizeInit")]
    internal static class Background_FinalizeInitPatch
    {
        internal static void Postfix()
        {
            //QualitySettings.vSyncCount = 0;
            Utils.CurrentMainAnimatedBgPlaying = false;
        }
    }

    [HarmonyPatch(typeof(UI_BackgroundMain), "BackgroundOnGUI"), StaticConstructorOnStartup]
    class UI_BackgroundMain_Patch
    {
        private static readonly Vector2 BGPlanetSize = new Vector2(2048f, 1280f);
        public static Vector2 MainBackgroundSize = new Vector2(2000f, 1190f);

        [HarmonyPrefix]
        static bool Prefix()
        {
            try
            {
                //If custom loader disabled and still loading before loading step of theme textures then we leave the usual bg(NO blackscreen)
                // Or if the current theme does not exist(steam HS, mod theme deleted)
                if ((Settings.disableCustomLoader && !LoaderGM.themeTexAlreadyLoaded) || Settings.disableWallpaper || !Themes.currentThemeExist)
                    return true;

                //Obtaining the applicable theme background
                string curTheme = Themes.getCustomBackgroundApplyableTheme();


                if (Settings.curTheme == Themes.VanillaThemeID && Settings.disableRandomBg)
                    return true;

                //If the animated background is available and loaded, we start reading it!
                if (Current.ProgramState == ProgramState.Entry && Themes.DBAnimatedBackground.ContainsKey(curTheme) && !Settings.disableVideoBg)
                {
                    Rect pos;
                    if ((double)Screen.width <= (double)Screen.height * ((double)MainBackgroundSize.x / (double)MainBackgroundSize.y))
                    {
                        int height = Screen.height;
                        float width = (float)Screen.height * (MainBackgroundSize.x / MainBackgroundSize.y);
                        pos = new Rect((float)(Screen.width / 2) - width / 2f, 0.0f, width, (float)height);
                    }
                    else
                    {
                        int width = Screen.width;
                        float height = (float)Screen.width * (MainBackgroundSize.y / MainBackgroundSize.x);
                        pos = new Rect(0.0f, (float)(Screen.height / 2) - height / 2f, (float)width, height);
                    }

                    if (!Utils.CurrentMainAnimatedBgPlaying)
                    {
                        if (Utils.CurrentMainAnimatedBg == null && Themes.DBAnimatedBackground[curTheme].isDone)
                            Utils.CurrentMainAnimatedBg = Themes.DBAnimatedBackground[curTheme].GetMovieTexture();

                        if (Utils.CurrentMainAnimatedBg != null && Utils.CurrentMainAnimatedBg.isReadyToPlay)
                        {
                            GUI.DrawTexture(pos, Utils.CurrentMainAnimatedBg, ScaleMode.ScaleToFit);
                            Themes.LogMsg("Start animated background animation of duration "+ Utils.CurrentMainAnimatedBg.duration);
                            Utils.CurrentMainAnimatedBg.loop = true;
                            Utils.CurrentMainAnimatedBg.Play();
                            Utils.CurrentMainAnimatedBgPlaying = true;
                        }
                        else
                        {
                            //To wait RimWorld original background display
                            return true;
                        }
                    }
                    else
                    {
                        GUI.DrawTexture(pos, Utils.CurrentMainAnimatedBg, ScaleMode.ScaleToFit);
                    }
                }
                else
                {
                    bool flag = true;

                    if ((float)UI.screenWidth > (float)UI.screenHeight * (BGPlanetSize.x / BGPlanetSize.y))
                    {
                        flag = false;
                    }
                    Rect position;
                    if (flag)
                    {
                        float height = (float)UI.screenHeight;
                        float num = (float)UI.screenHeight * (BGPlanetSize.x / BGPlanetSize.y);
                        position = new Rect((float)(UI.screenWidth / 2) - num / 2f, 0f, num, height);
                    }
                    else
                    {
                        float width = (float)UI.screenWidth;
                        float num2 = (float)UI.screenWidth * (BGPlanetSize.y / BGPlanetSize.x);
                        position = new Rect(0f, (float)(UI.screenHeight / 2) - num2 / 2f, width, num2);
                    }

                    Texture bg = Themes.getThemeTex("UI_BackgroundMain", "BGPlanet",curTheme);
                    if (bg != null)
                        GUI.DrawTexture(position, bg, ScaleMode.ScaleToFit);
                }

                return false;
            }
            catch (Exception e)
            {
                Themes.LogError("UI.BackgroundMain patch failed : "+e.Message);
                return true;
            }
        }
    }
   
}
