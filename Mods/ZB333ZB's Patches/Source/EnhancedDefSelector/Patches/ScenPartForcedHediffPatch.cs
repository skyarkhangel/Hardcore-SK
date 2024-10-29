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
    [HarmonyPatch(typeof(ScenPart_ForcedHediff), "DoEditInterface")]
    public class ScenPartForcedHediffPatch
    {
        private static class Cache
        {
            public static readonly MethodInfo PossibleHediffs = AccessTools.Method(
                typeof(ScenPart_ForcedHediff),
                "PossibleHediffs");

            public static readonly FieldInfo Hediff = AccessTools.Field(
                typeof(ScenPart_ForcedHediff),
                "hediff");

            public static readonly FieldInfo SeverityRange = AccessTools.Field(
                typeof(ScenPart_ForcedHediff),
                "severityRange");

            public static readonly MethodInfo DoPawnModifierEditInterface = AccessTools.Method(
                typeof(ScenPart_PawnModifier),
                "DoPawnModifierEditInterface");
        }

        private class HediffSelector : IScenarioDefPatch
        {
            private readonly ScenPart_ForcedHediff scenPart;

            public HediffSelector(ScenPart_ForcedHediff scenPart)
            {
                this.scenPart = scenPart;
            }

            public IEnumerable<Def> GetAvailableDefs()
            {
                return (IEnumerable<HediffDef>)Cache.PossibleHediffs.Invoke(scenPart, null);
            }

            public string GetCurrentDefLabel()
            {
                var hediff = Cache.Hediff.GetValue(scenPart) as HediffDef;
                return hediff?.LabelCap ?? "SelectHediff".Translate();
            }

            public void OnDefSelected(Def selectedDef)
            {
                var hediff = selectedDef as HediffDef;
                Cache.Hediff.SetValue(scenPart, hediff);

                var severityRange = (FloatRange)Cache.SeverityRange.GetValue(scenPart);
                var maxSeverity = hediff.lethalSeverity <= 0f ? 1f : hediff.lethalSeverity * 0.99f;

                if (severityRange.max > maxSeverity)
                    severityRange.max = maxSeverity;
                if (severityRange.min > maxSeverity)
                    severityRange.min = maxSeverity;

                Cache.SeverityRange.SetValue(scenPart, severityRange);
            }

            public Rect GetEditRect(Listing_ScenEdit listing)
            {
                return listing.GetScenPartRect(scenPart, ScenPart.RowHeight);
            }
        }

        private static bool Prefix(ScenPart_ForcedHediff __instance, Listing_ScenEdit listing)
        {
            var rect = listing.GetScenPartRect(__instance, ScenPart.RowHeight * 3f + 31f);

            var hediffSelector = new HediffSelector(__instance);
            if (Widgets.ButtonText(rect.TopPartPixels(ScenPart.RowHeight), hediffSelector.GetCurrentDefLabel()))
            {
                Find.WindowStack.Add(new DefSelectionWindow(hediffSelector));
            }

            var severityRect = new Rect(rect.x, rect.y + ScenPart.RowHeight, rect.width, 31f);
            var severityRange = (FloatRange)Cache.SeverityRange.GetValue(__instance);
            var hediff = (HediffDef)Cache.Hediff.GetValue(__instance);
            var maxSeverity = hediff.lethalSeverity <= 0f ? 1f : hediff.lethalSeverity * 0.99f;

            Widgets.FloatRange(severityRect, listing.CurHeight.GetHashCode(), ref severityRange, 0f, maxSeverity,
                "ConfigurableSeverity", ToStringStyle.FloatTwo);
            Cache.SeverityRange.SetValue(__instance, severityRange);

            Cache.DoPawnModifierEditInterface.Invoke(__instance,
                new object[] { rect.BottomPartPixels(ScenPart.RowHeight * 2f) });

            return false;
        }
    }
}