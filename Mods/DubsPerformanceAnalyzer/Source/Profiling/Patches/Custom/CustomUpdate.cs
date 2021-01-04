using HarmonyLib;
using System.Reflection;

namespace Analyzer.Profiling
{
    [Entry("entry.update.custom", Category.Update)]
    internal class CustomProfilersUpdate
    {
        public static bool Active = false;
    }

}
