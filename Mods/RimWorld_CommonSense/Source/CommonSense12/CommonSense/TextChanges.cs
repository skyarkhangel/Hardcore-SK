using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

namespace CommonSense
{
    class TextChanges
    {
        [HarmonyPatch(typeof(ThingFilter), "SetAllowAllWhoCanMake")]
        public class ThingFilter_SetAllowAllWhoCanMake_CommonSensePatch
        {
            static bool Prefix(ThingFilter __instance, ThingDef thing)
            {
                List<ThingDef> allowAllWhoCanMake = Traverse.Create(__instance).Field("allowAllWhoCanMake").GetValue<List<ThingDef>>();
                if (allowAllWhoCanMake == null)
                {
                    allowAllWhoCanMake = new List<ThingDef>();
                    Traverse.Create(__instance).Field("allowAllWhoCanMake").SetValue(allowAllWhoCanMake);
                    allowAllWhoCanMake.Add(thing);
                }
                return true;
            }
        }

        static string ShortCategory(ThingCategoryDef tcDef)
        {
            if (tcDef.parent == null)
                return "NoCategory".Translate().CapitalizeFirst();
            else
                return tcDef.label.CapitalizeFirst();
        }

        static string GetCategoryPath(ThingCategoryDef tcDef)
        {
            if (tcDef.parent == null)
                return "NoCategory".Translate().CapitalizeFirst();
            else
            {
                string s = tcDef.label.CapitalizeFirst();
                ThingCategoryDef def = tcDef.parent;

                while (def.parent != null)
                {
                    s += " \\ " + def.label.CapitalizeFirst();
                    def = def.parent;
                }
                return s;
            }
        }

        public static StatDrawEntry CategoryEntry(ThingCategoryDef tcDef)
        {

            return new StatDrawEntry(StatCategoryDefOf.Basics, "Category".Translate(), GetCategoryPath(tcDef), ShortCategory(tcDef), 1000);
        }

        static IEnumerable<StatDrawEntry> CategoryEntryRow(Thing thing)
        {
            ThingCategoryDef d;
            if (thing == null || thing.def == null || (d = thing.def.FirstThingCategory) == null)
                yield break;
            
            yield return CategoryEntry(d);
        }

        //public static void DrawStatsReport(Rect rect, Thing thing)
        [HarmonyPatch(typeof(StatsReportUtility), "DrawStatsReport", new Type[] { typeof(Rect), typeof(Thing) })]
        static class StatsReportUtility_DrawStatsReport_CommonSensePatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase mb)
            {
                FieldInfo LcachedDrawEntries = AccessTools.Field(typeof(StatsReportUtility), "cachedDrawEntries");
                MethodInfo LAddRange = AccessTools.Method(typeof(List<StatDrawEntry>), "AddRange");

                bool b = false;
                foreach (var i in (instructions))
                {
                    yield return i;
                    if (!b && i.opcode == OpCodes.Brfalse)
                    {
                        b = true;
                        yield return new CodeInstruction(OpCodes.Ldsfld, LcachedDrawEntries);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TextChanges), "CategoryEntryRow"));
                        yield return new CodeInstruction(OpCodes.Callvirt, LAddRange);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ThingFilter), nameof(ThingFilter.Summary), MethodType.Getter)]
        public class ThingFilter_Summary_CommonSensePatch
        {
            static bool Prefix(ThingFilter __instance, ref string __result)
            {
                if (!Settings.gui_extended_recipe)
                    return true;

                if (!__instance.customSummary.NullOrEmpty())
                {
                    __result = __instance.customSummary;
                }

                List<ThingDef> thingDefs = Traverse.Create(__instance).Field("thingDefs").GetValue<List<ThingDef>>();
                List<string> categories = Traverse.Create(__instance).Field("categories").GetValue<List<string>>();
                //List<string> tradeTagsToAllow = Traverse.Create(__instance).Field("tradeTagsToAllow").GetValue<List<string>>();
                //List<string> tradeTagsToDisallow = Traverse.Create(__instance).Field("tradeTagsToDisallow").GetValue<List<string>>();
                //List<string> thingSetMakerTagsToAllow = Traverse.Create(__instance).Field("thingSetMakerTagsToAllow").GetValue<List<string>>();
                //List<string> thingSetMakerTagsToDisallow = Traverse.Create(__instance).Field("thingSetMakerTagsToDisallow").GetValue<List<string>>();
                //List<string> disallowedCategories = Traverse.Create(__instance).Field("disallowedCategories").GetValue<List<string>>();
                //List<string> specialFiltersToAllow = Traverse.Create(__instance).Field("specialFiltersToAllow").GetValue<List<string>>();
                //List<string> specialFiltersToDisallow = Traverse.Create(__instance).Field("specialFiltersToDisallow").GetValue<List<string>>();
                //List<StuffCategoryDef> stuffCategoriesToAllow = Traverse.Create(__instance).Field("stuffCategoriesToAllow").GetValue<List<StuffCategoryDef>>();
                List<ThingDef> allowAllWhoCanMake = Traverse.Create(__instance).Field("allowAllWhoCanMake").GetValue<List<ThingDef>>();
                //FoodPreferability disallowWorsePreferability = Traverse.Create(__instance).Field("disallowWorsePreferability").GetValue<FoodPreferability>();
                //bool disallowInedibleByHuman = Traverse.Create(__instance).Field("disallowInedibleByHuman").GetValue<bool>();
                //Type allowWithComp = Traverse.Create(__instance).Field("allowWithComp").GetValue<Type>();
                //Type disallowWithComp = Traverse.Create(__instance).Field("disallowWithComp").GetValue<Type>();
                //float disallowCheaperThan = Traverse.Create(__instance).Field("disallowCheaperThan").GetValue<float>();
                //List<ThingDef> disallowedThingDefs = Traverse.Create(__instance).Field("disallowedThingDefs").GetValue<List<ThingDef>>();
                HashSet<ThingDef> allowedDefs = Traverse.Create(__instance).Field("allowedDefs").GetValue<HashSet<ThingDef>>();


                if (!categories.NullOrEmpty())
                {
                    __result = DefDatabase<ThingCategoryDef>.GetNamed(categories[0]).label;
                    for (int i = 1; i < categories.Count; i++)
                        __result += ", " + DefDatabase<ThingCategoryDef>.GetNamed(categories[i]).label;
                }
                else if (!allowAllWhoCanMake.NullOrEmpty())
                {
                    HashSet<StuffCategoryDef> l = new HashSet<StuffCategoryDef>();
                    foreach (var c in (allowAllWhoCanMake)) l.AddRange(c.stuffCategories);
                    __result = "";
                    foreach (var def in l)
                        __result += __result == "" ? def.label.CapitalizeFirst() : ", " + def.label.CapitalizeFirst();
                }
                else if (allowedDefs != null && allowedDefs.Count > 0)
                {
                    __result = "";
                    foreach (var thing in allowedDefs)
                        __result += __result == "" ? thing.label : ", " + thing.label;

                }
                else if (!thingDefs.NullOrEmpty())
                {
                    __result = thingDefs[0].label;
                    for (int i = 1; i < thingDefs.Count; i++)
                        __result += ", " + thingDefs[i].label;
                }
                else __result = "UsableIngredients".Translate();
                __instance.customSummary = __result;
                return false;
                
            }
        }
    }
}
