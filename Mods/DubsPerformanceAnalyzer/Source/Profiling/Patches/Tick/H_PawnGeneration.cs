using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.tick.pawngen", Category.Tick)]
    public static class H_PawnGeneration
    {
        public static bool Active = false;

        [Setting("By faction")]
        public static bool ByFaction = false;

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            yield return AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new Type[] { typeof(PawnGenerationRequest) });
        }

        public static string GetName(PawnGenerationRequest request)
        {
            return ByFaction ? $"Request for {request.Faction.Name}" : $"Request for {request.KindDef.label}";
        }
    }
}
