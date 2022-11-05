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
    [HarmonyPatch(typeof(CustomCursor), "Activate"), StaticConstructorOnStartup]
    class Activate_Patch
    {
        private static Vector2 CursorHotspot = new Vector2(3f, 3f);

        [HarmonyPrefix]
        static bool Prefix()
        {
            if (!(Themes.initialized && Themes.vanillaThemeSaved))
                return true;

            try
            {
                Texture2D tex;
                if (Settings.disableCustomCursors)
                    tex = Themes.getThemeTex("CustomCursor", "CursorTex", Themes.VanillaThemeID);
                else
                    tex = Themes.getThemeTex("CustomCursor", "CursorTex");

                Cursor.SetCursor(tex, CursorHotspot, CursorMode.Auto);
                return false;
            }
            catch(Exception e)
            {
                Themes.LogError("CustomCursor.Activate patch failed : "+e.Message);
                return true;
            }
        }
    }
}