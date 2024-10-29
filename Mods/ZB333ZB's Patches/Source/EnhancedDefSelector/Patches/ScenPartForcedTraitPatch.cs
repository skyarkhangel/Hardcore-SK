using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using EnhancedDefSelector.Core;
using EnhancedDefSelector.Windows;
using System.Linq;

namespace EnhancedDefSelector.Patches
{
    [HarmonyPatch(typeof(ScenPart_ForcedTrait), "DoEditInterface")]
    public class ScenPartForcedTraitPatch
    {
        private static class Cache
        {
            public static readonly FieldInfo TraitField = AccessTools.Field(
                typeof(ScenPart_ForcedTrait),
                "trait");

            public static readonly FieldInfo DegreeField = AccessTools.Field(
                typeof(ScenPart_ForcedTrait),
                "degree");

            public static readonly MethodInfo DoPawnModifierEditInterface = AccessTools.Method(
                typeof(ScenPart_PawnModifier),
                "DoPawnModifierEditInterface");
        }

        private class TraitSelector : IScenarioDefPatch
        {
            private readonly ScenPart_ForcedTrait scenPart;

            public TraitSelector(ScenPart_ForcedTrait scenPart)
            {
                this.scenPart = scenPart;
            }

            public IEnumerable<Def> GetAvailableDefs()
            {
                return from trait in DefDatabase<TraitDef>.AllDefs
                       from degree in trait.degreeDatas
                       select new TraitDefWrapper(trait, degree.degree);
            }

            public string GetCurrentDefLabel()
            {
                var trait = Cache.TraitField.GetValue(scenPart) as TraitDef;
                var degree = (int)Cache.DegreeField.GetValue(scenPart);

                return trait?.DataAtDegree(degree).LabelCap ?? "SelectTrait".Translate();
            }

            public void OnDefSelected(Def selectedDef)
            {
                var wrapper = selectedDef as TraitDefWrapper;
                Cache.TraitField.SetValue(scenPart, wrapper.TraitDef);
                Cache.DegreeField.SetValue(scenPart, wrapper.Degree);
            }

            public Rect GetEditRect(Listing_ScenEdit listing)
            {
                return listing.GetScenPartRect(scenPart, ScenPart.RowHeight * 3f)
                    .TopPart(0.333f);
            }
        }

        private class TraitDefWrapper : Def
        {
            public TraitDef TraitDef { get; }
            public int Degree { get; }

            public TraitDefWrapper(TraitDef traitDef, int degree)
            {
                TraitDef = traitDef;
                Degree = degree;
                defName = $"{traitDef.defName}_{degree}";
                label = traitDef.DataAtDegree(degree).label;
            }

            public override TaggedString LabelCap
            {
                get
                {
                    return new TaggedString(label.CapitalizeFirst());
                }
            }
        }

        private static bool Prefix(ScenPart_ForcedTrait __instance, Listing_ScenEdit listing)
        {

            var rect = listing.GetScenPartRect(__instance, ScenPart.RowHeight * 3f);
            var traitSelector = new TraitSelector(__instance);

            if (Widgets.ButtonText(rect.TopPart(0.333f), traitSelector.GetCurrentDefLabel()))
            {
                Find.WindowStack.Add(new DefSelectionWindow(traitSelector));
            }

            Cache.DoPawnModifierEditInterface.Invoke(__instance,
                new object[] { rect.BottomPart(0.666f) });

            return false;
        }
    }
}