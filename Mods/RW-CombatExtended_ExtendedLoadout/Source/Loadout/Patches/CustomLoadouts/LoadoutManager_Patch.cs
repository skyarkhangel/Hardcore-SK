using HarmonyLib;

namespace CombatExtended.ExtendedLoadout;

[HarmonyPatch(typeof(LoadoutManager))]
public static class LoadoutManager_Patch
{
    static bool Prepare() => ExtendedLoadoutMod.Instance.useMultiLoadouts;

    [HarmonyPatch(nameof(LoadoutManager.RemoveLoadout))]
    [HarmonyPostfix]
    public static void RemoveLoadout(Loadout loadout)
    {
        LoadoutMulti_Manager.RemoveLoadout(loadout);
    }

    [HarmonyPatch(nameof(LoadoutManager.ExposeData))]
    [HarmonyPostfix]
    public static void ExposeData(LoadoutManager __instance)
    {
        LoadoutMulti_Manager.ExposeData(__instance);
    }
}