using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.tick.room", Category.Tick)]
    internal class H_Room
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods() => typeof(RoomStatWorker).AllSubclasses().Select(rsw => rsw.GetMethod("GetScore"));
        public static string GetLabel(RoomStatWorker __instance) => $"{__instance.def.defName} - {__instance.def.workerClass.FullName}";
    }
}