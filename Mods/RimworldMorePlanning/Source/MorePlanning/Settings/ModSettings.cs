
using HugsLib;
using HugsLib.Settings;
using Verse;

namespace MorePlanning.Settings
{
    class ModSettings
    {
        private SettingHandle<bool> _removeIfBuildingDespawned;
        public bool RemoveIfBuildingDespawned => _removeIfBuildingDespawned;

        private SettingHandle<bool> _shiftKeyForOverride;
        public bool ShiftKeyForOverride => _shiftKeyForOverride;

        private SettingHandle<int> _planOpacity;
        public int PlanOpacity
        {
            get => _planOpacity.Value;
            set
            {
                _planOpacity.Value = value;
                HugsLibController.SettingsManager.SaveChanges();
            }
        }

        public int DefaultPlanOpacity => _planOpacity.DefaultValue;

        private ModSettings()
        {

        }

        public static ModSettings CreateModSettings(ModSettingsPack settingsPack)
        {
            var settings = new ModSettings();

            settings._removeIfBuildingDespawned = settingsPack.GetHandle("removeIfBuildingDespawned", "MorePlanning.SettingRemoveIfBuildingDespawned.label".Translate(), "MorePlanning.SettingRemoveIfBuildingDespawned.desc".Translate(), false);
            settings._shiftKeyForOverride = settingsPack.GetHandle("shiftKeyForOverride", "MorePlanning.SettingShiftKeyForOverride.label".Translate(), "MorePlanning.SettingShiftKeyForOverride.desc".Translate(), false);
            settings._planOpacity = settingsPack.GetHandle("opacity", "MorePlanning.SettingPlanOpacity.label".Translate(), "MorePlanning.SettingPlanOpacity.desc".Translate(), 25);
            settings._planOpacity.NeverVisible = true;

            return settings;
        }
    }
}
