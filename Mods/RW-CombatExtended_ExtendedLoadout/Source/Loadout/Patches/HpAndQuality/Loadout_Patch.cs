using HarmonyLib;
using Verse;

namespace CombatExtended.ExtendedLoadout;

[HarmonyPatch(typeof(Loadout))]
public static class Loadout_Patch
{
    static bool Prepare() => ExtendedLoadoutMod.Instance.useHpAndQualityInLoadouts;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Loadout.Copy), typeof(Loadout))]
    private static void Copy(Loadout source, Loadout __result)
    {
        __result.CopyLoadoutExtended(source);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Loadout.ExposeData))]
    private static void ExposeData(Loadout __instance)
    {
        __instance.Extended().ExposeData();
    }
}