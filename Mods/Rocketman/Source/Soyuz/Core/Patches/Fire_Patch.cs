using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RocketMan;
using Verse;
using Verse.Sound;

namespace Soyuz.Patches
{
    // [SoyuzPatch(typeof(Fire), nameof(Fire.Tick))]
    // public static class Fire_Tick_Patch
    // {
    //    private const int _TickInterval = 3;
    //
    //    private static MethodBase mIsFireCustomTickInterval = AccessTools.Method(typeof(Fire_Tick_Patch), nameof(Fire_Tick_Patch.IsFireTickHashInterval));
    //
    //    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    //    {
    //        List<CodeInstruction> codes = instructions.ToList();
    //        Label l1 = generator.DefineLabel();
    //        Label l2 = generator.DefineLabel();
    //
    //        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(RocketPrefs), nameof(RocketPrefs.Enabled)));
    //        yield return new CodeInstruction(OpCodes.Brfalse_S, l2);
    //
    //        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(RocketPrefs), nameof(RocketPrefs.TimeDilation)));
    //        yield return new CodeInstruction(OpCodes.Brfalse_S, l2);
    //
    //        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(RocketPrefs), nameof(RocketPrefs.TimeDilationFire)));
    //        yield return new CodeInstruction(OpCodes.Brfalse_S, l2);
    //
    //        yield return new CodeInstruction(OpCodes.Ldarg_0);
    //        yield return new CodeInstruction(OpCodes.Ldc_I4_S, _TickInterval);
    //        yield return new CodeInstruction(OpCodes.Call, mIsFireCustomTickInterval);
    //        yield return new CodeInstruction(OpCodes.Brtrue_S, l2);
    //
    //        yield return new CodeInstruction(OpCodes.Ldarg_0);
    //        yield return new CodeInstruction(OpCodes.Ldarg_0);
    //        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Fire), nameof(Fire.ticksSinceSpawn)));
    //        yield return new CodeInstruction(OpCodes.Ldc_I4_S, 1);
    //        yield return new CodeInstruction(OpCodes.Add);
    //        yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(Fire), nameof(Fire.ticksSinceSpawn)));
    //
    //        yield return new CodeInstruction(OpCodes.Ldarg_0);
    //        yield return new CodeInstruction(OpCodes.Ldarg_0);
    //        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Fire), nameof(Fire.ticksSinceSpread)));
    //        yield return new CodeInstruction(OpCodes.Ldc_I4_S, 1);
    //        yield return new CodeInstruction(OpCodes.Add);
    //        yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(Fire), nameof(Fire.ticksSinceSpread)));
    //
    //        yield return new CodeInstruction(OpCodes.Ldarg_0);
    //        yield return new CodeInstruction(OpCodes.Ldarg_0);
    //        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Fire), nameof(Fire.ticksUntilSmoke)));
    //        yield return new CodeInstruction(OpCodes.Ldc_I4_S, 1);
    //        yield return new CodeInstruction(OpCodes.Sub);
    //        yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(Fire), nameof(Fire.ticksUntilSmoke)));
    //
    //        yield return new CodeInstruction(OpCodes.Ldarg_0);
    //        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Fire_Tick_Patch), nameof(Fire_Tick_Patch.TickExtras)));
    //
    //        yield return new CodeInstruction(OpCodes.Br_S, l1);
    //
    //        if (codes[0].labels == null)
    //        {
    //            codes[0].labels = new List<Label>();
    //        }
    //        codes[0].labels.Add(l2);
    //
    //        for (int i = 0; i < codes.Count; i++)
    //        {
    //            CodeInstruction code = codes[i];
    //
    //            if (code.opcode == OpCodes.Ret)
    //            {
    //                if (code.labels == null)
    //                {
    //                    code.labels = new List<Label>();
    //                }
    //                code.labels.Add(l1);
    //            }
    //            yield return code;
    //        }
    //    }
    //
    //    private static bool IsFireTickHashInterval(Fire fire, int interval)
    //    {
    //        return (!RocketPrefs.Enabled || !RocketPrefs.TimeDilation || !RocketPrefs.TimeDilationFire)
    //            ? true : (fire.IsHashIntervalTick(interval) || fire.IsHashIntervalTick(150));
    //    }
    //
    //    private static void TickExtras(Fire fire)
    //    {
    //        if (fire.sustainer != null)
    //        {
    //            fire.sustainer.Maintain();
    //        }
    //        else if (!fire.Position.Fogged(fire.Map))
    //        {
    //            SoundInfo info = SoundInfo.InMap(new TargetInfo(fire.Position, fire.Map), MaintenanceType.PerTick);
    //            fire.sustainer = SustainerAggregatorUtility.AggregateOrSpawnSustainerFor(fire, SoundDefOf.FireBurning, info);
    //        }
    //    }
    // }
}
