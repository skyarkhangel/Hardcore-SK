using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RocketMan
{
    public partial class RocketMod : Mod
    {
        public static RocketSettings Settings;

        public static RocketMod Instance;

        public static Vector2 scrollPositionStatSettings = Vector2.zero;

        public RocketMod(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(() =>
            {
                Main.DefsLoaded();
            }, "RocketMan.RocketMan", doAsynchronously: false, exceptionHandler: null, showExtraUIInfo: true);

            Finder.Mod = Instance = this;
            Finder.ModContentPack = content;
            if (!Directory.Exists(RocketEnvironmentInfo.CustomConfigFolderPath))
            {
                Directory.CreateDirectory(RocketEnvironmentInfo.CustomConfigFolderPath);
                Log.Message($"ROCKETMAN: Created RocketMan config folder at <color=orange>{RocketEnvironmentInfo.CustomConfigFolderPath}</color>");
            }
            Logger.Initialize();
            // Patch all core functions
            RocketStartupPatcher.PatchAll();
            // Program start here
            Finder.PluginsLoader = new RocketPluginsLoader();
            try
            {
                foreach (Assembly assembly in Finder.PluginsLoader.LoadAll())
                {
                    RocketAssembliesInfo.Assemblies.Add(assembly);
                    if (!content.assemblies.loadedAssemblies.Any(a => a.GetName().Name == assembly.GetName().Name))
                        content.assemblies.loadedAssemblies.Add(assembly);
                    Log.Message($"<color=orange>ROCKETMAN</color>: Loaded <color=red>{assembly.FullName}</color>");
                }
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETMAN: loading plugin failed {er.Message}:{er.StackTrace}");
                Logger.Debug("Loading plugins failed", exception: er);
            }
            finally
            {
                RocketAssembliesInfo.Assemblies.AddRange(RocketAssembliesInfo.RocketManAssembliesInAppDomain);
                foreach (Assembly assembly in RocketAssembliesInfo.Assemblies)
                    Logger.Debug($"Found in AppDomain after loading assembly {assembly.FullName}", file: "Assemblies.log");
                Main.ReloadActions();
                foreach (var action in Main.onInitialization)
                    action.Invoke();
            }
        }

        public override string SettingsCategory()
        {
            return "RocketMan";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            DoSettings(inRect);
            WriteSettings();
            GUIUtility.ClearGUIState();
        }

        private static readonly Listing_Collapsible.Group_Collapsible group = new Listing_Collapsible.Group_Collapsible();

        private static readonly Listing_Collapsible collapsible_general = new Listing_Collapsible();

        private static readonly Listing_Collapsible collapsible_junk = new Listing_Collapsible(group);

        private static readonly Listing_Collapsible collapsible_speed = new Listing_Collapsible(group);

        private static readonly Listing_Collapsible collapsible_genMap = new Listing_Collapsible(group);

        private static readonly Listing_Collapsible collapsible_other = new Listing_Collapsible(group);

        private static readonly Listing_Collapsible collapsible_GlowGrid = new Listing_Collapsible(group);

        private static readonly Listing_Collapsible collapsible_debug = new Listing_Collapsible(group);

        private static readonly Listing_Collapsible collapsible_experimental = new Listing_Collapsible(group);

        private static bool guiGroupCreated = false;

        public static void DoSettings(Rect inRect, bool doStats = true, Action<Listing_Standard> extras = null)
        {
            if (!guiGroupCreated)
            {
                guiGroupCreated = true;

                collapsible_junk.Group = group;
                group.Register(collapsible_junk);

                collapsible_other.Group = group;
                group.Register(collapsible_other);

                collapsible_debug.Group = group;
                group.Register(collapsible_debug);

                collapsible_GlowGrid.Group = group;
                group.Register(collapsible_GlowGrid);

                collapsible_experimental.Group = group;
                group.Register(collapsible_experimental);                
            }
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                collapsible_general.Expanded = true;
                collapsible_general.Begin(inRect, KeyedResources.RocketMan_Settings, drawIcon: false, drawInfo: false);

                if (collapsible_general.CheckboxLabeled(KeyedResources.RocketMan_Enable, ref RocketPrefs.Enabled))
                {
                    ResetRocketDebugPrefs();
                }
                if (collapsible_general.CheckboxLabeled("RocketMan.ShowIcon".Translate(), ref RocketPrefs.MainButtonToggle, "RocketMan.ShowIcon.Description".Translate()))
                {
                    MainButtonDef mainButton_WindowDef = DefDatabase<MainButtonDef>.GetNamed("RocketWindow", errorOnFail: false);
                    if (mainButton_WindowDef != null)
                    {
                        mainButton_WindowDef.buttonVisible = RocketPrefs.MainButtonToggle;
                        string state = RocketPrefs.MainButtonToggle ? "shown" : "hidden";
                        Log.Message($"ROCKETMAN: <color=red>MainButton</color> is now {state}!");
                    }
                }
                collapsible_general.CheckboxLabeled("RocketMan.ProgressBar".Translate(), ref RocketPrefs.ShowWarmUpPopup, "RocketMan.ProgressBar.Description".Translate());
                collapsible_general.End(ref inRect);
                inRect.yMin += 5;

                if (Find.World != null)
                {
                    WorldInfoComponent infoComponent = Find.World.GetComponent<WorldInfoComponent>();
                    collapsible_genMap.Begin(inRect, KeyedResources.RocketMan_GenMapSize);
                    collapsible_genMap.Label(KeyedResources.RocketMan_GenMapSize_Text);
                    collapsible_genMap.Line(1);
                    collapsible_genMap.Label(KeyedResources.RocketMan_GenMapSize_Note);
                    collapsible_genMap.Columns(18, new Action<Rect>[]{
                        (rect)=>{
                            GUIFont.Anchor = TextAnchor.MiddleLeft;
                            float a = infoComponent.InitialMapWidth;
                            string buffer = $"{a}";
                            Widgets.Label(rect, KeyedResources.RocketMan_GenMapSize_Width);
                            Widgets.TextFieldNumeric(rect.RightHalf(), ref a, ref buffer, 0, 1000);
                            if(infoComponent.InitialMapWidth != a)
                            {
                                infoComponent.InitialMapWidth = (int)a;
                                infoComponent.useCustomMapSizes = true;
                            }
                        },
                        (rect)=>{
                            GUIFont.Anchor = TextAnchor.MiddleLeft;
                            float a = infoComponent.InitialMapHeight;
                            string buffer = $"{a}";
                            Widgets.Label(rect.MoveTopLeftCorner(25f, 0), KeyedResources.RocketMan_GenMapSize_Height);
                            Widgets.TextFieldNumeric(rect.RightHalf(), ref a, ref buffer, 0, 1000);
                            if(infoComponent.InitialMapHeight != a)
                            {
                                infoComponent.InitialMapHeight = (int)a;
                                infoComponent.useCustomMapSizes = true;
                            }
                        }
                    }, useMargins: true);
                    collapsible_genMap.End(ref inRect);
                    inRect.yMin += 5;
                }

                if (RocketPrefs.Enabled)
                {
                    //collapsible_speed.Begin(inRect, "RocketMan.GameSpeed".Translate());
                    //collapsible_speed.CheckboxLabeled("RocketMan.DisableForcedSlowdowns".Translate(), ref RocketPrefs.DisableForcedSlowdowns, "RocketMan.DisableForcedSlowdowns.Description".Translate());
                    //collapsible_speed.CheckboxLabeled(KeyedResources.RocketMan_ProgressBar_Pause, ref RocketPrefs.PauseAfterWarmup);
                    //collapsible_speed.End(ref inRect);
                    //inRect.yMin += 5;

                    if (RocketEnvironmentInfo.IsDevEnv)
                    {
                        collapsible_junk.Begin(inRect, "RocketMan.Junk".Translate());
                        collapsible_junk.CheckboxLabeled("RocketMan.CorpseRemoval".Translate(), ref RocketPrefs.CorpsesRemovalEnabled, "RocketMan.CorpseRemoval.Description".Translate());
                        collapsible_junk.End(ref inRect);
                        inRect.yMin += 5;
                    }

                    //collapsible_other.Begin(inRect, "RocketMan.StatCacheSettings".Translate());
                    //collapsible_other.CheckboxLabeled("RocketMan.Adaptive".Translate(), ref RocketPrefs.Learning, "RocketMan.Adaptive.Description".Translate());
                    //collapsible_other.CheckboxLabeled("RocketMan.AdaptiveAlert.Label".Translate(), ref RocketPrefs.LearningAlertEnabled, "RocketMan.AdaptiveAlert.Description".Translate());
                    //collapsible_other.CheckboxLabeled("RocketMan.EnableGearStatCaching".Translate(), ref RocketPrefs.StatGearCachingEnabled);
                    //collapsible_other.End(ref inRect);
                    //inRect.yMin += 5;

                    collapsible_GlowGrid.Begin(inRect, KeyedResources.Proton_GlowGrid);
                    collapsible_GlowGrid.Label(KeyedResources.Proton_GlowGrid_Description);
                    collapsible_GlowGrid.Line(1);                   
                    collapsible_GlowGrid.CheckboxLabeled(KeyedResources.Proton_GlowGrid_Enable, ref RocketPrefs.GlowGridOptimization);                    
                    // if (RocketPrefs.GlowGridOptimization)
                    // {
                    // collapsible_GlowGrid.Gap(1);
                    // collapsible_GlowGrid.Label(KeyedResources.Proton_GlowGrid_Limiter_Tip);
                    // collapsible_GlowGrid.CheckboxLabeled(KeyedResources.Proton_GlowGrid_Limiter, ref RocketPrefs.GlowGridOptimizationLimiter);
                    // }
                    collapsible_GlowGrid.End(ref inRect);
                    inRect.yMin += 5;

                    if (Prefs.DevMode || RocketEnvironmentInfo.IsDevEnv)
                    {
                        collapsible_experimental.Begin(inRect, KeyedResources.RocketMan_Experimental);                        
                        // if (RocketEnvironmentInfo.IsDevEnv)
                        // {
                        //    collapsible_experimental.CheckboxLabeled(KeyedResources.RocketMan_TranslationCaching, ref RocketPrefs.TranslationCaching);
                        //    collapsible_experimental.Line(1);
                        // }
                        // collapsible_experimental.Label(KeyedResources.RocketMan_Experimental_Description);
                        bool devKeyEnabled = File.Exists(RocketEnvironmentInfo.DevKeyFilePath);
                        if (collapsible_experimental.CheckboxLabeled(KeyedResources.RocketMan_Experimental_OptInBeta, ref devKeyEnabled))
                        {
                            if (!devKeyEnabled && File.Exists(RocketEnvironmentInfo.DevKeyFilePath))
                            {
                                File.Delete(RocketEnvironmentInfo.DevKeyFilePath);
                                RocketPrefs.TimeDilationColonists = false;
                            }
                            if (devKeyEnabled && !File.Exists(RocketEnvironmentInfo.DevKeyFilePath))
                                File.WriteAllText(RocketEnvironmentInfo.DevKeyFilePath, "enabled");
                        }
                        collapsible_experimental.Line(1);
                        collapsible_experimental.CheckboxLabeled(KeyedResources.RocketMan_FixBeauty, ref RocketPrefs.FixBeauty, KeyedResources.RocketMan_FixBeauty_Tip);
                        collapsible_experimental.End(ref inRect);
                        inRect.yMin += 5;
                    }
                    collapsible_debug.Begin(inRect, "Debugging options");

                    if (collapsible_debug.CheckboxLabeled("RocketMan.Debugging".Translate(), ref RocketDebugPrefs.Debug, "RocketMan.Debugging.Description".Translate())
                    && !RocketDebugPrefs.Debug)
                    {
                        ResetRocketDebugPrefs();
                    }
                    if (RocketDebugPrefs.Debug)
                    {
                        collapsible_debug.Line(1);
                        //collapsible_debug.CheckboxLabeled("Enable Stat Logging (Will kill performance)", ref RocketDebugPrefs.StatLogging);
                        collapsible_debug.CheckboxLabeled("Enable GlowGrid flashing", ref RocketDebugPrefs.DrawGlowerUpdates);
                        collapsible_debug.CheckboxLabeled("Enable GlowGrid refresh", ref RocketPrefs.EnableGridRefresh);
                        collapsible_debug.Gap();
                    }
                    collapsible_debug.End(ref inRect);
                }
            });
        }

        public static void ResetRocketDebugPrefs()
        {
            RocketDebugPrefs.Debug = false;
            RocketDebugPrefs.Debug150MTPS = false;
            RocketDebugPrefs.LogData = false;
            RocketDebugPrefs.StatLogging = false;
            RocketDebugPrefs.FlashDilatedPawns = false;
            RocketDebugPrefs.AlwaysDilating = false;
            RocketPrefs.EnableGridRefresh = false;
            RocketPrefs.RefreshGrid = false;
            RocketStates.SingleTickIncrement = false;
        }
    }
}