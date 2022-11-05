using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(Pawn_RecordsTracker), nameof(Pawn_RecordsTracker.RecordsTick))]
    public class Pawn_RecordsTracker_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var codes = instructions.MethodReplacer(
                AccessTools.Method(typeof(Gen), nameof(Gen.IsHashIntervalTick), new[] {typeof(Thing), typeof(int)}),
                AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsCustomTickInterval))).ToList();
            foreach (var code in codes)
            {
                if (code.OperandIs(80))
                    code.operand = 90;
                yield return code;
            }
        }
    }
}