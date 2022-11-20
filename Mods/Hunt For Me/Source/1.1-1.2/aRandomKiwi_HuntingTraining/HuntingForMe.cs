using HarmonyLib;
using System.Reflection;
using Verse;
using UnityEngine;

namespace aRandomKiwi.HFM
{
    [StaticConstructorOnStartup]
    class HuntingForMe : Mod
    {
        public static Settings Settings;

        public HuntingForMe(ModContentPack content) : base(content)
        {
            base.GetSettings<Settings>();
        }

        public void Save()
        {
            LoadedModManager.GetMod<HuntingForMe>().GetSettings<Settings>().Write();
        }

        public override string SettingsCategory()
        {
            return "Hunt For Me";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }
}