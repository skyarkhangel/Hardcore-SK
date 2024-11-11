using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Linq;
using EnhancedDefSelector.Core;
using EnhancedDefSelector.Windows;

namespace EnhancedDefSelector.Patches
{
    [HarmonyPatch(typeof(ScenPart_StartingAnimal), "DoEditInterface")]
    public class ScenPartStartingAnimalPatch
    {
        private static class Cache
        {
            public static readonly MethodInfo PossibleAnimals = AccessTools.Method(
                typeof(ScenPart_StartingAnimal),
                "PossibleAnimals");

            public static readonly FieldInfo AnimalKind = AccessTools.Field(
                typeof(ScenPart_StartingAnimal),
                "animalKind");

            public static readonly FieldInfo Count = AccessTools.Field(
                typeof(ScenPart_StartingAnimal),
                "count");

            public static readonly FieldInfo CountBuf = AccessTools.Field(
                typeof(ScenPart_StartingAnimal),
                "countBuf");
        }

        private class AnimalSelector : IScenarioDefPatch
        {
            private readonly ScenPart_StartingAnimal scenPart;

            public AnimalSelector(ScenPart_StartingAnimal scenPart)
            {
                this.scenPart = scenPart;
            }

            public IEnumerable<Def> GetAvailableDefs()
            {
                var possibleAnimals = (IEnumerable<PawnKindDef>)Cache.PossibleAnimals
                    .Invoke(scenPart, new object[] { false });

                var result = new List<Def>
                {
                    new DefPlaceholder("RandomPet".TranslateSimple())
                };
                result.AddRange(possibleAnimals.Cast<Def>());
                return result;
            }

            public string GetCurrentDefLabel()
            {
                var animalKind = Cache.AnimalKind.GetValue(scenPart) as PawnKindDef;
                return animalKind?.LabelCap ?? "RandomPet".TranslateSimple().CapitalizeFirst();
            }

            public void OnDefSelected(Def selectedDef)
            {
                if (selectedDef is DefPlaceholder)
                {
                    Cache.AnimalKind.SetValue(scenPart, null);
                }
                else
                {
                    Cache.AnimalKind.SetValue(scenPart, selectedDef);
                }
            }

            public Rect GetEditRect(Listing_ScenEdit listing)
            {
                return listing.GetScenPartRect(scenPart, ScenPart.RowHeight * 2f).BottomHalf();
            }
        }

        private static bool Prefix(ScenPart_StartingAnimal __instance, Listing_ScenEdit listing)
        {
            var rect = listing.GetScenPartRect(__instance, ScenPart.RowHeight * 2f);

            Listing_Standard listingStandard = new();
            listingStandard.Begin(rect.TopHalf());
            listingStandard.ColumnWidth = rect.width;

            int count = (int)Cache.Count.GetValue(__instance);
            string countBuf = (string)Cache.CountBuf.GetValue(__instance);

            listingStandard.TextFieldNumeric(ref count, ref countBuf, 1f, 1E+09f);

            Cache.Count.SetValue(__instance, count);
            Cache.CountBuf.SetValue(__instance, countBuf);

            listingStandard.End();

            var animalSelector = new AnimalSelector(__instance);
            if (Widgets.ButtonText(rect.BottomHalf(), animalSelector.GetCurrentDefLabel()))
            {
                Find.WindowStack.Add(new DefSelectionWindow(animalSelector));
            }

            return false;
        }
    }

        public class DefPlaceholder : Def
    {
        public DefPlaceholder(string label)
        {
            this.label = label;
            defName = label;
        }
    }
}
