using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Verse;

namespace RocketMan.Patches
{
    [RocketStartupPatch(typeof(Game), nameof(Game.FinalizeInit))]
    public static class Game_FinalizeInit_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static void Postfix()
        {
            Main.WorldLoaded();
        }
    }

    [RocketStartupPatch(typeof(Game), nameof(Game.DeinitAndRemoveMap))]
    public static class Game_DeinitAndRemoveMap_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static void Postfix(Map map)
        {
            Main.MapDiscarded(map);
        }
    }

    [RocketStartupPatch(typeof(Game), nameof(Game.UpdatePlay))]
    public static class Game_UpdatePlay_Patch
    {
        [HarmonyPriority(Priority.First)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Postfix()
        {
            RocketStates.Context = ContextFlag.Updating;
        }
    }
}
