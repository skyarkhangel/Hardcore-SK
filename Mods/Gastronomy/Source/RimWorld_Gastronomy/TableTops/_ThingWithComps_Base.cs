using System.Runtime.CompilerServices;
using HarmonyLib;
using Verse;

namespace Gastronomy.TableTops
{
    /// <summary>
    /// So we can call the base method on ThingWithComps and avoid whatever overrides it
    /// </summary>
    public class _ThingWithComps_Base
    {
        [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.SpawnSetup))]
        public class SpawnSetup
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void Base(ThingWithComps instance, Map map, bool respawningAfterLoad) { } // Can't remove unused parameters
        }

        [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.Destroy))]
        public class Destroy
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void Base(ThingWithComps instance, DestroyMode mode) { }
        }

        [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.DeSpawn))]
        public class DeSpawn
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void Base(ThingWithComps instance, DestroyMode mode) { }
        }
    }
}
