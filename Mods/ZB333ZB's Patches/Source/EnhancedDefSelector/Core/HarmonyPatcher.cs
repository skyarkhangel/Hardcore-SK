using HarmonyLib;
using Verse;

namespace EnhancedDefSelector.Core
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatcher
    {
        static HarmonyPatcher()
        {
            new Harmony("com.ZB333ZB.EnhancedScenarioDefsSelector").PatchAll();
        }
    }
}