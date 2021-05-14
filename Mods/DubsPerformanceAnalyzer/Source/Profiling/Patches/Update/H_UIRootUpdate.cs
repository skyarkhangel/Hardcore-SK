using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.Sound;

namespace Analyzer.Profiling
{
    [Entry("entry.update.uiroot", Category.Update)]
    internal static class H_UIRootUpdate
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            yield return AccessTools.Method(typeof(ScreenshotTaker), nameof(ScreenshotTaker.Update));
            yield return AccessTools.Method(typeof(DragSliderManager), nameof(DragSliderManager.DragSlidersUpdate));
            yield return AccessTools.Method(typeof(WindowStack), nameof(WindowStack.WindowsUpdate));
            yield return AccessTools.Method(typeof(MouseoverSounds), nameof(MouseoverSounds.ResolveFrame));
            yield return AccessTools.Method(typeof(UIHighlighter), nameof(UIHighlighter.UIHighlighterUpdate));
            yield return AccessTools.Method(typeof(Messages), nameof(Messages.Update));
            yield return AccessTools.Method(typeof(WorldInterface), nameof(WorldInterface.WorldInterfaceUpdate));
            yield return AccessTools.Method(typeof(MapInterface), nameof(MapInterface.MapInterfaceUpdate));
            yield return AccessTools.Method(typeof(AlertsReadout), nameof(AlertsReadout.AlertsReadoutUpdate));
            yield return AccessTools.Method(typeof(LessonAutoActivator), nameof(LessonAutoActivator.LessonAutoActivatorUpdate));
            yield return AccessTools.Method(typeof(Tutor), nameof(Tutor.TutorUpdate));
        }
    }
}