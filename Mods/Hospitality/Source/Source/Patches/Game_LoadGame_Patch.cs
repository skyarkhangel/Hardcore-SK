using HarmonyLib;
using Hospitality.Utilities;
using Verse;

namespace Hospitality.Patches
{
    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
    public static class Game_LoadGame_Patch
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix()
        {
            ThoughtResultCache.Reset();
        }
    }
}
