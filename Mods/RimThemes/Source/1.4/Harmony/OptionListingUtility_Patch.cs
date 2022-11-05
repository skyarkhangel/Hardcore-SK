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
    [HarmonyPatch(typeof(OptionListingUtility), "DrawOptionListing"), StaticConstructorOnStartup]
    class DrawOptionListing_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(ref float __result)
        {
            if (Utils.squeezedDrawOptionListingIndex != 0)
            {
                Utils.squeezedDrawOptionListingIndex -= 1;
                if (Utils.squeezedDrawOptionListingIndex == 0)
                {
                    __result = Utils.squeezedDrawOptionListingIndexReturnVal;
                    Utils.squeezedDrawOptionListingIndexReturnVal = 0;
                    return false;
                }
            }
            return true;
        }
    }

}
