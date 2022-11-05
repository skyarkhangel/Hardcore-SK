using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;
using RocketMan;
using Verse;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(Caravan_NeedsTracker), nameof(Caravan_NeedsTracker.TrySatisfyJoyNeed))]
    public static class Caravan_NeedsTracker_TrySatisfyJoyNeed_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            return instructions.MethodReplacer(
                AccessTools.Method(typeof(Gen), nameof(Gen.IsHashIntervalTick), new[] { typeof(Thing), typeof(int) }),
                AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsCustomTickInterval)));
        }
    }
}
