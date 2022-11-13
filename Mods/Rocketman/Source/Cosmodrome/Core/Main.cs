using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Build.Utilities;
using RimWorld;
using RocketMan.Tabs;
using Verse;

namespace RocketMan
{
    public static partial class Main
    {
        private static int debugging = 0;

        private const int TickRareMultiplier = 3;
        private const int TickLongerMultiplier = 4;

        private static List<Action> onDefsLoaded;
        private static List<Action> onWorldLoaded;
        private static List<Action> onMapLoaded;
        private static List<Action> onMapComponentsInitializing;
        private static List<Action> onTick;
        private static List<Action> onTickRare;
        private static List<Action> onTickLong;
        private static List<Action> onTickRarer;
        private static List<Action> onTickLonger;

        public static List<Action> onStaticConstructors;
        public static List<Action> onInitialization;
        public static List<Action> onScribe;
        public static List<Action<Map>> onMapDiscarded;
        public static List<Action> onSettingsScribedLoaded;

        public static List<Func<ITabContent>> yieldTabContent;

        public static List<Action> onDebugginEnabled;
        public static List<Action> onDebugginDisabled;

        public static void ReloadActions()
        {
            onDefsLoaded = FunctionsUtility.GetActions<OnDefsLoaded>().ToList();
            onWorldLoaded = FunctionsUtility.GetActions<OnWorldLoaded>().ToList();
            onMapLoaded = FunctionsUtility.GetActions<OnMapLoaded>().ToList();
            onMapComponentsInitializing = FunctionsUtility.GetActions<OnMapComponentsInitializing>().ToList();
            onTick = FunctionsUtility.GetActions<OnTick>().ToList();
            onTickRare = FunctionsUtility.GetActions<OnTickRare>().ToList();
            onTickRarer = FunctionsUtility.GetActions<OnTickRarer>().ToList();
            onTickLong = FunctionsUtility.GetActions<OnTickLong>().ToList();
            onTickLonger = FunctionsUtility.GetActions<OnTickLonger>().ToList();
            onDebugginEnabled = FunctionsUtility.GetActions<OnDebugginEnabled>().ToList();
            onDebugginDisabled = FunctionsUtility.GetActions<OnDebugginDisabled>().ToList();
            yieldTabContent = FunctionsUtility.GetFunctions<YieldTabContent, ITabContent>().ToList();
            onMapDiscarded = FunctionsUtility.GetActions<OnMapDiscarded, Map>().ToList();
            onScribe = FunctionsUtility.GetActions<Main.OnScribe>().ToList();
            onSettingsScribedLoaded = FunctionsUtility.GetActions<Main.OnSettingsScribedLoaded>().ToList();
            onStaticConstructors = FunctionsUtility.GetActions<Main.OnStaticConstructor>().ToList();
            onInitialization = FunctionsUtility.GetActions<Main.OnInitialization>().ToList();
            RocketPrefs.SettingsFields = FieldsUtility.GetFields<Main.SettingsField>().ToArray();
        }

        static Main()
        {
            Log.Message($"<color=orange>ROCKETMAN:</color> Version { RocketAssembliesInfo.Version }");
            Log.Message($"R is 2.3={GenRadial.NumCellsInRadius(2.3f)}, 8.9={GenRadial.NumCellsInRadius(8.9f)} 4.5={GenRadial.NumCellsInRadius(4.5f)}");
            // ----------------------
            // TODO more stylizations.
            // this is used to stylize the log output of rocketman.
            EditWindow_Log_DoMessagesListing_Patch.PatchEditWindow_Log();
            // ----------------------
            // Offical start of the code.                        
            // ----------------------
            // TODO implement compatiblity xml support
            // foreach (var mod in ModsConfig.ActiveModsInLoadOrder)
            // {
            //     Log.Message($"{mod.PackageId}, {mod.Name}, {mod.PackageIdPlayerFacing}");
            // }
        }

        public static void OnStaticConstructorOnStartup()
        {
            onStaticConstructors = FunctionsUtility.GetActions<OnStaticConstructor>().ToList();
            for (var i = 0; i < onStaticConstructors.Count; i++) onStaticConstructors[i].Invoke();
        }

        public static void MapLoaded(Map map)
        {
            for (var i = 0; i < onMapLoaded.Count; i++) onMapLoaded[i].Invoke();
        }

        public static void MapComponentsInitializing(Map map)
        {
            for (var i = 0; i < onMapComponentsInitializing.Count; i++) onMapComponentsInitializing[i].Invoke();
        }

        public static void WorldLoaded()
        {
            for (var i = 0; i < onWorldLoaded.Count; i++) onWorldLoaded[i].Invoke();
        }

        public static void MapDiscarded(Map map)
        {
            for (var i = 0; i < onMapDiscarded.Count; i++) onMapDiscarded[i].Invoke(map);
        }

        public static void DefsLoaded()
        {
            // --------------
            // Used to tell other parts that defs are ready
            RocketStates.DefsLoaded = true;
            // Loading Settings
            Log.Message($"ROCKETMAN: RocketMan settings are stored in <color=red>{RocketEnvironmentInfo.RocketSettingsFilePath}</color>");
            RocketMod.Instance.LoadSettings();
            // Reload action            
            Log.Message("ROCKETMAN: Defs loaded!");
            // Execute the flaged methods
            for (var i = 0; i < onDefsLoaded.Count; i++) onDefsLoaded[i].Invoke();
            // --------------
            // start loading xml data
            XMLParser.ParseXML();
            // --------------
            // load xml data and parse it
            IgnoreMeDatabase.ParsePrepare();
            IncompatibilityHelper.Prepare();            
            // --------------
            // start patching
            RocketPatcher.PatchAll();
            Finder.Rocket.PatchAll();
            Finder.Harmony.PatchAll();
            // --------------
            // rewrite the updated settings
            RocketMod.Instance.WriteSettings();
        }

        private static BucketActionTicker[] _tickers;

        public static void Tick()
        {
            int currentTick = GenTicks.TicksGame;
            // --------------
            // Check if debugging changed
            CheckDebugging();
            // Tick OnTick
            for (int i = 0; i < onTick.Count; i++) onTick[i].Invoke();
            // --------------
            // Initialize buckets
            if (_tickers == null) PrepareTickingBuckets();
            // If this fails we are doomed!
            // --------------
            // Tick buckets
            for (int i = 0; i < _tickers.Length; i++) _tickers[i].Tick(currentTick);
        }

        private static void PrepareTickingBuckets()
        {
            _tickers = new BucketActionTicker[]
            {
                new BucketActionTicker(
                    onTickRare, GenTicks.TickRareInterval),
                new BucketActionTicker(
                    onTickRarer, GenTicks.TickRareInterval * Main.TickRareMultiplier),
                new BucketActionTicker(
                    onTickLong, GenTicks.TickLongInterval),
                new BucketActionTicker(
                    onTickLonger, GenTicks.TickLongInterval * Main.TickLongerMultiplier),
            };
            _tickers = _tickers.Where(t => !t.Empty).ToArray();
        }

        private static void CheckDebugging()
        {
            bool changed = false;
            switch (debugging)
            {
                case 0:
                    if (RocketDebugPrefs.Debug == true)
                        changed = true;
                    else return;
                    break;
                case 1:
                    if (RocketDebugPrefs.Debug == false)
                        return;
                    debugging = 2;
                    changed = true;
                    break;
                case 2:
                    if (RocketDebugPrefs.Debug == true)
                        return;
                    debugging = 1;
                    changed = true;
                    break;
            }
            if (!changed)
            {
                return;
            }
            if (debugging == 1)
            {
                for (var i = 0; i < onDebugginDisabled.Count; i++) onDebugginDisabled[i].Invoke();
            }
            else if (debugging == 2)
            {
                for (var i = 0; i < onDebugginEnabled.Count; i++) onDebugginEnabled[i].Invoke();
            }
        }

        /// <summary>
        /// The flaged function will be called after the <c>DefDatabase</c> are created. It can be used to performe initialization action on startup.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnDefsLoaded : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called every tick.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnTick : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called every 250 ticks.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnTickRare : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called every 750 ticks.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnTickRarer : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called every 2000 ticks.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnTickLong : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called every 8000 ticks.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnTickLonger : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called after the world is loadeed
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnWorldLoaded : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called after a map is loaded
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnMapLoaded : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called after a map is removed
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnMapDiscarded : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called when a map is being initialized.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnMapComponentsInitializing : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called on the <c>OnStaticConstructor</c>
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnStaticConstructor : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called when <c>RocketMan</c> is writing or loading settings.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnScribe : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called after <c>RocketMan</c> settings are loaded on startup. This will run after all <c>Def</c>s are loaded. 
        /// </summary>
        /// <remarks>
        /// The flaged function will get called outside the scribing function. Thus <c>Scribe.mod</c> will be <c>Inactive</c>
        /// </remarks>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnSettingsScribedLoaded : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called as soon as RocketMan is loaded. This is the first thing called post initialization.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnInitialization : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called when <c>RocketMan</c> is looking for UI tabs.        
        /// </summary>
        /// <remarks>
        /// Note: The flaged function must have a return type of <c>ITabContent</c>
        /// </remarks>
        [AttributeUsage(AttributeTargets.Method)]
        public class YieldTabContent : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called when debugging is enabled
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnDebugginEnabled : Attribute
        {
        }

        /// <summary>
        /// The flaged function will be called when debugging is disabled
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class OnDebugginDisabled : Attribute
        {
        }

        /// <summary>
        /// This flage static field so it can be accessed by getting all fields with this atteribute
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class SettingsField : Attribute
        {
            public object warmUpValue;

            public SettingsField(object warmUpValue)
            {
                this.warmUpValue = warmUpValue;
            }
        }

        internal sealed class BucketActionTicker
        {
            private readonly int baseInterval;

            private readonly int bucketInterval;

            private readonly List<Action>[] buckets;

            private readonly bool empty;

            private int cycleIndex = 0;

            public int Interval
            {
                get => baseInterval;
            }

            public bool Empty
            {
                get => empty;
            }

            public BucketActionTicker(IEnumerable<Action> actions, int interval)
            {
                if (actions.EnumerableNullOrEmpty())
                {
                    this.empty = true;
                    return;
                }
                this.baseInterval = interval;
                this.bucketInterval = (int)Math.Max((float)interval / (float)actions.Count(), 1);
                this.buckets = new List<Action>[Math.Min(interval, actions.Count())];
                for (int i = 0; i < buckets.Length; i++)
                {
                    this.buckets[i] = new List<Action>();
                }
                int k = 0;
                foreach (Action action in actions)
                {
                    this.buckets[k].Add(action);
                    k = (k + 1) % this.bucketInterval;
                }
                this.Log_BucketData();
            }

            public void Tick(int currentTick)
            {
                if (empty || currentTick % bucketInterval != 0)
                {
                    return;
                }
                foreach (Action action in this.buckets[this.cycleIndex])
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception er)
                    {
                        this.Log_Error(er);
                    }
                }
                this.cycleIndex = (cycleIndex + 1) % this.buckets.Length;
            }

            private void Log_Error(Exception er)
            {
                Log.Error($"Created ticker bucket: BaseInterval={baseInterval} BucketInterval={bucketInterval} {er}");
                RocketMan.Logger.Debug($"", exception: er);
            }

            private void Log_BucketData()
            {
                int j = 0;
                RocketMan.Logger.Debug($"Created ticker bucket: BaseInterval={baseInterval} BucketInterval={bucketInterval}");
                foreach (List<Action> bucket in buckets)
                {
                    RocketMan.Logger.Debug($"Bucket[{j++}].Count = {bucket.Count}");
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    internal static class Main_StaticConstructor
    {
        static Main_StaticConstructor()
        {
            Main.OnStaticConstructorOnStartup();
        }
    }
}