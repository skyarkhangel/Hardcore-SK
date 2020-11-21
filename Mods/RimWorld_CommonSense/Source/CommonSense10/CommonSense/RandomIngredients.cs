using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;

namespace CommonSense
{

    class RandomIngredients
    {
        
        //public static Thing MakeThing(ThingDef def, ThingDef stuff = null)
        [HarmonyPatch(typeof(ThingMaker), "MakeThing", new Type[] { typeof(ThingDef), typeof(ThingDef) })]
        static class ThingMaker_MakeThing_CommonSensePatch
        {
            public static Dictionary<ThingDef, RecipeDef> hTable = new Dictionary<ThingDef, RecipeDef>();

            static void Postfix(Thing __result, ThingDef def, ThingDef stuff)
            {
                if (!Settings.add_meal_ingredients || __result == null || !__result.def.IsIngestible)
                    return;

                CompIngredients ings = __result.TryGetComp<CompIngredients>();
                if (ings == null || ings.ingredients.Count > 0)
                    return;

                RecipeDef d = hTable.TryGetValue(def);
                if (d == null)
                {
                    List<RecipeDef> l = DefDatabase<RecipeDef>.AllDefsListForReading;
                    if (l == null)
                        return;

                    d = l.Where(x => !x.ingredients.NullOrEmpty() && x.products.Any(y => y.thingDef == def)).RandomElement();
                    
                    if (d == null)
                        return;

                    hTable.Add(def, d);
                }
                foreach (IngredientCount c in d.ingredients)
                {
                    ThingFilter ic = c.filter;

                    if (ic == null)
                        return;

                    IEnumerable<ThingDef> l = ic.AllowedThingDefs;

                    if (l == null)
                        return;

                    ThingDef td = l.Where(
                        x => x.IsIngestible && x.comps != null && !x.comps.Any(y => y.compClass == typeof(CompIngredients)) &&
                        !FoodUtility.IsHumanlikeMeat(x) && (x.ingestible.specialThoughtAsIngredient == null || x.ingestible.specialThoughtAsIngredient.stages == null
                        || x.ingestible.specialThoughtAsIngredient.stages[0].baseMoodEffect >= 0)
                    ).RandomElement();

                    if (td != null)
                        ings.RegisterIngredient(td);
                }
            }
        }
        
        //public static IEnumerable<Thing> MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
        [HarmonyPatch]
        static class GenRecipe_MakeRecipeProducts_CommonSensePatch
        {
            private static FieldInfo ingredientsCompField;

            internal static void ClearIngs(CompIngredients ings)
            {
                ings.ingredients.Clear();
            }

            internal static MethodBase TargetMethod()
            {
                Type nestedTypeResult = null;
                const string targetMethod = "<MakeRecipeProducts>";
                foreach (var nestedType in typeof(GenRecipe).GetNestedTypes(AccessTools.all))
                {
                    if (!nestedType.Name.Contains(targetMethod)) continue;
                    nestedTypeResult = nestedType;
                }

                if (nestedTypeResult == null) throw new Exception($"Could not find {targetMethod} Iterator Class");

                if (ingredientsCompField == null)
                {
                    var fields = AccessTools.GetDeclaredFields(nestedTypeResult);
                    foreach (var i in fields)
                    {
                        if (i.FieldType == typeof(CompIngredients))
                            if (ingredientsCompField == null)
                                ingredientsCompField = i;
                            else
                                throw new Exception("Multiple CompIngredients fields found");
                    }
                }

                if (ingredientsCompField == null) throw new Exception($"Could not find (CompIngredients) in {nestedTypeResult.FullName}.DeclaredFields");

                var result = AccessTools.Method(nestedTypeResult, "MoveNext");

                if (result == null) throw new Exception($"Could not find MoveNext in {nestedTypeResult.FullName}");

                return result;
            }

            [HarmonyTranspiler]
            internal static IEnumerable<CodeInstruction> CleanIngList(IEnumerable<CodeInstruction> instrs)
            {

                CodeInstruction prei = null;
                foreach (var i in (instrs))
                {
                    yield return i;

                    if (i.opcode == OpCodes.Brfalse && prei.opcode == OpCodes.Ldfld && prei.operand == ingredientsCompField)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, ingredientsCompField);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GenRecipe_MakeRecipeProducts_CommonSensePatch), nameof(GenRecipe_TryDispenseFood_CommonSensePatch.ClearIngs), new Type[] { typeof(CompIngredients) }));
                    }
                    prei = i;
                }
            }
        }

        //public virtual Thing TryDispenseFood()
        [HarmonyPatch(typeof(Building_NutrientPasteDispenser), "TryDispenseFood")]
        static class GenRecipe_TryDispenseFood_CommonSensePatch
        {
            public static void ClearIngs(CompIngredients ings)
            {
                ings.ingredients.Clear();
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> CleanIngList(IEnumerable<CodeInstruction> instrs)
            {
                foreach (CodeInstruction instr in instrs)
                {
                    yield return instr;
                    if (instr.opcode == OpCodes.Stloc_S && instr.operand is LocalBuilder && ((LocalBuilder)instr.operand).LocalType == typeof(CompIngredients))
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, instr.operand);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GenRecipe_MakeRecipeProducts_CommonSensePatch), nameof(GenRecipe_TryDispenseFood_CommonSensePatch.ClearIngs), new Type[] { typeof(CompIngredients) }));
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(CompIngredients), "PostSplitOff", new Type[] {typeof(Thing)})]
        [HarmonyPatch(typeof(Thing), "SplitOff", new Type[] { typeof(int) })]
        static class Thing_SplitOff_CommonSensePatch
        {

            public static void ClearIngs(Thing thing)
            {
                CompIngredients comp = thing.TryGetComp<CompIngredients>();
                if (comp != null)
                    comp.ingredients.Clear();
            }

            [HarmonyTranspiler]
            internal static IEnumerable<CodeInstruction> CleanIngList(IEnumerable<CodeInstruction> instrs)
            {
                CodeInstruction prei = null;
                foreach (var i in (instrs))
                {
                    yield return i;

                    if(i.opcode == OpCodes.Stloc_0 && prei.opcode == OpCodes.Call && prei.operand == typeof(ThingMaker).GetMethod(nameof(ThingMaker.MakeThing)))
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Thing_SplitOff_CommonSensePatch), nameof(Thing_SplitOff_CommonSensePatch.ClearIngs), new Type[] { typeof(Thing) }));
                    }
                    prei = i;
                }
            }
        }
    }
}
