using HarmonyLib;
using Verse;

namespace RocketMan.Patches
{
    [RocketPatch(typeof(TimeSlower), nameof(TimeSlower.SignalForceNormalSpeed))]
    public class TimeSlower_SignalForceNormalSpeed_Patch
    {
        public static bool Prepare() => !RocketCompatibilityInfo.SmartSpeedLoaded;

        [HarmonyPriority(int.MinValue)]
        public static bool Prefix()
        {
            if (!RocketPrefs.Enabled)
                return true;
            if (!RocketPrefs.DisableForcedSlowdowns)
                return true;
            return false;
        }
    }

    [RocketPatch(typeof(TimeSlower), nameof(TimeSlower.SignalForceNormalSpeedShort))]
    public class TimeSlower_SignalForceNormalSpeedShort_Patch
    {
        public static bool Prepare() => !RocketCompatibilityInfo.SmartSpeedLoaded;

        [HarmonyPriority(int.MinValue)]
        public static bool Prefix()
        {
            if (!RocketPrefs.Enabled)
                return true;
            if (!RocketPrefs.DisableForcedSlowdowns)
                return true;
            return false;
        }
    }
}