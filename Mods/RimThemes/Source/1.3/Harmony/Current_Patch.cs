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
        [HarmonyPatch(typeof(Current), "Notify_LoadedSceneChanged"), StaticConstructorOnStartup]
        class Notify_LoadedSceneChanged_Patch
        {
            [HarmonyPostfix]
            static void Listener()
            {
                if (GenScene.InEntryScene)
                {
                    //If activate we try to change the background of the main menu
                    if (!Settings.disableRandomBg)
                        Themes.setNewRandomBg();

                    LoaderGM comp = GameObject.Find("Camera").AddComponent<LoaderGM>() as LoaderGM;
                 }
                else
                {
                    LoaderGM comp = GameObject.Find("Camera").AddComponent<LoaderGM>() as LoaderGM;
                }
            }
        }
}
