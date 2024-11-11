using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace StorageSelector.Integration.WhatsMissing
{
    public static class WMRequirements
    {
        private static readonly IWhatsMissingSettings Settings = new WhatsMissingSettingsImpl();

        private static string MakeColor(int needed, int got)
        {
            return "<color=#" + (
                (got < 1) ? "F4003D" :
                (got < needed) ? "FFA400" :
                (got < 2 * needed) ? "BCF994" :
                "97B7EF"
            ) + ">";
        }

        public static void RenderRequirements(Bill_Production bill, Listing_Standard listing, Rect rect)
        {
            Map currentMap = Find.CurrentMap;
            ResourceCounter resourceCounter = currentMap.resourceCounter;
            List<Pawn> colonists = currentMap.mapPawns.FreeColonists.ToList();
            RecipeDef recipe = bill.recipe;

            string description = recipe.description;
            if (!string.IsNullOrWhiteSpace(description))
            {
                listing.Label(description + "\n");
            }

            listing.Label("WhatsMissing.Requires".Translate());
            listing.Label($"{recipe.WorkAmountTotal(null):0} " + "WhatsMissing.WorkAmount".Translate());

            var ingredientValueGetter = recipe.IngredientValueGetter;
            var ingredients = recipe.ingredients;
            bool isNutrition = ingredientValueGetter is IngredientValueGetter_Nutrition;
            bool isVolume = ingredientValueGetter is IngredientValueGetter_Volume;
            bool isSpecial = isNutrition || isVolume;

            foreach (var ingredient in ingredients)
            {
                string summary = ingredient.filter.Summary;
                if (string.IsNullOrEmpty(summary))
                    continue;

                string text = ingredientValueGetter.BillRequirementsDescription(recipe, ingredient);
                if (!isSpecial)
                {
                    listing.Label(text);
                    continue;
                }

                var ingredientsByCount = new Dictionary<int, List<(ThingDef def, int count)>>();
                foreach (var allowedDef in ingredient.filter.AllowedThingDefs)
                {
                    int required = ingredient.CountRequiredOfFor(allowedDef, recipe, null);
                    if (required > 0)
                    {
                        if (!ingredientsByCount.TryGetValue(required, out var list))
                        {
                            list = new List<(ThingDef, int)>();
                            ingredientsByCount[required] = list;
                        }
                        list.Add((allowedDef, resourceCounter.GetCount(allowedDef)));
                    }
                }

                if (ingredientsByCount.Count == 0)
                {
                    listing.Label(text);
                    continue;
                }

                var tooltip = new StringBuilder();
                tooltip.AppendLine(text);
                tooltip.AppendLine("\n" + "WhatsMissing.HaveNeeded".Translate());
                if (recipe.allowMixingIngredients)
                {
                    tooltip.AppendLine("WhatsMissing.MixingPossible".Translate());
                }

                var notAllowedTooltip = new StringBuilder();
                var labels = new List<string>();
                ThingDef lastDef = null;

                foreach (var (needed, items) in ingredientsByCount.OrderBy(kv => kv.Key))
                {
                    var allowedItems = items.Where(i => IsAllowedIngredient(i.def, ingredient, bill));
                    var notAllowedItems = items.Where(i => !IsAllowedIngredient(i.def, ingredient, bill));

                    foreach (var group in allowedItems.GroupBy(i => i.count).OrderByDescending(g => g.Key))
                    {
                        if (Settings.HideZeroCountIngredients && group.Key == 0)
                            continue;

                        var itemLabels = group.Select(i => i.def.label).OrderBy(l => l);
                        tooltip.AppendLine($"{MakeColor(needed, group.Key)}{group.Key} / {needed}</color> {string.Join("; ", itemLabels)}");
                    }

                    foreach (var group in notAllowedItems.GroupBy(i => i.count).OrderByDescending(g => g.Key))
                    {
                        if (Settings.HideZeroCountIngredients && group.Key == 0)
                            continue;

                        var itemLabels = group.Select(i => i.def.label).OrderBy(l => l);
                        notAllowedTooltip.AppendLine($"{MakeColor(needed, group.Key)}{group.Key} / {needed}</color> {string.Join("; ", itemLabels)}");
                    }

                    int got = 0;
                    var availableCounts = allowedItems.Select(i => i.count);
                    if (recipe.allowMixingIngredients && availableCounts.Any())
                    {
                        got = availableCounts.Sum();
                    }
                    else if (availableCounts.Any())
                    {
                        got = availableCounts.Max();
                    }

                    labels.Add($"{MakeColor(needed, got)}{needed}</color>");
                    lastDef = items[items.Count - 1].def;
                }

                if (notAllowedTooltip.Length > 0)
                {
                    tooltip.AppendLine("\n" + "WhatsMissing.NotAllowed".Translate());
                    tooltip.AppendLine(notAllowedTooltip.ToString());
                }

                var labelRect = listing.GetRect(24f);
                var finalText = string.Join(" | ", labels) + " " + (isNutrition ?
                    ("WhatsMissing.Nutrition".Translate() + " (" + summary + ")") :
                    summary);

                if (Widgets.ButtonInvisible(labelRect))
                {
                    Find.WindowStack.Add(new Dialog_InfoCard(lastDef));
                }

                GUI.color = Color.white;
                Widgets.Label(labelRect, finalText);

                if (Mouse.IsOver(labelRect))
                {
                    var tooltipRect = labelRect;
                    tooltipRect.width = Settings.MaxTooltipsWidth;
                    TooltipHandler.TipRegion(tooltipRect, tooltip.ToString());
                }

                listing.Gap(4f);
            }

            if (!recipe.skillRequirements.NullOrEmpty())
            {
                listing.Label("\n" + "MinimumSkills".Translate());

                foreach (var req in recipe.skillRequirements)
                {
                    var pawns = colonists
                        .Where(p => !p.skills.GetSkill(req.skill).TotallyDisabled)
                        .Select(p => (pawn: p, level: p.skills.GetSkill(req.skill).Level))
                        .GroupBy(x => x.level)
                        .OrderByDescending(g => g.Key)
                        .Select(g => (level: g.Key, pawns: g.Select(x => x.pawn).OrderBy(p => p.Name.ToStringShort).ToList()))
                        .ToList();

                    var skillTooltip = new StringBuilder();
                    skillTooltip.AppendLine(req.Summary);

                    foreach (var (level, pawnList) in pawns)
                    {
                        skillTooltip.AppendLine($"{MakeColor(req.minLevel, level)}{level} / {req.minLevel}</color> {string.Join("; ", pawnList.Select(p => p.Name.ToStringShort))}");
                    }

                    var skillRect = listing.GetRect(24f);
                    if (Widgets.ButtonInvisible(skillRect))
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(req.skill));
                    }

                    GUI.color = Color.white;
                    Widgets.Label(skillRect, $"{req.skill.LabelCap} {MakeColor(req.minLevel, pawns.FirstOrDefault().level)}{req.minLevel}</color>");

                    if (Mouse.IsOver(skillRect))
                    {
                        var tooltipRect = skillRect;
                        tooltipRect.width = Settings.MaxTooltipsWidth;
                        TooltipHandler.TipRegion(tooltipRect, skillTooltip.ToString());
                    }

                    listing.Gap(4f);
                }
            }

            if (!isVolume)
            {
                string extraDesc = ingredientValueGetter.ExtraDescriptionLine(recipe);
                if (!string.IsNullOrWhiteSpace(extraDesc))
                {
                    listing.Label(extraDesc);
                }
            }

        }

        private static bool IsAllowedIngredient(ThingDef def, IngredientCount ingredient, Bill_Production bill)
        {
            if (!ingredient.IsFixedIngredient || !ingredient.filter.Allows(def))
            {
                return bill.recipe.fixedIngredientFilter.Allows(def) && bill.ingredientFilter.Allows(def);
            }
            return true;
        }
    }
}
