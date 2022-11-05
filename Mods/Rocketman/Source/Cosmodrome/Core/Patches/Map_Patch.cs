using System;
using HarmonyLib;
using Verse;

namespace RocketMan.Patches
{
    [RocketStartupPatch(typeof(Map), nameof(Map.FinalizeInit))]
    public static class Map_FinalizeInit_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static void Postfix(Map __instance)
        {
            Main.MapLoaded(__instance);
        }
    }

    [RocketStartupPatch(typeof(Map), nameof(Map.ConstructComponents))]
    public static class Map_ConstructComponents_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static void Postfix(Map __instance)
        {
            Main.MapComponentsInitializing(__instance);
        }
    }
}
