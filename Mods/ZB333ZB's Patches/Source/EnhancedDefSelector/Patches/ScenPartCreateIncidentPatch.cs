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
    public class ScenPartCreateIncidentPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("RimWorld.ScenPart_CreateIncident"), "DoEditInterface");
        }

        private static class Cache
        {
            public static readonly MethodInfo RandomizableIncidents = AccessTools.Method(
                AccessTools.TypeByName("RimWorld.ScenPart_CreateIncident"),
                "RandomizableIncidents");

            public static readonly FieldInfo Incident = AccessTools.Field(
                typeof(ScenPart_IncidentBase),
                "incident");

            public static readonly FieldInfo IntervalDays = AccessTools.Field(
                AccessTools.TypeByName("RimWorld.ScenPart_CreateIncident"),
                "intervalDays");

            public static readonly FieldInfo IntervalDaysBuffer = AccessTools.Field(
                AccessTools.TypeByName("RimWorld.ScenPart_CreateIncident"),
                "intervalDaysBuffer");

            public static readonly FieldInfo Repeat = AccessTools.Field(
                AccessTools.TypeByName("RimWorld.ScenPart_CreateIncident"),
                "repeat");
        }

        private class IncidentSelector : IScenarioDefPatch
        {
            private readonly object scenPart;

            public IncidentSelector(object scenPart)
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
                var baseRect = listing.GetScenPartRect(scenPart as ScenPart, ScenPart.RowHeight * 3f);
                return new Rect(baseRect.x, baseRect.y, baseRect.width, baseRect.height / 3f);
            }
        }

        private static bool Prefix(object __instance, Listing_ScenEdit listing)
        {
            var incidentSelector = new IncidentSelector(__instance);

            if (Widgets.ButtonText(incidentSelector.GetEditRect(listing), incidentSelector.GetCurrentDefLabel()))
            {
                Find.WindowStack.Add(new DefSelectionWindow(incidentSelector));
            }

            var rect = listing.GetScenPartRect(__instance as ScenPart, ScenPart.RowHeight * 3f);
            var rect1 = new Rect(rect.x, rect.y + rect.height / 3f, rect.width, rect.height / 3f);
            var rect2 = new Rect(rect.x, rect.y + rect.height * 2f / 3f, rect.width, rect.height / 3f);

            float intervalDays = (float)Cache.IntervalDays.GetValue(__instance);
            string intervalDaysBuffer = (string)Cache.IntervalDaysBuffer.GetValue(__instance);
            bool repeat = (bool)Cache.Repeat.GetValue(__instance);

            Widgets.TextFieldNumericLabeled(rect1, "intervalDays".Translate(), ref intervalDays, ref intervalDaysBuffer, 0f, 1E+09f);
            Widgets.CheckboxLabeled(rect2, "repeat".Translate(), ref repeat);

            Cache.IntervalDays.SetValue(__instance, intervalDays);
            Cache.IntervalDaysBuffer.SetValue(__instance, intervalDaysBuffer);
            Cache.Repeat.SetValue(__instance, repeat);

            return false;
        }
    }
}
