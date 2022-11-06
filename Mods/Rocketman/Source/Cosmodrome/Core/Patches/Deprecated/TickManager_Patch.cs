using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Verse;


namespace RocketMan.Patches
{
    public class TickManager_Patch
    {
        [RocketPatch(typeof(TickManager), nameof(TickManager.DoSingleTick))]
        public static class TickManager_DoSingleTick_Context_Patch
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [HarmonyPriority(Priority.First)]
            public static void Prefix()
            {
                RocketStates.Context = ContextFlag.Ticking;
            }
        }

        [RocketPatch(typeof(TickManager), nameof(TickManager.TickRateMultiplier), MethodType.Getter)]
        public static class TickManager_TickRateMultiplier_Patch
        {
            public static bool Prepare() => !RocketCompatibilityInfo.SmartSpeedLoaded;

            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(ref float __result)
            {
                if (true
                    && RocketPrefs.Enabled
                    && RocketDebugPrefs.Debug150MTPS
                    && RocketDebugPrefs.Debug)
                {
                    __result = 150f;
                    return false;
                }
                return true;
            }
        }

        [RocketPatch(typeof(TickManager), nameof(TickManager.Notify_GeneratedPotentiallyHostileMap))]
        public static class TickManager_Notify_GeneratedPotentiallyHostileMap_Patch
        {
            public static bool Prepare() => !RocketCompatibilityInfo.SmartSpeedLoaded;

            [HarmonyPriority(int.MinValue)]
            public static bool Prefix()
            {
                if (true
                    && Prefs.DevMode
                    && RocketDebugPrefs.Debug
                    && RocketPrefs.DisableForcedSlowdowns)
                    return false;
                return true;
            }
        }

        [RocketPatch(typeof(TickManager), nameof(TickManager.TickManagerUpdate))]
        public static class TickManager_TickManagerUpdate_Patch
        {
            public static bool Prepare() => !RocketCompatibilityInfo.SmartSpeedLoaded;

            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(TickManager __instance)
            {
                if (RocketPrefs.Enabled && RocketDebugPrefs.Debug && RocketStates.SingleTickIncrement)
                {
                    if (RocketStates.SingleTickIncrement && RocketStates.SingleTickLeft > 0)
                    {
                        if (__instance.Paused)
                        {
                            __instance.TogglePaused();
                        }
                        RocketStates.SingleTickLeft = Math.Max(RocketStates.SingleTickLeft - 1, 0);
                        return true;
                    }
                    if (!__instance.Paused)
                    {
                        __instance.Pause();
                    }
                    return false;
                }
                return true;
            }
        }
    }
}

