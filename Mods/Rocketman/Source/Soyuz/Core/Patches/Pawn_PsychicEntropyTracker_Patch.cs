using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(Pawn_PsychicEntropyTracker), nameof(Pawn_PsychicEntropyTracker.PsychicEntropyTrackerTick))]
    public class Pawn_PsychicEntropyTracker_PsychicEntropyTrackerTick_Patch
    {
        public static bool Prepare()
        {
            return ModsConfig.royaltyActive;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
           ILGenerator generator)
        {
            return instructions.MethodReplacer(
                AccessTools.Method(typeof(Gen), nameof(Gen.IsHashIntervalTick), new[] { typeof(Thing), typeof(int) }),
                AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsCustomTickInterval)));
        }
    }
}
