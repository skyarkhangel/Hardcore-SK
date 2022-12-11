namespace PeteTimesSix.SimpleSidearms.Utilities
{
    public static class Enums
    {
        public enum OptionsTab { Presets, Automation, Allowances, Spawning, Misc }
        public enum SettingsPreset { NoneApplied, Custom, Lite, Disabled, LoadoutOnly, Basic, Advanced, Excessive, Brawler }
        public enum WeaponListKind { Both, Melee, Ranged }
        public enum RangeCategory : byte { Touch, Short, Medium, Long }
        public enum WeaponSearchType { Both, Ranged, Melee, MeleeCapable }
        public enum PrimaryWeaponMode { Ranged, Melee, BySkill, ByGenerated }
        public enum LimitModeSingleSidearm { None, RelativeWeight, AbsoluteWeight, Selection }
        public enum LimitModeAmountOfSidearms { None, RelativeWeight, AbsoluteWeight, Slots }
        public enum FumbleModeOptionsEnum { Never, InDistress, InCombat, Always }
        public enum DroppingModeEnum { InDistress, Combat, Calm, UsedUp }

    }
}
