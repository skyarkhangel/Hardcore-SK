using System;
namespace RocketMan
{
    public static class RocketDebugPrefs
    {
        public static bool Debug = false;

        public static bool StatLogging = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool FlashDilatedPawns = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool AlwaysDilating = false;

        public static bool DrawGlowerUpdates = false;

        public static bool LogData = false;

        public static bool Debug150MTPS = false;        
    }
}
