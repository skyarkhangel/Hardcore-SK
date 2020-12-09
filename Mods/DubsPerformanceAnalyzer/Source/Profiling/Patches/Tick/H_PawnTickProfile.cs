using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

namespace Analyzer.Profiling
{
    [Entry("entry.tick.pawn", Category.Tick)]
    internal static class H_PawnTickProfile
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            yield return AccessTools.Method(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.AgeTick));
            yield return AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.HealthTick));
            yield return AccessTools.Method(typeof(Pawn_RecordsTracker), nameof(Pawn_RecordsTracker.RecordsTick));
            yield return AccessTools.Method(typeof(Pawn_InventoryTracker), nameof(Pawn_InventoryTracker.InventoryTrackerTick));
            yield return AccessTools.Method(typeof(VerbTracker), nameof(VerbTracker.VerbsTick));
            yield return AccessTools.Method(typeof(Pawn_CarryTracker), nameof(Pawn_CarryTracker.CarryHandsTick));
            yield return AccessTools.Method(typeof(Pawn_NeedsTracker), nameof(Pawn_NeedsTracker.NeedsTrackerTick));
            yield return AccessTools.Method(typeof(Pawn_MindState), nameof(Pawn_MindState.MindStateTick));
            yield return AccessTools.Method(typeof(Pawn_RotationTracker), nameof(Pawn_RotationTracker.RotationTrackerTick));
            yield return AccessTools.Method(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.PatherTick));
            yield return AccessTools.Method(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.FinalizeTick));
            yield return AccessTools.Method(typeof(Pawn_StanceTracker), nameof(Pawn_StanceTracker.StanceTrackerTick));
            yield return AccessTools.Method(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.EquipmentTrackerTick));
            yield return AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.ApparelTrackerTick));
            yield return AccessTools.Method(typeof(Pawn_SkillTracker), nameof(Pawn_SkillTracker.SkillsTick));
            yield return AccessTools.Method(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.GuestTrackerTick));
            yield return AccessTools.Method(typeof(Pawn_RoyaltyTracker), nameof(Pawn_RoyaltyTracker.RoyaltyTrackerTick));
            yield return AccessTools.Method(typeof(Pawn_AbilityTracker), nameof(Pawn_AbilityTracker.AbilitiesTick));
            yield return AccessTools.Method(typeof(Pawn_TrainingTracker), nameof(Pawn_TrainingTracker.TrainingTrackerTickRare));
            yield return AccessTools.Method(typeof(Pawn_CallTracker), nameof(Pawn_CallTracker.CallTrackerTick));
            yield return AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.RelationsTrackerTick));
            yield return AccessTools.Method(typeof(Pawn_PsychicEntropyTracker), nameof(Pawn_PsychicEntropyTracker.PsychicEntropyTrackerTick));
            yield return AccessTools.Method(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.InteractionsTrackerTick));
            yield return AccessTools.Method(typeof(Pawn_DrawTracker), nameof(Pawn_DrawTracker.DrawTrackerTick));
        }
    }
}