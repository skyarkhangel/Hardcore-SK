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
    [HarmonyPatch(typeof(ScenPart_StartingResearch), "DoEditInterface")]
    public class ScenPartStartingResearchPatch
    {
        private static class Cache
        {
            public static readonly MethodInfo NonRedundantResearchProjects = AccessTools.Method(typeof(ScenPart_StartingResearch), "NonRedundantResearchProjects");
            public static readonly FieldInfo Project = AccessTools.Field(typeof(ScenPart_StartingResearch), "project");
        }

        private class ResearchSelector : IScenarioDefPatch
        {
            private readonly ScenPart_StartingResearch scenPart;

            public ResearchSelector(ScenPart_StartingResearch scenPart)
            {
                this.scenPart = scenPart;
            }

            public IEnumerable<Def> GetAvailableDefs()
            {
                return (IEnumerable<ResearchProjectDef>)Cache.NonRedundantResearchProjects.Invoke(scenPart, null);
            }

            public string GetCurrentDefLabel()
            {
                var project = (ResearchProjectDef)Cache.Project.GetValue(scenPart);
                return project.LabelCap;
            }

            public void OnDefSelected(Def selectedDef)
            {
                Cache.Project.SetValue(scenPart, selectedDef);
            }

            public Rect GetEditRect(Listing_ScenEdit listing)
            {
                return listing.GetScenPartRect(scenPart, ScenPart.RowHeight);
            }
        }

        private static bool Prefix(ScenPart_StartingResearch __instance, Listing_ScenEdit listing)
        {
            var researchSelector = new ResearchSelector(__instance);

            if (Widgets.ButtonText(researchSelector.GetEditRect(listing), researchSelector.GetCurrentDefLabel()))
            {
                Find.WindowStack.Add(new DefSelectionWindow(researchSelector));
            }

            return false;
        }
    }
}