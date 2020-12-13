using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ExperimentalOptimizations.Fix
{
    [FixOn(InitStage.ModInit)]
    public class Bubbler_Add_Patch
    {
        public static void Patch()
        {
            var set_DoNonPlayer = AccessTools.Method("Bubbles.Interface.Theme:set_DoNonPlayer");
            if (set_DoNonPlayer != null)
            {
                set_DoNonPlayer.Invoke(null, new object[] {false});
                Log.Message($"[ExperimentalOptimizations] Bubbles.Interface.Theme optimized");
            }
        }
    }
}