using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ExperimentalOptimizations.Optimizations
{
    public class Pawn_MindState_Settings
    {
        public static bool Pawn_MindState = true;
       
        public static bool Enabled() => Pawn_MindState;

        public static void DoSettingsWindowContents(Listing_Standard l)
        {
            if (l.CheckBoxIsChanged("Pawn_MindState".Translate(), ref Pawn_MindState))
            {
                if (Pawn_MindState) MindState.Patch();
                else MindState.UnPatch();
            }
        }

        public static void ExposeData()
        {
            Scribe_Values.Look(ref Pawn_MindState, "Pawn_MindState", true);
        }
    }

    [Optimization("Pawn_MindState", typeof(Pawn_MindState_Settings))]
    public class MindState
    {
        private static readonly List<H.PatchInfo> Patches = new List<H.PatchInfo>();

        public static void Init()
        {
            var prefix = typeof(MindState).Method(nameof(MindStateTick)).ToHarmonyMethod(priority: 999);
            H.PatchInfo patch = typeof(Pawn_MindState).Method(nameof(Pawn_MindState.MindStateTick)).Patch(prefix: prefix, autoPatch: false);
            Patches.Add(patch);
        }

        public static void Patch()
        {
            foreach (var patch in Patches) patch.Enable();
            Log.Message($"[ExperimentalOptimizations] PatchMindStateTick done");
        }

        public static void UnPatch()
        {
            foreach (var patch in Patches) patch.Disable();
            Log.Message($"[ExperimentalOptimizations] UnPatchMindStateTick done");
        }

        private static bool MindStateTick(Pawn_MindState __instance)
        {
            if (Find.TickManager.TicksGame % 123 == 0)
                return true;

            bool callOrig = __instance.pawn.IsHashIntervalTick(5);
            if (!callOrig && GenLocalDate.DayTick(__instance.pawn) == 0)
                __instance.interactionsToday = 0;

            return callOrig;
        }
    }
}