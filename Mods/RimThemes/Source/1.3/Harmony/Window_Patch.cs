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
    [HarmonyPatch(typeof(Window), "WindowOnGUI"), StaticConstructorOnStartup]
    class WindowStackOnGUI_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(Window __instance)
        {
            try {
                if (Themes.windowsShadow())
                    __instance.drawShadow = true;
                else
                    __instance.drawShadow = false;

                WindowAnim anim = Themes.getWindowAnimation();
                string wid = __instance.ID.ToString();

                //First showed console ==> affichage prioritaire
                /*if(!Utils.firstOpenedConsole && __instance is EditWindow_Log)
                {
                    Utils.firstOpenedConsole = true;
                    Utils.vipWindowID = __instance.ID;
                }*/

                //We reserve the appearance animation only for the main dialog windows
                if (anim != WindowAnim.None && ( (__instance.layer == WindowLayer.Dialog && __instance.forcePause) || (__instance.layer == WindowLayer.GameUI && __instance.ID == 235086) || (__instance is EditWindow)))
                {
                    //If the first time we see the window we change its size and initiate the variables
                    float TMS = Time.fixedUnscaledTime * 1000;

                    if (!Themes.DBEffect.ContainsKey(wid))
                    {
                        //Log.Message("Init Window anim ==>" + wid);
                        //We define as the window as in progress / or finished of init
                        Themes.DBEffect[wid] = 1;
                        //On Init le voyant d'animation finie
                        Themes.DBEffect["END" + wid] = 0;
                        //On init le compteur de gt 
                        Themes.DBEffect["TMS" + wid] = TMS;
                        //Nombre d'appels (au 25 eme cloture event)
                        Themes.DBEffect["NBC" + wid] = 0;

                        if (anim == WindowAnim.Clip)
                        {
                            //Step saving 
                            Themes.DBEffect["PITCH" + wid] = (__instance.windowRect.height - 30) / 25;
                            Themes.DBEffect["TARGETY" + wid] = __instance.windowRect.y;
                            Themes.DBEffect["TARGETH" + wid] = __instance.windowRect.height;
                            __instance.windowRect.y = (__instance.windowRect.y + (__instance.windowRect.height / 2)) - (30 / 2);
                            __instance.windowRect.height = 30;
                        }
                        else if (anim == WindowAnim.SlideRight)
                        {
                            Themes.DBEffect["PITCH" + wid] = (__instance.windowRect.width + __instance.windowRect.x) / 25;
                            Themes.DBEffect["TARGET" + wid] = __instance.windowRect.x;
                            __instance.windowRect.x = -1 * __instance.windowRect.width;
                        }
                        else if (anim == WindowAnim.SlideLeft)
                        {
                            Themes.DBEffect["PITCH" + wid] = (__instance.windowRect.width + __instance.windowRect.x) / 25;
                            Themes.DBEffect["TARGET" + wid] = __instance.windowRect.x;
                            __instance.windowRect.x = UI.screenWidth;
                        }
                        else if (anim == WindowAnim.SlideTop)
                        {
                            Themes.DBEffect["PITCH" + wid] = (__instance.windowRect.height + __instance.windowRect.y) / 25;
                            Themes.DBEffect["TARGET" + wid] = __instance.windowRect.y;
                            __instance.windowRect.y = UI.screenHeight;
                        }
                        else if (anim == WindowAnim.SlideBottom)
                        {
                            Themes.DBEffect["PITCH" + wid] = (__instance.windowRect.height + __instance.windowRect.y) / 25;
                            Themes.DBEffect["TARGET" + wid] = __instance.windowRect.y;
                            __instance.windowRect.y = -1 * __instance.windowRect.height;
                        }
                    }
                    else if (Themes.DBEffect["END" + wid] != 1)
                    {
                        Themes.DBEffect["NBC" + wid]++;
                        Themes.DBEffect["TMS" + wid] = TMS;

                        if (anim == WindowAnim.Clip)
                        {
                            //If anima not finished we continue the current animation effect
                            __instance.windowRect.y -= Themes.DBEffect["PITCH" + wid] / 2;
                            __instance.windowRect.height += Themes.DBEffect["PITCH" + wid];
                            if (Themes.DBEffect["NBC" + wid] >= 25)
                            {
                                __instance.windowRect.y = Themes.DBEffect["TARGETY" + wid];
                                __instance.windowRect.height = Themes.DBEffect["TARGETH" + wid];
                            }
                        }
                        else if (anim == WindowAnim.SlideRight)
                        {
                            __instance.windowRect.x += Themes.DBEffect["PITCH" + wid];
                            if (Themes.DBEffect["NBC" + wid] >= 25)
                                __instance.windowRect.x = Themes.DBEffect["TARGET" + wid];
                        }
                        else if (anim == WindowAnim.SlideLeft)
                        {
                            __instance.windowRect.x -= Themes.DBEffect["PITCH" + wid];
                            if (Themes.DBEffect["NBC" + wid] >= 25)
                                __instance.windowRect.x = Themes.DBEffect["TARGET" + wid];
                        }
                        else if (anim == WindowAnim.SlideTop)
                        {
                            __instance.windowRect.y -= Themes.DBEffect["PITCH" + wid];
                            if (Themes.DBEffect["NBC" + wid] >= 25)
                                __instance.windowRect.y = Themes.DBEffect["TARGET" + wid];
                        }
                        else if (anim == WindowAnim.SlideBottom)
                        {
                            __instance.windowRect.y += Themes.DBEffect["PITCH" + wid];
                            if (Themes.DBEffect["NBC" + wid] >= 25)
                                __instance.windowRect.y = Themes.DBEffect["TARGET" + wid];
                        }

                        //If reached end 
                        if (Themes.DBEffect["NBC" + wid] >= 25)
                        {
                            Themes.DBEffect["END" + wid] = 1;
                        }
                    }
                }
                else {
                    //Avoid displaying anim if window already displayed with theme without anim (theme manager)
                    if (__instance is Dialog_ThemesList && !Themes.DBEffect.ContainsKey(wid))
                    {
                        Themes.DBEffect[wid] = 1;
                        Themes.DBEffect["END" + wid] = 1;
                    }
                }

                if (!Themes.dialogStacking() && !Utils.isNSBlacklistedWindowsType(__instance) &&  ( (__instance.layer == WindowLayer.Dialog)
                    || (__instance.layer == WindowLayer.GameUI && __instance.ID == -235086)
                    || (__instance is MainTabWindow && !(__instance is MainTabWindow_Inspect))
                    || (__instance is EditWindow)))
                {

                    if (Utils.lastShowedWin.Count() != 0 && Utils.lastShowedWin.Last().type == 2)
                    {
                        if (__instance.layer == WindowLayer.GameUI && __instance.ID == -235086)
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        //IEnumerable<Window> ld = Find.WindowStack.Windows.Where((x => (((x.layer == WindowLayer.Dialog) || x is EditWindow || x is MainTabWindow) )));
                        Window lwin=null;// = ld.Last();
                        foreach (Window win in Find.WindowStack.Windows)
                        {
                            //If the exception of the start of the 1st console, it is displayed in priority
                            /*if(Utils.vipWindowID == win.ID)
                            {
                                lwin = win;
                                break;
                            }*/

                            if ((lwin == null || win.ID >= lwin.ID) && !Utils.isNSBlacklistedWindowsType(win) &&
                                ((win.layer == WindowLayer.Dialog) || (win is MainTabWindow && !(__instance is MainTabWindow_Inspect))|| (win is EditWindow)))
                            {
                                lwin = win;
                            }
                        }
                        //Log.Message(">>" + lwin.GetType()+" "+ (lwin.layer == WindowLayer.Dialog)+" "+(lwin is MainTabWindow)+" "+(lwin is EditWindow));
                        if (lwin == __instance)// || !lwin.forcePause)
                            return true;
                        else
                            return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Themes.LogError("Patch Window.WindowOnGUI failed : " + e.Message);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Window), "Close"), StaticConstructorOnStartup]
    class Window_Close_Patch
    {
        [HarmonyPostfix]
        static void Listener(bool doCloseSound, Window __instance)
        {
            try
            {
                /*if ((__instance.layer == WindowLayer.Dialog && __instance.ID != -235086) || __instance is EditWindow)
                {
                    Utils.lastShowedWin.Pop();
                    Log.Message("Dec");
                }*/

                string wid = __instance.ID.ToString();

                //IF necessary, we clean the dictionary keys
                if (Themes.DBEffect.ContainsKey(wid))
                {
                    Themes.DBEffect.Remove(wid);
                    Themes.DBEffect.Remove("NBC" + wid);
                    Themes.DBEffect.Remove("END" + wid);
                    Themes.DBEffect.Remove("TMS" + wid);
                    Themes.DBEffect.Remove("PITCH" + wid);
                    Themes.DBEffect.Remove("TARGET" + wid);
                    if (Themes.DBEffect.ContainsKey("TARGETY" + wid))
                    {
                        Themes.DBEffect.Remove("TARGETY" + wid);
                        Themes.DBEffect.Remove("TARGETH" + wid);
                    }
                }
            }
            catch(Exception e)
            {
                Themes.LogError("Patch Window.Close failed : " + e.Message);
            }
        }
    }
}
