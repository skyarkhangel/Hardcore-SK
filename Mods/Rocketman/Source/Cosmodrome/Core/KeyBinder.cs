using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RocketMan
{
    public static class KeyBinder
    {
        private static bool success = false;

        private static KeyBindingDef ToggleRocketMan;

        private static KeyBindingDef ToggleAlerts;

        private static KeyBindingDef ToggleDebugging;

        private static KeyBindingDef ToggleSlowdowns;

        private static MethodBase mtarget = AccessTools.Method(typeof(UIRoot_Play), nameof(UIRoot_Play.UIRootOnGUI));

        private static MethodBase mOnGUI = AccessTools.Method(typeof(KeyBinder), nameof(KeyBinder.OnGUI));

        [Main.OnDefsLoaded]
        public static void Start()
        {
            try
            {
                ToggleRocketMan = KeyBindingDef.Named("RocketKeyBindingDisable");

                ToggleAlerts = KeyBindingDef.Named("RocketKeyToggleAlerts");

                ToggleDebugging = KeyBindingDef.Named("RocketKeyToggleDebugging");

                ToggleSlowdowns = KeyBindingDef.Named("RocketKeyToggleForcedSlowdowns");

                Finder.Harmony.Patch(mtarget, postfix: new HarmonyMethod(mOnGUI as MethodInfo));

                Log.Message("ROCKETMAN: Patched KeyBinder!");

                success = true;
            }
            catch (Exception er)
            {
                Logger.Debug("ROCKETMAN: Failed to initialize the KeyBinder", exception: er);
            }
        }

        private static void OnGUI()
        {
            if (!success)
            {
                return;
            }
            try
            {
                if (ToggleRocketMan.KeyDownEvent)
                {
                    RocketPrefs.Enabled = !RocketPrefs.Enabled;
                }
                if (ToggleAlerts.KeyDownEvent)
                {
                    RocketPrefs.DisableAllAlert = !RocketPrefs.DisableAllAlert;
                    if (RocketPrefs.DisableAllAlert)
                    {
                        List<Alert> alerts = (Find.UIRoot as UIRoot_Play)?.alerts?.AllAlerts;
                        if (alerts != null)
                        {
                            foreach (Alert alert in alerts)
                            {
                                alert.cachedActive = false;
                                alert.cachedLabel = string.Empty;
                            }
                        }
                    }
                }
                if (ToggleDebugging.KeyDownEvent)
                {
                    RocketDebugPrefs.Debug = !RocketDebugPrefs.Debug;
                }
                if (ToggleSlowdowns.KeyDownEvent)
                {
                    RocketPrefs.DisableForcedSlowdowns = !RocketPrefs.DisableForcedSlowdowns;
                }
            }
            catch (Exception er)
            {
                Logger.Debug("ROCKETMAN: KeyBinder failed!", exception: er);
            }
        }
    }
}
