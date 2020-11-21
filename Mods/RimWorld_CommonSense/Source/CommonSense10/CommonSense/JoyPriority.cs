using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace CommonSense
{
    public class CompJoyToppedOff : ThingComp
    {
        public bool JoyToppedOff = false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref JoyToppedOff, "CommonSenseJoyToppedOff", defaultValue: false);
        }
    }

    class JoyPriority
    {

        static float JoyPolicePriority(Pawn pawn)
        {
            if (!Settings.fun_police)
                return 0.8f;

            CompJoyToppedOff c = pawn.TryGetComp<CompJoyToppedOff>();
            if (c == null || !c.JoyToppedOff)
                return 0.95f;
            else
                return 0.8f;
        }

        /*
        static float PickUpMeal(Pawn pawn)
        {
            if (!Settings.fun_police)
                return 20f;

            if (pawn.GetTimeAssignment() == TimeAssignmentDefOf.Sleep && (pawn.needs == null || pawn.needs.rest == null || pawn.needs.rest.CurLevel > 0.3f) || pawn.GetTimeAssignment() == TimeAssignmentDefOf.Joy)
                return 9999f;
            else
                return 20f;
        }
        */

        [HarmonyPatch(typeof(ThinkNode_Priority_GetJoy), "GetPriority")]
        static class ThinkNode_Priority_GetJoy_GetPriority_CommonSensePatch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase mb)
            {
                foreach (var i in (instructions))
                {
                    if (i.opcode == OpCodes.Ldc_R4 && (float)i.operand == 0.95f)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(JoyPriority), nameof(JoyPriority.JoyPolicePriority)));
                    }
                    else if (i.opcode == OpCodes.Ldc_R4 && (float)i.operand == 2f)
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 4f);
                    }
                    else
                    {
                        yield return i;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(JobGiver_PackFood), "TryGiveJob")]
        static class JobGiver_PackFood_TryGiveJob_CommonSensePatch
        {
            static MethodInfo LGetInventoryPackableFoodNutrition = null;
            static MethodInfo LIsGoodPackableFoodFor = null;
            static bool Prepare()
            {

                LGetInventoryPackableFoodNutrition = AccessTools.Method(typeof(JobGiver_PackFood), "GetInventoryPackableFoodNutrition");
                if (LGetInventoryPackableFoodNutrition == null)
                    throw new Exception("Can't get method JobGiver_PackFood.GetInventoryPackableFoodNutrition");

                LIsGoodPackableFoodFor = AccessTools.Method(typeof(JobGiver_PackFood), "IsGoodPackableFoodFor");
                if (LIsGoodPackableFoodFor == null)
                    throw new Exception("Can't get method JobGiver_PackFood.IsGoodPackableFoodFor");

                return true;
            }

            static bool Prefix(JobGiver_PackFood __instance, ref Job __result, Pawn pawn)
            {
                if (!Settings.fun_police || pawn.timetable == null || pawn.timetable.CurrentAssignment != TimeAssignmentDefOf.Joy
                    && (pawn.timetable.CurrentAssignment != TimeAssignmentDefOf.Sleep || pawn.needs != null && pawn.needs.rest != null && pawn.needs.rest.CurLevel <= 0.3f))
                    return true;

                if (pawn.inventory == null)
                {
                    __result = null;
                    return false;
                }

                float invNutrition = (float)LGetInventoryPackableFoodNutrition.Invoke(__instance, new object[] { pawn });
                if (invNutrition > 0.4f)
                {
                    __result = null;
                    return false;
                }

                if (pawn.Map.resourceCounter.TotalHumanEdibleNutrition < pawn.Map.mapPawns.ColonistsSpawnedCount * 1.5f)
                {
                    __result = null;
                    return false;
                }

                Thing thing = null;
                ref Thing foodSource = ref thing;
                ThingDef thingDef = null;
                ref ThingDef foodDef = ref thingDef;

                if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, false, out foodSource, out foodDef, false, false, false, false, false, false, true))
                {
                    __result = null;
                    return false;
                }

                Building_NutrientPasteDispenser building_NutrientPasteDispenser = thing as Building_NutrientPasteDispenser;
                if (building_NutrientPasteDispenser != null)
                {
                    __result = null;
                    return false;
                }

                if (!(bool)LIsGoodPackableFoodFor.Invoke(__instance, new object[] { thing, pawn }))
                {
                    __result = null;
                    return false;
                }

                float num = pawn.needs.food.MaxLevel - invNutrition;
                int num2 = Mathf.FloorToInt(num / thing.GetStatValue(StatDefOf.Nutrition, true));
                num2 = Mathf.Min(num2, thing.stackCount);
                num2 = Mathf.Max(num2, 1);

                __result = new Job(JobDefOf.TakeInventory, thing)
                {
                    count = num2
                };
                return false;
            }
        }
    }
}
