using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using System.IO;
using Verse.Sound;
using HarmonyLib;

namespace aRandomKiwi.RimThemes
{
    [HarmonyPatch(typeof(Page_ModsConfig), "DoWindowContents"), StaticConstructorOnStartup]
    class DoWindowContents_Patch
    {
        [HarmonyPrefix]
        static bool ListenerPrefix(Page_ModsConfig __instance, Rect rect)
        {
            Utils.tempDisableNoTransparentText = true;
            return true;
        }

        [HarmonyPostfix]
        static void ListenerPostfix(Page_ModsConfig __instance, Rect rect)
        {
            ModMetaData curMod = __instance.selectedMod;

            try
            {
                if (curMod != null && curMod.enabled)
                {
                    string path;
                    //Check if the mod has themes
                    if (curMod.PackageId == Utils.currentMod.PackageId)
                        path = curMod.RootDir.FullName + Path.DirectorySeparatorChar + "Textures" + Path.DirectorySeparatorChar + "Themes";
                    else
                        path = curMod.RootDir.FullName + Path.DirectorySeparatorChar + "RimThemes";
                    //Theme folder found
                    if (Directory.Exists(path))
                    {
                        //Default theme search
                        string[] folders = System.IO.Directory.GetDirectories(path);
                        foreach (var dir in folders)
                        {
                            var theme = dir.Remove(0, dir.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                            if (theme.StartsWith("-"))
                            {
                                Rect rect3 = new Rect(rect.width - (64 + 8), 48f, 64f, 64f);
                                if (Widgets.ButtonImage(rect3, Loader.logoSmallTex))
                                {
                                    if (curMod.PackageId == Utils.currentMod.PackageId)
                                        Themes.changeThemeNow("-1", theme,true);
                                    else
                                        Themes.changeThemeNow(curMod.PackageId, theme,true);
                                }
                            }
                        }
                    }
                }
                Utils.tempDisableNoTransparentText = false;
            }
            catch(Exception e)
            {
                Utils.tempDisableNoTransparentText = false;
                Themes.LogError("Patch failed : Page_ModsConfig.DoWindowContents : " + e.Message);
            }
        }
    }
}
