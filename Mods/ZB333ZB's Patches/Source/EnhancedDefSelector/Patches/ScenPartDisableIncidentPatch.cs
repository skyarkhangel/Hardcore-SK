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
    [HarmonyPatch]
    public class ScenPartDisableIncidentPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(ScenPart_DisableIncident), "DoIncidentEditInterface");
        }

        private static class Cache
        {
            public static readonly MethodInfo RandomizableIncidents = AccessTools.Method(
                typeof(ScenPart_DisableIncident),
                "RandomizableIncidents");

            public static readonly FieldInfo Incident = AccessTools.Field(
                typeof(ScenPart_IncidentBase),
                "incident");
        }

        private class IncidentSelector : IScenarioDefPatch
        {
            private readonly ScenPart_DisableIncident scenPart;

            public IncidentSelector(ScenPart_DisableIncident scenPart)
            {
                this.scenPart = scenPart;
            }

            public IEnumerable<Def> GetAvailableDefs()
            {
                return DefDatabase<IncidentDef>.AllDefs;
            }

            public string GetCurrentDefLabel()
            {
                var incident = Cache.Incident.GetValue(scenPart) as IncidentDef;
                return incident?.LabelCap ?? "SelectIncident".Translate();
            }

            public void OnDefSelected(Def selectedDef)
            {
                Cache.Incident.SetValue(scenPart, selectedDef);
            }

            public Rect GetEditRect(Listing_ScenEdit listing)
            {
                return listing.GetScenPartRect(scenPart, ScenPart.RowHeight);
            }
        }

        private static bool Prefix(ScenPart_DisableIncident __instance, Rect rect)
        {
            var incidentSelector = new IncidentSelector(__instance);

            if (Widgets.ButtonText(rect, incidentSelector.GetCurrentDefLabel()))
            {
                Find.WindowStack.Add(new DefSelectionWindow(incidentSelector));
            }

            return false;
        }
    }
}