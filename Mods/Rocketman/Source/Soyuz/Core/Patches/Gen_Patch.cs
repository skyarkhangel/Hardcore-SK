using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(Gen), nameof(Gen.IsHashIntervalTick), parameters: new[] { typeof(Thing), typeof(int) })]
    public static class Gen_IsHashIntervalTick_Patch
    {
        private static MethodBase mIsCustomTickIntervalInternel = AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsCustomTickInterval_newtemp));
        private static MethodBase mCurrent = AccessTools.PropertyGetter(typeof(ContextualExtensions), nameof(ContextualExtensions.Current));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            if (mIsCustomTickIntervalInternel == null || mCurrent == null)
            {
                Log.Error($"SOYUZ:mIsCustomTickIntervalInternel is {mIsCustomTickIntervalInternel == null}");
                Log.Error($"SOYUZ:mCurrent is {mCurrent == null}");
            }
            List<CodeInstruction> codes = instructions.ToList();
            Label l1 = generator.DefineLabel();
            Label l2 = generator.DefineLabel();

            yield return new CodeInstruction(OpCodes.Call, mCurrent);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Bne_Un_S, l1);

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, mIsCustomTickIntervalInternel);

            yield return new CodeInstruction(OpCodes.Br_S, l2);

            codes[0].labels = new List<Label>() { l1 };
            codes[codes.Count - 1].labels = new List<Label>() { l2 };
            foreach (CodeInstruction code in codes)
                yield return code;
        }
    }
}
