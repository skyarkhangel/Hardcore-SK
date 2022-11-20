using HarmonyLib;
using Hospitality.Utilities;
using Verse;

namespace Hospitality.Patches
{
    internal static class Thing_SpawnSetupPatch
    {
        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("SpawnSetup")]
        public class SpawnSetup
        {
            public static void Postfix(Thing __instance)
            {
                ThingCache.TryRegisterNewThing(__instance);
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("DeSpawn")]
        public class DeSpawn
        {
            public static bool Prefix(Thing __instance)
            {
                ThingCache.TryDeregister(__instance, __instance.Map);
                return true;
            }
        }
    }
}
