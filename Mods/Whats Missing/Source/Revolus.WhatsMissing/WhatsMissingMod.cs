using HarmonyLib;
using static HarmonyLib.AccessTools;
using Verse;
using RimWorld;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Linq;

namespace Revolus.WhatsMissing {
    public class WhatsMissingMod : Mod {
        public WhatsMissingMod(ModContentPack content) : base(content) {
            var harmony = new Harmony(nameof(WhatsMissingMod));
            harmony.Patch(
                Method(typeof(Dialog_BillConfig), nameof(Dialog_BillConfig.DoWindowContents), new Type[] { typeof(Rect) }),
                transpiler: new HarmonyMethod(typeof(WhatsMissingMod), nameof(Patch__Dialog_BillConfig__DoWindowContents__Transpiler))
            );
        }

        private static string MakeColor(int needed, int got) => $"<color=#{(got < 1 ? "F4003D" : got < needed ? "FFA400" : got < 2 * needed ? "BCF994" : "97B7EF")}>";

        private static IEnumerable<CodeInstruction> Patch__Dialog_BillConfig__DoWindowContents__Transpiler(IEnumerable<CodeInstruction> instructions) {
            var done = false;

            string rememberLoc = null;
            LocalVariableInfo listingLoc = null;
            LocalVariableInfo stringBuilderLoc = null;
            var rectLocs = new List<LocalVariableInfo>();
            var stringBuilderLoaded = false;

            // Trap and replace StringBuilder.ToString() with a call to my method, and a return statement.
            // The output would be printed to the default listing.
            // The last instantiated Standard_Listing is the output listing.
            // The first three populated Rects are the relevant Rects for placement inside the window.
            foreach (var instruction in instructions) {
                if (!done) {
                    var opcode = instruction.opcode;
                    var operand = instruction.operand;

                    if (rememberLoc != null) {
                        if (opcode == OpCodes.Stloc_S && operand is LocalVariableInfo loc) {
                            if (rememberLoc == nameof(Listing_Standard)) {
                                listingLoc = loc;
                            } else if (rememberLoc == nameof(StringBuilder)) {
                                stringBuilderLoc = loc;
                            }
                        }
                        rememberLoc = null;
                    } else if (opcode == OpCodes.Ldloca_S) {
                        if (operand is LocalVariableInfo loc && loc.LocalType == typeof(Rect)) {
                            if (rectLocs.All(l => l.LocalIndex != loc.LocalIndex)) {
                                rectLocs.Add(loc);
                            }
                        }
                    } else if (opcode == OpCodes.Newobj) {
                        if (operand is MethodBase methodInfo) {
                            var declaringType = methodInfo.DeclaringType;
                            if (declaringType == typeof(Listing_Standard)) {
                                rememberLoc = nameof(Listing_Standard);
                            } else if (declaringType == typeof(StringBuilder)) {
                                rememberLoc = nameof(StringBuilder);
                            }
                        }
                    } else if (stringBuilderLoc != null) {
                        if (stringBuilderLoaded) {
                            stringBuilderLoaded = false;
                            if (opcode == OpCodes.Callvirt && Method(typeof(object), "ToString").Equals(operand)) {
                                // pop StringBuilder instance from stack
                                yield return new CodeInstruction(OpCodes.Pop);
                                // argument bill
                                yield return new CodeInstruction(OpCodes.Ldarg_0);
                                yield return new CodeInstruction(OpCodes.Ldfld, Field(typeof(Dialog_BillConfig), "bill"));
                                // argument listing
                                yield return new CodeInstruction(OpCodes.Ldloc_S, listingLoc);
                                // argument rect
                                yield return new CodeInstruction(OpCodes.Ldloc_S, rectLocs[0]);
                                // argument rect3
                                yield return new CodeInstruction(OpCodes.Ldloc_S, rectLocs[2]);
                                // call Patch__Dialog_BillConfig__DoWindowContents__Mixin
                                yield return new CodeInstruction(OpCodes.Call, Method(typeof(WhatsMissingMod), nameof(Patch__Dialog_BillConfig__DoWindowContents__Mixin)));
                                // return
                                yield return new CodeInstruction(OpCodes.Ret);
                                
                                // Done. Need to emit further opcodes anyway, in order not to break jump labels.
                                Log.Message("[WhatsMissing] Patched Dialog_BillConfig");
                                done = true;

                            }
                        } else if (opcode == OpCodes.Ldloc_S && operand is LocalVariableInfo varInfo && varInfo.LocalIndex == stringBuilderLoc.LocalIndex) {
                            stringBuilderLoaded = true;
                        }
                    }
                }
                
                yield return instruction;
            }
        }

        public static void Patch__Dialog_BillConfig__DoWindowContents__Mixin(Bill_Production bill, Listing_Standard listing, Rect rect, Rect rect3) {
            var currentMap = Find.CurrentMap;
            var resourceCounter = currentMap.resourceCounter;
            var colonists = currentMap.mapPawns.FreeColonists.ToList();
            
            var recipe = bill.recipe;
            try {
                var description = recipe.description;
                if (!string.IsNullOrWhiteSpace(description)) {
                    listing.Label($"{description}\n");
                }

                listing.Label("Requires (see tooltips, ingredients can be clicked):");
                listing.Label($"{recipe.WorkAmountTotal(null):0} work");

                var ingrValueGetter = recipe.IngredientValueGetter;
                var ingredients = recipe.ingredients;
                var isNutrition = ingrValueGetter is IngredientValueGetter_Nutrition;
                var isVolume = ingrValueGetter is IngredientValueGetter_Volume;
                var defaultValueFormatter = isNutrition || isVolume;
                for (int ingrIndex = 0, ingrCount = ingredients.Count; ingrIndex < ingrCount; ++ingrIndex) {
                    var ingrAndCount = ingredients[ingrIndex];
                    
                    var summary = ingrAndCount.filter.Summary;
                    if (string.IsNullOrEmpty(summary)) {
                        continue;
                    }
                    
                    var descr = ingrValueGetter.BillRequirementsDescription(recipe, ingrAndCount);
                    if (!defaultValueFormatter) {
                        listing.Label(descr);
                        continue;
                    }

                    var neededCountDict = new Dictionary<int, List<(ThingDef td, int count)>>();
                    foreach (var td in ingrAndCount.filter.AllowedThingDefs) {
                        var tdNeeded = ingrAndCount.CountRequiredOfFor(td, recipe);
                        if (tdNeeded <= 0) {
                            // impossible
                            continue;
                        }
                        if (!neededCountDict.TryGetValue(tdNeeded, out var neededList)) {
                            neededList = new List<(ThingDef, int)>();
                            neededCountDict.Add(tdNeeded, neededList);
                        }
                        neededList.Add((td, resourceCounter.GetCount(td)));
                    }

                    if (neededCountDict.Count == 0) {
                        // impossible
                        listing.Label(descr);
                        continue;
                    }
                    
                    var tooltip = new StringBuilder();
                    tooltip.AppendLine(descr);
                    tooltip.AppendLine("\nYou have \u2044 needed:");
                    if (recipe.allowMixingIngredients) {
                        tooltip.AppendLine("(mixing ingredients is possible)");
                    }

                    ThingDef lastTd = null;
                    var tdCount = 0;
                    var labelList = new List<string>();
                    foreach (var (needed, list) in neededCountDict.Select(kv => (needed: kv.Key, list: kv.Value)).OrderBy(i => i.needed)) {
                        tooltip.AppendLine();
                        foreach (var gotGroup in list.GroupBy(i => i.count).OrderBy(i => -i.Key)) {
                            var names = gotGroup.Select(i => i.td.label).ToList();
                            names.Sort(StringComparer.InvariantCultureIgnoreCase);
                            tooltip.AppendLine($"{MakeColor(needed, gotGroup.Key)}{gotGroup.Key} \u2044 {needed}</color> {string.Join("; ", names)}");
                        }

                        var got = recipe.allowMixingIngredients ? list.Select(i => i.count).Sum() : list.Select(i => i.count).Max();
                        var color = MakeColor(needed, got);
                        labelList.Add($"{MakeColor(needed, got)}{needed}</color>");

                        tdCount += list.Count;
                        lastTd = list[list.Count - 1].td;
                    }
                    if (tdCount == 0) {
                        // impossible
                        continue;
                    }

                    var labelRect = listing.Label(
                        $"{string.Join(" | ", labelList)} {(isNutrition ? $"nutrition ({summary})" : summary)}",
                        tooltip: tooltip.ToString()
                    );
                    if (Widgets.ButtonInvisible(labelRect)) {
                        Find.WindowStack.Add(new Dialog_InfoCard(lastTd));
                    }
                }

                var colonistSkillsDict = new Dictionary<string, List<(int s, List<Pawn> p)>>();
                if (recipe.skillRequirements is List<SkillRequirement> skillReqs) {
                    // listing.Label($"{"MinimumSkills".Translate()} {recipe.MinSkillString}");
                    for (int i = 0, l = skillReqs.Count; i < l; ++i) {
                        var skillReq = skillReqs[i];
                        var skill = skillReq.skill;
                        var minLevel = skillReq.minLevel;
                        if (!colonistSkillsDict.TryGetValue(skill.defName, out var colonistSkills)) {
                            colonistSkills = (
                                colonists.
                                Select(col => (c: col, s: col.skills.GetSkill(skill))).
                                Where(cs => !cs.s.TotallyDisabled).
                                Select(cs => (cs.c, s: cs.s.levelInt)).
                                GroupBy(cs => cs.s).
                                Select(g => (
                                    s: g.Key,
                                    p: g.AsEnumerable().Select(cs => cs.c).OrderBy(p => p.Name.ToStringShort).ToList()
                                )).
                                OrderBy(ps => -ps.s).
                                ToList()
                            );
                            colonistSkillsDict.Add(skill.defName, colonistSkills);
                        }

                        Rect labelRect;
                        if (colonistSkills.NullOrEmpty()) {
                            // no colonists in map??
                            labelRect = listing.Label($"{skill.LabelCap} {minLevel}");
                        } else {
                            var tooltip = new StringBuilder();
                            tooltip.AppendLine(skillReq.Summary);
                            foreach ((var skillLevel, var pawns) in colonistSkills) {
                                tooltip.AppendLine(
                                    $"{MakeColor(minLevel, skillLevel)}{skillLevel} \u2044 {minLevel}</color> " +
                                    string.Join("; ", pawns.Select(p => p.Name.ToStringShort))
                                );
                            }
                            labelRect = listing.Label($"{skill.LabelCap} {MakeColor(minLevel, colonistSkills[0].s)}{minLevel}</color>", tooltip: tooltip.ToString());
                        }
                        if (Widgets.ButtonInvisible(labelRect)) {
                            Find.WindowStack.Add(new Dialog_InfoCard(skill));
                        }
                    }
                }

                if (!isVolume) {
                    var extraLine = ingrValueGetter.ExtraDescriptionLine(recipe);
                    if (!string.IsNullOrWhiteSpace(extraLine)) {
                        listing.Label(extraLine);
                    }
                }
            } finally {
                listing.End();
            }

            var products = recipe.products;
            if (products.Count == 1) {
                ThingDef thingDef = products[0].thingDef;
                Widgets.InfoCardButton(rect.x, rect3.y, thingDef, GenStuff.DefaultStuffFor(thingDef));
            }
        }
    }
}
