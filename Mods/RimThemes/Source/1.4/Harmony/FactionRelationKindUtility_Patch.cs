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
    [HarmonyPatch(typeof(FactionRelationKindUtility), "GetColor"), StaticConstructorOnStartup]
    class FactionRelationKindUtility_GetColor_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(FactionRelationKind kind, ref Color __result)
        {
            try
            {
                string theme = Settings.curTheme;
                if (kind != FactionRelationKind.Neutral || !Themes.DBTextColorFactionsNeutral.ContainsKey(theme))
                    return true;

                __result = Themes.DBTextColorFactionsNeutral[theme];
                return false;
            }
            catch(Exception e)
            {
                Themes.LogError("Patch FactionRelationKindUtility.GetColor failed : " + e.Message);
                return true;
            }
        }
    }
}