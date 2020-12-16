using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Analyzer.Performance
{
    class H_SectionCtor : PerfPatch
    {
        public static bool Enabled = true;
        public static List<Type> sections = new List<Type>();
        public override string Name => "temp.section";
        public override PerformanceCategory Category => PerformanceCategory.Optimizes;


        public override void OnEnabled(Harmony harmony)
        {
            sections = typeof(SectionLayer).AllSubclassesNonAbstract().ToList();
            harmony.Patch(AccessTools.Constructor(typeof(Section), new Type[] { typeof(IntVec3), typeof(Map) }), prefix: new HarmonyMethod(typeof(H_SectionCtor), nameof(H_SectionCtor.CtorReplacement)));
        }

        public static bool CtorReplacement(Section __instance, IntVec3 sectCoords, Map map)
        {
            __instance.botLeft = sectCoords * 17;
            __instance.map = map;
            __instance.layers = new List<SectionLayer>();

            foreach (var t in sections)
            {
                __instance.layers.Add((SectionLayer)Activator.CreateInstance(t, __instance));
            }

            return false;
        }
    }
}
