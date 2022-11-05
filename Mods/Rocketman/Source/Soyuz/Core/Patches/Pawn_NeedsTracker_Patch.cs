using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(Pawn_NeedsTracker), nameof(Pawn_NeedsTracker.NeedsTrackerTick))]
    public class Pawn_NeedsTracker_NeedTrackerTick_Patch
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