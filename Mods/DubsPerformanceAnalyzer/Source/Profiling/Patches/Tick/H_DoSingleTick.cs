using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.tick.single", Category.Tick)]
    internal class H_DoSingleTick
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            yield return AccessTools.Method(typeof(GameComponentUtility), nameof(GameComponentUtility.GameComponentTick));
            yield return AccessTools.Method(typeof(ScreenshotTaker), nameof(ScreenshotTaker.QueueSilentScreenshot));
            yield return AccessTools.Method(typeof(FilthMonitor), nameof(FilthMonitor.FilthMonitorTick));
            yield return AccessTools.Method(typeof(Map), nameof(Map.MapPreTick));
            yield return AccessTools.Method(typeof(Map), nameof(Map.MapPostTick));
            yield return AccessTools.Method(typeof(DateNotifier), nameof(DateNotifier.DateNotifierTick));
            yield return AccessTools.Method(typeof(Scenario), nameof(Scenario.TickScenario));
            yield return AccessTools.Method(typeof(World), nameof(World.WorldTick));
            yield return AccessTools.Method(typeof(StoryWatcher), nameof(StoryWatcher.StoryWatcherTick));
            yield return AccessTools.Method(typeof(GameEnder), nameof(GameEnder.GameEndTick));
            yield return AccessTools.Method(typeof(Storyteller), nameof(Storyteller.StorytellerTick));
            yield return AccessTools.Method(typeof(TaleManager), nameof(TaleManager.TaleManagerTick));
            yield return AccessTools.Method(typeof(World), nameof(World.WorldPostTick));
            yield return AccessTools.Method(typeof(History), nameof(History.HistoryTick));
            yield return AccessTools.Method(typeof(LetterStack), nameof(LetterStack.LetterStackTick));
            yield return AccessTools.Method(typeof(Autosaver), nameof(Autosaver.AutosaverTick));
        }
    }
}
