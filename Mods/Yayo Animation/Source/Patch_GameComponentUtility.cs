using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using yayoAni.Data;

namespace yayoAni
{
    // GameComponent would cause (one time) errors if removed from game
    public static class Patch_GameComponentUtility
    {
        [HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.StartedNewGame))]
        [HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.LoadedGame))]
        public static class ResetOnStartedOrLoaded
        {
            [UsedImplicitly]
            public static void Postfix() => DataUtility.Reset();
        }

        [HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.GameComponentTick))]
        public static class DoTicking
        {
            [UsedImplicitly]
            public static void Postfix()
            {
                if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
                    DataUtility.GC();
            }
        }
    }
}