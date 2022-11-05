using HarmonyLib;
using System.Reflection;
using Verse;
using UnityEngine;

namespace aRandomKiwi.RimThemes
{
    [StaticConstructorOnStartup]
    class RimThemes : Mod
    {
        public static LoaderGM comp;
        public RimThemes(ModContentPack content) : base(content)
        {
            base.GetSettings<Settings>();
            Themes.LogMsg("[RimThemes] Init");
            Utils.currentMod = content;
            Utils.currentModInst = this;

            var inst = new Harmony("rimworld.aRandomKiwi.RimTheme");
            inst.PatchAll(Assembly.GetExecutingAssembly());
            Themes.LogMsg("Init Harmony patchs");

            Log.Message(Utils.releaseInfo);

            //Add GM of the loader to override the display
            comp = GameObject.Find("Camera").AddComponent<LoaderGM>() as LoaderGM;
        }

        public void Save()
        {
            LoadedModManager.GetMod<RimThemes>().GetSettings<Settings>().Write();
        }

        public override string SettingsCategory()
        {
            return "RimThemes";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }
}