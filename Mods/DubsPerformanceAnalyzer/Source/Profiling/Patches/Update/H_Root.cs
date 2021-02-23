using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;
using Verse.Sound;
using Verse.Steam;

namespace Analyzer.Profiling
{
    [Entry("entry.update.root", Category.Update)]
    internal static class H_Root
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            yield return AccessTools.Method(typeof(ResolutionUtility), nameof(ResolutionUtility.Update));
            yield return AccessTools.Method(typeof(RealTime), nameof(RealTime.Update));
            yield return AccessTools.Method(typeof(LongEventHandler), nameof(LongEventHandler.LongEventsUpdate));
            yield return AccessTools.Method(typeof(Rand), nameof(Rand.EnsureStateStackEmpty));
            yield return AccessTools.Method(typeof(Widgets), nameof(Widgets.EnsureMousePositionStackEmpty));
            yield return AccessTools.Method(typeof(SteamManager), nameof(SteamManager.Update));
            yield return AccessTools.Method(typeof(PortraitsCache), nameof(PortraitsCache.PortraitsCacheUpdate));
            yield return AccessTools.Method(typeof(AttackTargetsCache), nameof(AttackTargetsCache.AttackTargetsCacheStaticUpdate));
            yield return AccessTools.Method(typeof(Pawn_MeleeVerbs), nameof(Pawn_MeleeVerbs.PawnMeleeVerbsStaticUpdate));
            yield return AccessTools.Method(typeof(Storyteller), nameof(Storyteller.StorytellerStaticUpdate));
            yield return AccessTools.Method(typeof(CaravanInventoryUtility), nameof(CaravanInventoryUtility.CaravanInventoryUtilityStaticUpdate));
            yield return AccessTools.Method(typeof(UIRoot), nameof(UIRoot.UIRootUpdate));
            yield return AccessTools.Method(typeof(SoundRoot), nameof(SoundRoot.Update));
        }
    }
}