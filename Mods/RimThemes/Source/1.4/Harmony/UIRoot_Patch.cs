using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace aRandomKiwi.RimThemes
{
    [HarmonyPatch(typeof(UIRoot))]
    [HarmonyPatch("UIRootOnGUI")]
    [HarmonyPatch(new Type[0])]
    internal static class UIRoot_Patch
    {
        [HarmonyPostfix]
        private static void OnGUIHook()
        {
            try
            {
                if (!Themes.initialized)
                    return;
                if (Utils.needRefresh)
                {
                    //Theme update
                    UIMenuBackgroundManager.background.BackgroundOnGUI();
                    Utils.needRefresh = false;
                }
            }
            catch(Exception e)
            {
                Themes.LogError("UIRoot.UIRootOnGUI patch failed : "+e.Message);
            }
        }
    }
}