using System;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace RocketMan
{
    public static class Finder
    {
        public static readonly string HarmonyID = "Krkr.RocketMan";

        public static RocketMod Mod;

        public static ModContentPack ModContentPack;

        public static Harmony Harmony = new Harmony(HarmonyID);

        public static RocketShip.SkipperPatcher Rocket = new RocketShip.SkipperPatcher(HarmonyID);

        public static Window_Main RocketManWindow;

        public static RocketPluginsLoader PluginsLoader;

        public static StatSettingsGroup StatSettings;

        private static int _ticks = -1;
        private static World _world;
        /// <summary>
        /// Returns the ticks eplased since the game started/loaded.
        /// </summary>
        public static int SessionTicks
        {
            get
            {
                if (Current.Game == null)
                {
                    return 0;
                }
                World world = Find.World;
                if (world == null || Find.CurrentMap == null)
                {
                    return 0;
                }
                if (_world != world || _ticks == -1)
                {
                    _ticks = GenTicks.TicksGame;
                    _world = world;
                }
                return GenTicks.TicksGame - _ticks;
            }
        }
    }
}
