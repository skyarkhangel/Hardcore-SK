using System.Collections.Generic;
using HarmonyLib;

namespace CombatExtended.ExtendedLoadout;

//[HarmonyPatch(typeof(Loadout))]
public static class LoadoutProxy_Patch
{
    static bool Prepare() => ExtendedLoadoutMod.Instance.useMultiLoadouts;

    public static void Patch()
    {
        _ = ExtendedLoadoutMod.Harmony.Patch(AccessTools.PropertyGetter(typeof(Loadout), nameof(Loadout.SlotCount)), prefix: new HarmonyMethod(typeof(LoadoutProxy_Patch), nameof(SlotCount)));
        _ = ExtendedLoadoutMod.Harmony.Patch(AccessTools.PropertyGetter(typeof(Loadout), nameof(Loadout.Slots)), prefix: new HarmonyMethod(typeof(LoadoutProxy_Patch), nameof(Slots)));
    }

    public static void Unpatch()
    {
        ExtendedLoadoutMod.Harmony.Unpatch(AccessTools.PropertyGetter(typeof(Loadout), nameof(Loadout.SlotCount)), HarmonyPatchType.Prefix, ExtendedLoadoutMod.HarmonyId);
        ExtendedLoadoutMod.Harmony.Unpatch(AccessTools.PropertyGetter(typeof(Loadout), nameof(Loadout.Slots)), HarmonyPatchType.Prefix, ExtendedLoadoutMod.HarmonyId);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Loadout.SlotCount), MethodType.Getter)]
    public static bool SlotCount(Loadout __instance, ref int __result)
    {
        if (__instance is Loadout_Multi loadout)
        {
            __result = loadout.SlotCount;
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Loadout.Slots), MethodType.Getter)]
    public static bool Slots(Loadout __instance, ref List<LoadoutSlot> __result)
    {
        if (__instance is Loadout_Multi loadout)
        {
            __result = loadout.Slots;
            return false;
        }
        return true;
    }
}