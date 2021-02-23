using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.update.mapdrawer", Category.Update)]
    internal class H_DrawSection
    {
        public static bool Active = false;

        [Setting("By Def")] public static bool ByDef = false;

        public static void ProfilePatch()
        {
            Modbase.Harmony.Patch(AccessTools.Method(typeof(Section), nameof(Section.DrawSection)),
                new HarmonyMethod(typeof(H_DrawSection), "Prefix"));
        }

        public static bool Prefix(MethodBase __originalMethod, Section __instance, bool drawSunShadowsOnly)
        {
            if (!Active)
            {
                return true;
            }

            var count = __instance.layers.Count;
            for (var i = 0; i < count; i++)
            {
                if (!drawSunShadowsOnly || __instance.layers[i] is SectionLayer_SunShadows)
                {
                    string Namer()
                    {
                        var n = __instance.layers[i].GetType().ToString();
                        return n;
                    }

                    var name = __instance.layers[i].GetType().Name;

                    var prof = ProfileController.Start(name, Namer, __instance.layers[i].GetType(), null, null, __originalMethod);
                    __instance.layers[i].DrawLayer();
                    prof.Stop();
                }
            }

            if (!drawSunShadowsOnly && DebugViewSettings.drawSectionEdges)
            {
                GenDraw.DrawLineBetween(__instance.botLeft.ToVector3(),
                    __instance.botLeft.ToVector3() + new Vector3(0f, 0f, 17f));
                GenDraw.DrawLineBetween(__instance.botLeft.ToVector3(),
                    __instance.botLeft.ToVector3() + new Vector3(17f, 0f, 0f));
            }

            return false;
        }
    }
}