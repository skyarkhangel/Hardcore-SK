using HarmonyLib;
using System.Reflection;
using Verse;

namespace WorldGenRules
{
    [StaticConstructorOnStartup]
    class MyLittlePlanet : Mod
    {
#pragma warning disable 0649
        //public static Settings Settings;
#pragma warning restore 0649

        public MyLittlePlanet(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("net.quicksilverfox.rimworld.mod.worldgenrules");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            //base.GetSettings<Settings>();
        }

        public void Save()
        {
            //LoadedModManager.GetMod<WorldGenRules>().GetSettings<Settings>().Write();
        }

        //public override string SettingsCategory()
        //{
        //    return "WorldGenRules";
        //}

        //public override void DoSettingsWindowContents(Rect inRect)
        //{
        //    Settings.DoSettingsWindowContents(inRect);
        //}
    }
}
