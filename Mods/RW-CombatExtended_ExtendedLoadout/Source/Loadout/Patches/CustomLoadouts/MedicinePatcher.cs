using System;
using HarmonyLib;
using static CombatExtended.LoadoutGenericDef;

namespace CombatExtended.ExtendedLoadout;

[HarmonyPatch(typeof(LoadoutGenericDef))]
[HarmonyPatch("LoadoutGenericDef")] // if possible use nameof() here
class MedicinePatcher
{
    static AccessTools.FieldRef<LoadoutGenericDef, bool> isRunningRef =
        AccessTools.FieldRefAccess<LoadoutGenericDef, bool>("LoadoutGenericDef");

    static bool Prefix(LoadoutGenericDef __instance, ref int ___counter)
    {
        isRunningRef(__instance) = true;
        if (___counter > 100)
            return false;
        ___counter = 0;
        return true;
    }

    static void Postfix(ref int __result)
    {
        __result *= 2;
    }
}
