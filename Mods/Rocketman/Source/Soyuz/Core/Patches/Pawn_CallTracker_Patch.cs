using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(Pawn_CallTracker), nameof(Pawn_CallTracker.CallTrackerTick))]
    public class Pawn_CallTracker_CallTrackerTick_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var codes = instructions.MethodReplacer(
                AccessTools.Method(typeof(Gen), nameof(Gen.IsHashIntervalTick), new[] { typeof(Thing), typeof(int) }),
                AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsCustomTickInterval))).ToList();
            var l1 = generator.DefineLabel();

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld,
                AccessTools.Field(typeof(Pawn_CallTracker), nameof(Pawn_CallTracker.pawn)));
            yield return new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsBeingThrottled)));
            yield return new CodeInstruction(OpCodes.Brfalse_S, l1);

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld,
                AccessTools.Field(typeof(Pawn_CallTracker), nameof(Pawn_CallTracker.ticksToNextCall)));

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld,
                AccessTools.Field(typeof(Pawn_CallTracker), nameof(Pawn_CallTracker.pawn)));
            yield return new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.GetTimeDelta)));
            yield return new CodeInstruction(OpCodes.Ldc_I4, 1);
            yield return new CodeInstruction(OpCodes.Sub);

            yield return new CodeInstruction(OpCodes.Sub);
            yield return new CodeInstruction(OpCodes.Stfld,
                AccessTools.Field(typeof(Pawn_CallTracker), nameof(Pawn_CallTracker.ticksToNextCall)));

            codes[0].labels.Add(l1);
            foreach (var code in codes)
                yield return code;
        }
    }
}