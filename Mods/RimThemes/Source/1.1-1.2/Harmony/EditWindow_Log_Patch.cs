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
    [HarmonyPatch(typeof(EditWindow_Log), "DoWindowContents"), StaticConstructorOnStartup]
    class EditWindow_Log_DoWindowContents_PrefixPatch
    {
        [HarmonyPrefix]
        static void PrefixListener(Rect inRect)
        {
            if(Settings.disableCustomFontsInConsole)
                //Font definition as being that of the current theme
                Themes.forcedFontTheme = Themes.VanillaThemeID;
        }
    }

    [HarmonyPatch(typeof(EditWindow_Log), "DoWindowContents"), StaticConstructorOnStartup]
    class EditWindow_Log_DoWindowContents_PostfixPatch
    {
        [HarmonyPostfix]
        static void PostfixListener(Rect inRect)
        {
            if (Settings.disableCustomFontsInConsole)
                Themes.forcedFontTheme = "";
        }
    }
}
