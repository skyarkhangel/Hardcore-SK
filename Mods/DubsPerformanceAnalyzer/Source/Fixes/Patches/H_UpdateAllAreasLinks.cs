using HarmonyLib;
using Verse;

namespace Analyzer.Fixes
{
    internal class H_UpdateAllAreasLinks : Fix
    {
        public static bool Active = false;
        public override string Name => "fix.fixmodremoval";

        public override void OnGameInit(Game g, Harmony h)
        {
            var skiff = AccessTools.Method(typeof(AreaManager), nameof(AreaManager.UpdateAllAreasLinks));
            h.Patch(skiff, new HarmonyMethod(typeof(H_UpdateAllAreasLinks), nameof(Prefix)));
        }

        public static bool Prefix(AreaManager __instance)
        {
            if (!Active) return true;
            __instance.areas.RemoveAll(x => x == null || x.GetType().IsAbstract);
            return true;
        }
    }
}