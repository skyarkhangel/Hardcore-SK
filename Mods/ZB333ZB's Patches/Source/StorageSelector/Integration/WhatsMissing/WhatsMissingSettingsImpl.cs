using Verse;
using ZB333ZB_Patches;
using ZB333ZB_Patches.Core;

namespace StorageSelector.Integration.WhatsMissing
{
    public class WhatsMissingSettingsImpl : IWhatsMissingSettings
    {
        private ModSettingsData Settings => LoadedModManager.GetMod<ModSettingsMod>()?.GetSettings<ModSettingsData>();

        public bool HideZeroCountIngredients => Settings?.hideZeroCountIngredients ?? true;
        public int MaxTooltipsWidth => Settings?.maxTooltipsWidth ?? 260;
    }
}
