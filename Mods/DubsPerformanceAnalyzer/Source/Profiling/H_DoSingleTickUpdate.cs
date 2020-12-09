using HarmonyLib;
using Verse;

namespace Analyzer.Profiling
{
    internal class H_DoSingleTickUpdate
    {
#if DEBUG
        public static void Prefix()
        {
            if (GUIController.CurrentCategory == Category.Tick) // If we in Tick mode, start our update (can happen multiple times p frame)
                ProfileController.BeginUpdate();
        }
#endif

        public static void Postfix()
        {
            if (GUIController.CurrentCategory == Category.Tick) // If we in Tick mode, finish our update (can happen multiple times p frame)
                ProfileController.EndUpdate();
        }
    }
}