using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimalsLogic
{
    /*
     * Replaces all meat gained from butchery with chicken meat for generic animals, human meat for humanlikes and insect meat for insects.
     */
    class TastesLikeChicken
    {
        // Verse.Pawn
        // public override IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
        [HarmonyPatch(typeof(Pawn), "ButcherProducts", new Type[] { typeof(Pawn), typeof(float) })]
        static class Pawn_ButcherProducts_Patch
        {
            static void Postfix(ref IEnumerable<Thing> __result, ref Pawn __instance, float efficiency)
            {
                if (__result == null)
                {
                    return;
                }

                List<Thing> result = new List<Thing>(__result);

                CompShearable shareable;
                if (Settings.shear_corpses && (shareable = __instance.GetComp<CompShearable>()) != null && __instance.ageTracker.CurLifeStage.shearable)
                {
                    float fullness = shareable.Fullness;
                    if (__instance.Faction == null || !__instance.Faction.IsPlayer)
                        fullness = Rand.Value * 0.5f + 0.5f; // no one shears wild animals so they always have wool, but less than full (50 to 100%)

                    if (fullness >= 0.2)
                    {
                        CompProperties_Shearable props = shareable.Props;
                        int total_wool = GenMath.RoundRandom((float)props.woolAmount * fullness * efficiency);
                        while (total_wool > 0)
                        {
                            int wool_stack = Mathf.Clamp(total_wool, 1, props.woolDef.stackLimit);
                            total_wool -= wool_stack;
                            Thing thing = ThingMaker.MakeThing(props.woolDef);
                            thing.stackCount = wool_stack;
                            result.Add(thing);
                        }
                        __result = result.AsEnumerable();
                    }
                }

                if (!Settings.tastes_like_chicken)
                {
                    return;
                }

                Thing meat = result.Find(x => x.def.IsIngestible && x.def.ingestible.foodType == FoodTypeFlags.Meat);

                if (meat == null)
                {
                    return;
                }

                if (meat.def.defName.Contains("RawCHFood")) // Cosmic Horrors mod semi-support
                {
                    return; // do nothing
                }
                else if (__instance.RaceProps.Humanlike)
                {
                    meat.def = DefDatabase<ThingDef>.GetNamed("Meat_Human");
                }
                else if (__instance.RaceProps.FleshType == FleshTypeDefOf.Insectoid)
                {
                    meat.def = DefDatabase<ThingDef>.GetNamed("Meat_Megaspider");
                }
                else if (__instance.RaceProps.FleshType == FleshTypeDefOf.Normal)
                {
                    meat.def = DefDatabase<ThingDef>.GetNamed("Meat_Chicken");
                }

                __result = result.AsEnumerable();
            }
        }
    }

    /*
     * Configurable chance for animals to instantly rot from toxic buildup. Vanilla does it in a stupid way that makes animals like snakes to kill all other animals and die from starvation because they can't eat their prey.
     */
    class NoToxicRot
    {
        public static void Patch()
        {
            AnimalsLogic.harmony.Patch(
                typeof(Pawn).GetMethod("Kill"),
                transpiler: new HarmonyMethod(typeof(NoToxicRot).GetMethod(nameof(KillPatch)))
                );
        }

        [HarmonyTranspiler]
        public static List<CodeInstruction> KillPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            MethodInfo target = typeof(Hediff).GetMethod("get_Severity");

            for (int i = 0; i < codes.Count; i++)
            {
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand == target)
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
                {
                    codes[i].operand = typeof(NoToxicRot).GetMethod(nameof(RotChance)); // substitutes original function with mine; it takes Verse.Hediff from stack and instead of calling get_Severity() calls wrapper that returns severity * modifier
                    return codes;
                }
            }

            Log.Error("Animal Logic is unable to patch Pawn.Kill method.");
            return codes;
        }

        public static float RotChance(Hediff toxicBuildup)
        {
            return toxicBuildup.Severity * Settings.toxic_buildup_rot;
        }
    }
}
