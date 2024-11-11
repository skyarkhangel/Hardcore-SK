using HarmonyLib;
using Verse;
using RimWorld;

namespace ZB333ZB_Patches.Core
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("ZB333ZB.Patches");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Dialog_ModSettings))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new[] { typeof(Mod) })]
    public static class Dialog_ModSettings_Constructor_Patch
    {
        public static bool Prefix(Mod mod)
        {
            if (mod is ModSettingsMod ourMod)
            {
                var window = new SettingsWindow(ourMod, ModSettingsMod.Settings);
                Find.WindowStack.Add(window);
                return false;
            }
            return true;
        }
    }
}
