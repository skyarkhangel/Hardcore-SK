using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer.Profiling
{
    [Entry("entry.tick.custom", Category.Tick)]
    internal class CustomProfilersTick
    {
        public static bool Active = false;
    }
}
