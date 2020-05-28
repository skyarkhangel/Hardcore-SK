using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using System.Xml.Linq;
using System.Linq;
using System.Text;

namespace SeedsPlease
{
    public class SeedDef : ThingDef
    {
        public SeedProperties seed;
        public ThingDef harvest;
        public List<ThingDef> sources = new List<ThingDef> ();

        [Unsaved]
        public new ThingDef plant;

        static float AssignMarketValueFromHarvest(ThingDef thingDef)
        {
            var harvestedThingDef = thingDef.plant.harvestedThingDef;
            if (harvestedThingDef == null)
            {
                return 0.5f;
            }

            float factor = thingDef.plant.harvestYield / thingDef.plant.growDays + thingDef.plant.growDays / thingDef.plant.harvestYield;
            float value = harvestedThingDef.BaseMarketValue * factor * 2.5f;

            if (thingDef.plant.blockAdjacentSow)
            {
                value *= 1.5f;
            }

            int cnt = thingDef.plant.wildBiomes?.Count() ?? 0;
            if (cnt > 1)
            {
                value *= Mathf.Pow(0.95f, cnt);
            }

            if (harvestedThingDef == ThingDefOf.WoodLog)
            {
                value *= 0.2f;
            }
            else if (harvestedThingDef.IsAddictiveDrug)
            {
                value *= 1.3f;
            }
            else if (harvestedThingDef.IsDrug)
            {
                value *= 1.2f;
            }
            else if (harvestedThingDef.IsMedicine)
            {
                value *= 1.1f;
            }

            value *= Mathf.Lerp(0.8f, 1.6f, thingDef.plant.sowMinSkill / 20f);

            if (value > 25f)
            {
                value = 24.99f;
            }

            return Mathf.Round(value * 100f) / 100f;
        }

        public override void ResolveReferences ()
        {
            base.ResolveReferences ();

            foreach (var p in sources)
            {
                if (p.plant == null)
                {
                    Log.Warning("SeedsPlease :: " + p.defName + " is not a plant.");
                    continue;
                }

                p.blueprintDef = this;

                if (plant == null && p.plant.Sowable)
                {
                    plant = p;
                }
            }

            if (plant == null) {
                Log.Warning("SeedsPlease :: " + defName + " has no sowable plant.");

                return;
            }

            if (plant.blueprintDef == null)
            {
                plant.blueprintDef = this;
            }

            if (harvest != null) {
                plant.plant.harvestedThingDef = harvest;
            } else {
                harvest = plant.plant.harvestedThingDef;
            }

            if (BaseMarketValue <= 0f && harvest != null) {
                BaseMarketValue = AssignMarketValueFromHarvest(plant);
            }

#if DEBUG
			Log.Message ($"{plant} {harvest?.BaseMarketValue} => {BaseMarketValue}");
#endif
        }

        public static bool AddMissingSeeds(StringBuilder report) {
            bool isAnyMissing = false;
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.ToList()) {
                if (thingDef.plant == null) {
                    continue;
                }
                if (thingDef.blueprintDef != null)
                {
                    continue;
                }
                if (!thingDef.plant.Sowable) {
                    continue;
                }
                if (thingDef.plant.harvestedThingDef == null)  {
                    continue;
                }
                isAnyMissing = true;
                AddMissingSeed(report, thingDef);
            }
            return isAnyMissing;
        }

        static void AddMissingSeed(StringBuilder report, ThingDef thingDef)
        {
            string name = thingDef.defName;
            foreach (string prefix in SeedsPleaseMod.knownPrefixes)
            {
                name = name.Replace(prefix, "");
            }
            name = name.CapitalizeFirst();

            report.AppendLine();
            report.Append("<!-- SeedsPlease :: ");
            report.Append(thingDef.defName);
            report.Append(" (");
            report.Append(thingDef.modContentPack.IsCoreMod ? "Patched" : thingDef.modContentPack.PackageId);
            report.Append(") ");

            SeedDef seed = DefDatabase<SeedDef>.GetNamedSilentFail("Seed_" + name);
            if (seed == null)
            {
                var template = ResourceBank.ThingDefOf.Seed_Psychoid;

                seed = new SeedDef()
                {
                    defName = "Seed_" + name,
                    label = name.ToLower() + " seeds",
                    plant = thingDef,
                    harvest = thingDef.plant.harvestedThingDef,
                    stackLimit = template.stackLimit,
                    seed = new SeedProperties() { harvestFactor = 1f, seedFactor = 1f, baseChance = 0.95f, extraChance = 0.15f },
                    tradeTags = template.tradeTags,
                    thingCategories = template.thingCategories,
                    soundDrop = template.soundDrop,
                    soundInteract = template.soundInteract,
                    statBases = template.statBases,
                    graphicData = template.graphicData,
                    description = template.description,
                    thingClass = template.thingClass,
                    pathCost = template.pathCost,
                    rotatable = template.rotatable,
                    drawGUIOverlay = template.drawGUIOverlay,
                    alwaysHaulable = template.alwaysHaulable,
                    comps = template.comps,
                    altitudeLayer = template.altitudeLayer,
                    selectable = template.selectable,
                    useHitPoints = template.useHitPoints,
                    resourceReadoutPriority = template.resourceReadoutPriority,
                    category = template.category,
                    uiIcon = template.uiIcon,
                    uiIconColor = template.uiIconColor,
                };

                seed.BaseMarketValue = AssignMarketValueFromHarvest(thingDef);

                foreach(var category in seed.thingCategories) {
                    category.childThingDefs.Add(seed);
                }

                DefDatabase<ThingDef>.Add(seed);
                DefDatabase<SeedDef>.Add(seed);

                seed.ResolveReferences();

                report.Append("Autogenerated as ");
            }
            else
            {
                seed.sources.Add(thingDef);

                report.Append("Inserted to ");
            }

            report.Append(seed.defName);
            report.AppendLine("-->");
            report.AppendLine();

            var seedXml =
            new XElement("SeedsPlease.SeedDef", new XAttribute("ParentName", "SeedBase"),
                         new XElement("defName", seed.defName),
                         new XElement("label", seed.label),
                         new XElement("sources",
                                      new XElement("li", thingDef.defName)));

            report.AppendLine(seedXml.ToString());

            if (thingDef.plant.harvestedThingDef.IsStuff) {
                return;
            }

            float yieldCount = Mathf.Max(Mathf.Round(thingDef.plant.harvestYield / 3f), 4f);

            RecipeDef recipe = DefDatabase<RecipeDef>.GetNamedSilentFail("ExtractSeed_" + name);

            if (recipe == null)
            {
                var ingredient = new IngredientCount();
                ingredient.filter.SetAllow(thingDef.plant.harvestedThingDef, true);
                ingredient.SetBaseCount(yieldCount);

                recipe = new RecipeDef()
                {
                    defName = "ExtractSeed_" + name,
                    label = "extract " + name.ToLower() + " seeds",
                    description = "Extract seeds from " + thingDef.plant.harvestedThingDef.defName.Replace("Raw", ""),
                    ingredients = new List<IngredientCount>() { ingredient },
                    defaultIngredientFilter = ingredient.filter,
                    fixedIngredientFilter = ingredient.filter,
                    products = new List<ThingDefCountClass>() {
                        new ThingDefCountClass() { thingDef = seed, count = 3 }
                    },
                    researchPrerequisite = thingDef.researchPrerequisites?.FirstOrFallback(),
                    workAmount = 600f,
                    workSkill = SkillDefOf.Cooking,
                    effectWorking = EffecterDefOf.Vomit,
                    workSpeedStat = StatDefOf.EatingSpeed,
                    jobString = "Extracting seeds.",
                };

                DefDatabase<RecipeDef>.Add(recipe);
                ResourceBank.ThingDefOf.PlantProcessingTable.recipes.Add(recipe);
            }
            else
            {
                recipe.ingredients?.First()?.filter.SetAllow(thingDef.plant.harvestedThingDef, true);
            }

            var recipeXml =
            new XElement("RecipeDef", new XAttribute("ParentName", "ExtractSeed"),
                         new XElement("defName", recipe.defName),
                         new XElement("label", recipe.label),
                         new XElement("description", recipe.description),
                         new XElement("ingredients",
                                      new XElement("li",
                                                   new XElement("filter",
                                                                new XElement("thingDefs",
                                                                             new XElement("li", thingDef.plant.harvestedThingDef.defName))),
                                                   new XElement("count", yieldCount))),
                         new XElement("fixedIngredientFilter",
                                      new XElement("thingDefs",
                                                   new XElement("li", thingDef.plant.harvestedThingDef.defName))),
                         new XElement("products",
                                      new XElement(seed.defName, 3)));

            report.AppendLine();
            report.AppendLine(recipeXml.ToString());
        }
    }
}