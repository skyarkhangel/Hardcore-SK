using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Verse;

namespace PeteTimesSix.SimpleSidearms
{
    public class SimpleSidearms : Mod
    {
        public static SimpleSidearms_Settings Settings { get; internal set; }
        public static SimpleSidearms ModSingleton { get; private set; }

        public SimpleSidearms(ModContentPack content) : base(content)
        {
            ModSingleton = this;
            var harmony = new Harmony("PeteTimesSix.SimpleSidearms");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override string SettingsCategory()
        {
            return "SimpleSidearms_ModTitle".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
            base.DoSettingsWindowContents(inRect);
        }
    }

    [StaticConstructorOnStartup]
    public static class SimpleSidearms_PostInit 
    {
        static SimpleSidearms_PostInit()
        {
            SimpleSidearms.Settings = SimpleSidearms.ModSingleton.GetSettings<SimpleSidearms_Settings>();
            InferredValues.Init();
            SimpleSidearms.Settings.StartupChecks();

            if (SimpleSidearms.Settings.NeedsResaving)
            {
                Log.Message($"SS: Resaving settings by request (one-time migration or clearing out invalid defs).");
                SimpleSidearms.Settings.NeedsResaving = false;
                SimpleSidearms.ModSingleton.WriteSettings();
            }
        }
    }

}
