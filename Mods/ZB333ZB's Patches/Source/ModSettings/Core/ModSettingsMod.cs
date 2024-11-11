using UnityEngine;
using Verse;

namespace ZB333ZB_Patches
{
    public class ModSettingsMod : Mod
    {
        public static ModSettingsData Settings { get; private set; }

        public ModSettingsMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ModSettingsData>();
        }

        public override string SettingsCategory()
        {
            return "ZB333ZB's Patches";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
        }

        public override void WriteSettings()
        {
            if (!Settings.SettingsChanged())
            {
                base.WriteSettings();
            }
        }
    }
}
