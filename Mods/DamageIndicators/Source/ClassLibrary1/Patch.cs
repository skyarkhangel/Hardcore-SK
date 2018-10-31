using Verse;
using UnityEngine;
using RimWorld;
using System;
using Harmony;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Reflection;

namespace DamageMotes
{
    [StaticConstructorOnStartup, HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
    public static class DamageMotes_Patch
    {
        static DamageMotes_Patch()
        {
            HarmonyInstance.Create("com.spdskatr.DamageMotes.Patch").PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("SS Damage Indicators initialized.\n " + 
                "Patched infix non-destructive: " + typeof(Thing).FullName + "." + nameof(Thing.TakeDamage) + 
                "\n Patched Postfix non-destructive: " + typeof(ShieldBelt) + "." + nameof(ShieldBelt.CheckPreAbsorbDamage) +
                "\n Patched infix non-destructive: " + typeof(Verb_MeleeAttack) + ".TryCastShot");
        }
        public static void TakeDamageInfix(Thing instance, DamageWorker.DamageResult result, DamageInfo dinfo)
        {
            float num = result.totalDamageDealt;
            if (num > 0.01f && instance.Map != null && instance.ShouldDisplayDamage(dinfo.Instigator)) ThrowDamageMote(num, instance.Map, instance.DrawPos, num.ToString());
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (int i = 0; i < instructionsList.Count; i++)
            {
                var curInstruction = instructionsList[i];
                yield return curInstruction;
                if (curInstruction.opcode == OpCodes.Stloc_S && 
                    instructionsList[i - 1].operand == AccessTools.Method(typeof(DamageWorker), "Apply"))
                {
                    //Load 3 arguments: One the instance, one a local variable of the damage, one the damage info as provided in the arguments of original method
                    yield return new CodeInstruction(OpCodes.Ldarg_0);// Thing
                    yield return new CodeInstruction(OpCodes.Ldloc_S, (byte)5); // DamageResult
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // DamageInfo
                    //Call
                    yield return new CodeInstruction(OpCodes.Call, typeof(DamageMotes_Patch).GetMethod("TakeDamageInfix"));
                }
            }
        }
        public static void ThrowDamageMote(float damage, Map map, Vector3 loc, string text)
        {
            Color color = Color.white;
            //Determine colour
            if (damage >= 90f)
                color = Color.cyan;
            else if (damage >= 70f)
                color = Color.magenta;
            else if (damage >= 50f)
                color = Color.red;
            else if (damage >= 30f)
                color = Color.Lerp(Color.red, Color.yellow, 0.5f);//orange
            else if (damage >= 10f)
                color = Color.yellow;

            MoteMaker.ThrowText(loc, map, text, color, 3.65f);
        }
    }
    [HarmonyPatch(typeof(ShieldBelt), nameof(ShieldBelt.CheckPreAbsorbDamage))]
    public static class ShieldBelt_Patch
    {
        static void Postfix(DamageInfo dinfo, bool __result, ShieldBelt __instance)
        {
            if (__result && __instance.Wearer != null && __instance.Wearer.Map != null)
            {
                if (dinfo.Def != DamageDefOf.EMP)
                {
                    var amount = dinfo.Amount * Traverse.Create(__instance).Field("EnergyLossPerDamage").GetValue<float>() * 100;
                    MoteMaker.ThrowText(__instance.Wearer.DrawPos, __instance.Wearer.Map, ShieldBeltOutputString(__instance, amount), 3.65f);
                }
                if (__instance.ShieldState == ShieldState.Resetting)
                {
                    MoteMaker.ThrowText(__instance.Wearer.DrawPos, __instance.Wearer.Map, "PERSONALSHIELD_BROKEN".Translate(), 3.65f);
                }
            }
        }
        public static string ShieldBeltOutputString(Thing __instance, float amount)
        {
            return "(- " + amount.ToString("F0") + "/ " + (__instance.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true) * 100) + ")";
        }
    }
    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    public static class Verb_MeleeAttack_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instrList = instructions.ToList();
            for (int i = 0; i < instrList.Count; i++)
            {
                var instr = instrList[i];
                if (instr.opcode == OpCodes.Bge_Un && instrList[i-1].operand == typeof(Verb_MeleeAttack).GetMethod("GetNonMissChance", AccessTools.all))
                {
                    //Compares if first value less than second and pushes true/false onto stack
                    yield return new CodeInstruction(OpCodes.Clt_Un);
                    //Load second declared local variable (Thing)
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    //Use above two values on stack to call method DamageMotesUtil.TranspilerUtility_NotifyMiss
                    yield return new CodeInstruction(OpCodes.Call, typeof(DamageMotesUtil).GetMethod(nameof(DamageMotesUtil.TranspilerUtility_NotifyMiss), AccessTools.all));
                    //Use returned value (basically value of Clt_Un opcode) and break (activate original code)
                    yield return new CodeInstruction(OpCodes.Brfalse, instr.operand);
                    continue;
                }
                yield return instr;
            }
        }
    }
    static class DamageMotesUtil
    {
        /// <summary>
        /// Used on both the instigator and the target.
        /// </summary>
        public static bool ShouldDisplayDamage(this Thing target, Thing instigator)
        {
            return LoadedModManager.GetMod<DMMod>().settings.ShouldDisplayDamageAccordingToSettings(target, instigator);
        }
        public static bool TranspilerUtility_NotifyMiss(bool b, Thing t)
        {
            if (!b && t.Map != null)
                MoteMaker.ThrowText(t.DrawPos, t.Map, "DM_MISS".Translate(), 3.65f);
            return b;
        }
    }
}
