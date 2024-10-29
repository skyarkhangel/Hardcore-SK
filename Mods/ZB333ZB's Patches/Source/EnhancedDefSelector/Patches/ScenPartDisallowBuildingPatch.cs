using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using EnhancedDefSelector.Core;
using EnhancedDefSelector.Windows;

namespace EnhancedDefSelector.Patches
{
    [HarmonyPatch(typeof(ScenPart_DisallowBuilding), "DoEditInterface")]
    public class ScenPartDisallowBuildingPatch
    {
        private static class Cache
        {
            public static readonly MethodInfo PossibleBuildingDefs = AccessTools.Method(
                typeof(ScenPart_DisallowBuilding),
                "PossibleBuildingDefs");

            public static readonly FieldInfo Building = AccessTools.Field(
                typeof(ScenPart_DisallowBuilding),
                "building");
        }

        private class BuildingSelector : IScenarioDefPatch
        {
            private readonly ScenPart_DisallowBuilding scenPart;

            public BuildingSelector(ScenPart_DisallowBuilding scenPart)
            {
                this.scenPart = scenPart;
            }

            public IEnumerable<Def> GetAvailableDefs()
            {
                return (IEnumerable<ThingDef>)Cache.PossibleBuildingDefs.Invoke(scenPart, null);
            }

            public string GetCurrentDefLabel()
            {
                var building = Cache.Building.GetValue(scenPart) as ThingDef;
                return building?.LabelCap ?? "SelectBuilding".Translate();
            }

            public void OnDefSelected(Def selectedDef)
            {
                Cache.Building.SetValue(scenPart, selectedDef);
            }

            public Rect GetEditRect(Listing_ScenEdit listing)
            {
                return listing.GetScenPartRect(scenPart, ScenPart.RowHeight);
            }
        }

        private static bool Prefix(ScenPart_DisallowBuilding __instance, Listing_ScenEdit listing)
        {
            var buildingSelector = new BuildingSelector(__instance);

            if (Widgets.ButtonText(buildingSelector.GetEditRect(listing), buildingSelector.GetCurrentDefLabel()))
            {
                Find.WindowStack.Add(new DefSelectionWindow(buildingSelector));
            }

            return false;
        }
    }
}