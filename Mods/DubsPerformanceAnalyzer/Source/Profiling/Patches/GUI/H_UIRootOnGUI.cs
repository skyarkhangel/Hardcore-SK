using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.gui.uiroot", Category.GUI)]
    internal class H_UIRootOnGUI
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            yield return AccessTools.Method(typeof(UIRoot_Play), nameof(UIRoot_Play.UIRootOnGUI));
            yield return AccessTools.Method(typeof(UnityGUIBugsFixer), nameof(UnityGUIBugsFixer.OnGUI));
            yield return AccessTools.Method(typeof(MapInterface), nameof(MapInterface.MapInterfaceOnGUI_BeforeMainTabs));
            yield return AccessTools.Method(typeof(MapInterface), nameof(MapInterface.MapInterfaceOnGUI_AfterMainTabs));
            yield return AccessTools.Method(typeof(GameComponentUtility), nameof(GameComponentUtility.GameComponentOnGUI));
            yield return AccessTools.Method(typeof(MainButtonsRoot), nameof(MainButtonsRoot.MainButtonsOnGUI));
            yield return AccessTools.Method(typeof(WindowStack), nameof(WindowStack.WindowStackOnGUI));
            yield return AccessTools.Method(typeof(AlertsReadout), nameof(AlertsReadout.AlertsReadoutOnGUI));
        }
    }
}
