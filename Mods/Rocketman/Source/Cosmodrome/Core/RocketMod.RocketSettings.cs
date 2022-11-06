using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Verse;

namespace RocketMan
{
    public partial class RocketMod : Mod
    {
        public void LoadSettings()
        {
            bool settingsFound = false;
            try
            {
                if (File.Exists(RocketEnvironmentInfo.RocketSettingsFilePath))
                {
                    Scribe.loader.InitLoading(RocketEnvironmentInfo.RocketSettingsFilePath);
                    try
                    {
                        Scribe_Deep.Look(ref RocketMod.Settings, "ModSettings");
                        settingsFound = RocketMod.Settings != null;
                        if (RocketMod.Settings == null)
                            RocketMod.Settings = new RocketSettings();
                    }
                    catch (Exception er)
                    {
                        Log.Error($"ROCKETMAN: Error while scribing settings {er}");
                        Logger.Debug("Error while scribing settings", exception: er);
                    }
                    finally
                    {
                        Scribe.loader.FinalizeLoading();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"ROCKETMAN: Caught exception while loading mod settings data for {Content.FolderName}. Generating fresh settings. The exception was: {ex.ToString()}");
                RocketMod.Settings = null;
            }
            if (RocketMod.Settings == null)
            {
                RocketMod.Settings = new RocketSettings();
            }
            if (!settingsFound)
            {
                WriteSettings();
            }
            foreach (var action in Main.onSettingsScribedLoaded)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception er)
                {
                    Log.Error($"ROCKETMAN: Error in post srcibe {action} with error {er}");
                    Logger.Debug("Error in post srcibe", exception: er);
                }
            }
        }

        public override void WriteSettings()
        {
            if (RocketPrefs.WarmingUp && !(WarmUpMapComponent.current?.Finished ?? true))
            {
                WarmUpMapComponent.current.AbortWarmUp();
            }
            Scribe.saver.InitSaving(RocketEnvironmentInfo.RocketSettingsFilePath, "SettingsBlock");
            try
            {
                Scribe_Deep.Look(ref RocketMod.Settings, "ModSettings");
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETMAN: Error while scribing settings {er}");
                Logger.Debug("Error while scribing settings", exception: er);
            }
            finally
            {
                Scribe.saver.FinalizeSaving();
            }
        }

        public class RocketSettings : IExposable
        {
            public void ExposeData()
            {
                ScribeRocketPrefs();
                ScribeExtras();
                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    UpdateExceptions();
                }
                RocketPrefs.TimeDilationCaravans = false;
            }

            private void ScribeRocketPrefs()
            {
                string version = RocketAssembliesInfo.Version;
                bool upgrade = false;
                Scribe_Values.Look(ref version, "version", null, forceSave: true);
                if(version != RocketAssembliesInfo.Version && !RocketEnvironmentInfo.IsDevEnv)
                {
                    upgrade = true;
                    version = RocketAssembliesInfo.Version;
                }

                Scribe_Values.Look(ref RocketDebugPrefs.Debug, "debug", false);
                Scribe_Values.Look(ref RocketPrefs.Enabled, "enabled", true);
                Scribe_Values.Look(ref RocketPrefs.Learning, "learning", true);
                Scribe_Values.Look(ref RocketPrefs.FixBeauty, "FixBeauty", false);
                Scribe_Values.Look(ref RocketPrefs.StatGearCachingEnabled, "statGearCachingEnabled", true);
                Scribe_Values.Look(ref RocketPrefs.ShowWarmUpPopup, "showWarmUpPopup", true);
                Scribe_Values.Look(ref RocketPrefs.PauseAfterWarmup, "pauseAfterWarmup", false);
                Scribe_Values.Look(ref RocketPrefs.AlertThrottling, "alertThrottling", true);
                Scribe_Values.Look(ref RocketPrefs.DisableAllAlert, "disableAllAlert", false);
                Scribe_Values.Look(ref RocketPrefs.LearningAlertEnabled, "learningAlertEnabled", true);
                Scribe_Values.Look(ref RocketPrefs.TimeDilation, "timeDilation", true);
                Scribe_Values.Look(ref RocketPrefs.TimeDilationWildlife, "TimeDilationWildlife", true);
                                
                if (!upgrade)
                {                    
                    Scribe_Values.Look(ref RocketPrefs.TimeDilationColonists, "TimeDilationColonists", false);
                    Scribe_Values.Look(ref RocketPrefs.TimeDilationFire, "TimeDilationFire", false);
                    Scribe_Values.Look(ref RocketPrefs.TimeDilationCaravans, "timeDilationCaravan", false);
                    Scribe_Values.Look(ref RocketPrefs.TimeDilationVisitors, "timeDilationVisitors", false);
                    Scribe_Values.Look(ref RocketPrefs.TimeDilationWorldPawns, "timeDilationWorldPawns", false);
                    Scribe_Values.Look(ref RocketPrefs.TimeDilationColonyAnimals, "timeDialationColonyAnimals", false);
                    Scribe_Values.Look(ref RocketPrefs.TimeDilationCriticalHediffs, "timeDilationCriticalHediffs", false);
                    Scribe_Values.Look(ref RocketPrefs.CorpsesRemovalEnabled, "corpsesRemovalEnabled", false);
                }

                Scribe_Values.Look(ref RocketPrefs.MainButtonToggle, "mainButtonToggle", true);                
                Scribe_Values.Look(ref RocketPrefs.DisableForcedSlowdowns, "disableForcedSlowdowns", false);
                Scribe_Values.Look(ref RocketPrefs.TranslationCaching, "translationCaching", false);
                Scribe_Values.Look(ref RocketPrefs.GlowGridOptimization, "GlowGridOptimization", true);
                Scribe_Values.Look(ref RocketPrefs.GlowGridOptimizationLimiter, "GlowGridOptimizationLimiter", true);               
                
                if (!RocketEnvironmentInfo.IsDevEnv)
                    RocketPrefs.TimeDilationColonists = false;
            }

            private void ScribeExtras()
            {
                foreach (var action in Main.onScribe)
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception er)
                    {
                        Log.Error($"ROCKETMAN: Error scribing settings with mod {Scribe.mode} in action {action} with error {er}");
                        Logger.Debug($"Error scribing settings with mod {Scribe.mode}", exception: er);
                    }
                }
            }
        }
    }
}
