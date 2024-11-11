using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using EnhancedDefSelector.Core;
using EnhancedDefSelector.Windows;

namespace EnhancedDefSelector.Patches
{
    [HarmonyPatch(typeof(ScenPart_ThingCount), "DoEditInterface")]
    public class ScenPartThingCountPatch
    {
        private static class Cache
        {
            public static readonly MethodInfo PossibleThingDefs = AccessTools.Method(typeof(ScenPart_ThingCount), "PossibleThingDefs");
            public static readonly FieldInfo ThingDef = AccessTools.Field(typeof(ScenPart_ThingCount), "thingDef");
            public static readonly FieldInfo Stuff = AccessTools.Field(typeof(ScenPart_ThingCount), "stuff");
            public static readonly FieldInfo Quality = AccessTools.Field(typeof(ScenPart_ThingCount), "quality");
            public static readonly FieldInfo Count = AccessTools.Field(typeof(ScenPart_ThingCount), "count");
        }

        private const float TOTAL_ROWS = 4f;
        private const string DEFAULT_TRANSLATION_KEY = "Default";

        private class ThingDefSelector : IScenarioDefPatch
        {
            private readonly ScenPart_ThingCount scenPart;

            public ThingDefSelector(ScenPart_ThingCount scenPart)
            {
                this.scenPart = scenPart;
            }

            public IEnumerable<Def> GetAvailableDefs()
            {
                return (IEnumerable<ThingDef>)Cache.PossibleThingDefs.Invoke(scenPart, null);
            }

            public string GetCurrentDefLabel()
            {
                var thingDef = (ThingDef)Cache.ThingDef.GetValue(scenPart);
                return thingDef.LabelCap;
            }

            public void OnDefSelected(Def selectedDef)
            {
                var selectedThing = (ThingDef)selectedDef;
                Cache.ThingDef.SetValue(scenPart, selectedThing);
                Cache.Stuff.SetValue(scenPart, GenStuff.DefaultStuffFor(selectedThing));
                Cache.Quality.SetValue(scenPart, null);
            }

            public Rect GetEditRect(Listing_ScenEdit listing)
            {
                var baseRect = listing.GetScenPartRect(scenPart, ScenPart.RowHeight * TOTAL_ROWS);
                return new Rect(baseRect.x, baseRect.y, baseRect.width, baseRect.height / TOTAL_ROWS);
            }
        }

        private class StuffSelector : IScenarioDefPatch
        {
            private readonly ScenPart_ThingCount scenPart;

            public StuffSelector(ScenPart_ThingCount scenPart)
            {
                this.scenPart = scenPart;
            }

            public IEnumerable<Def> GetAvailableDefs()
            {
                var thingDef = (ThingDef)Cache.ThingDef.GetValue(scenPart);
                return GenStuff.AllowedStuffsFor(thingDef, TechLevel.Undefined).OrderBy(t => t.label);
            }

            public string GetCurrentDefLabel()
            {
                var stuff = (ThingDef)Cache.Stuff.GetValue(scenPart);
                return stuff.LabelCap;
            }

            public void OnDefSelected(Def selectedDef)
            {
                Cache.Stuff.SetValue(scenPart, selectedDef);
            }

            public Rect GetEditRect(Listing_ScenEdit listing)
            {
                var baseRect = listing.GetScenPartRect(scenPart, ScenPart.RowHeight * TOTAL_ROWS);
                return new Rect(baseRect.x, baseRect.y + baseRect.height / TOTAL_ROWS,
                    baseRect.width, baseRect.height / TOTAL_ROWS);
            }
        }

        private static bool Prefix(ScenPart_ThingCount __instance, Listing_ScenEdit listing, ref string ___countBuf)
        {
            var currentValues = GetCurrentValues(__instance);
            var (ThingRect, StuffRect, QualityRect, CountRect) = CalculateRects(listing.GetScenPartRect(__instance, ScenPart.RowHeight * TOTAL_ROWS));

            var thingSelector = new ThingDefSelector(__instance);
            if (Widgets.ButtonText(ThingRect, thingSelector.GetCurrentDefLabel()))
            {
                Find.WindowStack.Add(new DefSelectionWindow(thingSelector));
            }

            if (currentValues.ThingDef.MadeFromStuff)
            {
                var stuffSelector = new StuffSelector(__instance);
                if (Widgets.ButtonText(StuffRect, stuffSelector.GetCurrentDefLabel()))
                {
                    Find.WindowStack.Add(new DefSelectionWindow(stuffSelector));
                }
            }

            if (currentValues.ThingDef.HasComp(typeof(CompQuality)))
            {
                HandleQualitySelection(__instance, currentValues, QualityRect);
            }

            HandleCountInput(__instance, currentValues, CountRect, ref ___countBuf);

            return false;
        }

        private static (ThingDef ThingDef, ThingDef Stuff, QualityCategory? Quality, int Count) GetCurrentValues(ScenPart_ThingCount instance)
        {
            return (
                (ThingDef)Cache.ThingDef.GetValue(instance),
                (ThingDef)Cache.Stuff.GetValue(instance),
                (QualityCategory?)Cache.Quality.GetValue(instance),
                (int)Cache.Count.GetValue(instance)
            );
        }

        private static (Rect ThingRect, Rect StuffRect, Rect QualityRect, Rect CountRect) CalculateRects(Rect baseRect)
        {
            float rowHeight = baseRect.height / TOTAL_ROWS;
            return (
                new Rect(baseRect.x, baseRect.y, baseRect.width, rowHeight),
                new Rect(baseRect.x, baseRect.y + rowHeight, baseRect.width, rowHeight),
                new Rect(baseRect.x, baseRect.y + rowHeight * 2f, baseRect.width, rowHeight),
                new Rect(baseRect.x, baseRect.y + rowHeight * 3f, baseRect.width, rowHeight)
            );
        }

        private static void HandleQualitySelection(ScenPart_ThingCount instance,
            (ThingDef ThingDef, ThingDef Stuff, QualityCategory? Quality, int Count) current, Rect rect)
        {
            string qualityLabel = current.Quality.HasValue
                ? current.Quality.Value.GetLabel().CapitalizeFirst()
                : DEFAULT_TRANSLATION_KEY.Translate().ToString().CapitalizeFirst();

            if (!Widgets.ButtonText(rect, qualityLabel)) return;

            var qualityOptions = new List<FloatMenuOption>
            {
                new(
                    DEFAULT_TRANSLATION_KEY.Translate().ToString().CapitalizeFirst(),
                    () => Cache.Quality.SetValue(instance, null))
            };

            qualityOptions.AddRange(QualityUtility.AllQualityCategories.Select(qc =>
                new FloatMenuOption(
                    qc.GetLabel().CapitalizeFirst(),
                    () => Cache.Quality.SetValue(instance, qc))));

            Find.WindowStack.Add(new FloatMenu(qualityOptions));
        }

        private static void HandleCountInput(ScenPart_ThingCount instance,
            (ThingDef ThingDef, ThingDef Stuff, QualityCategory? Quality, int Count) current,
            Rect rect, ref string countBuf)
        {
            int count = current.Count;
            Widgets.TextFieldNumeric(rect, ref count, ref countBuf, 1f, 1E+09f);
            Cache.Count.SetValue(instance, count);
        }
    }
}