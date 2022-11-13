using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RocketMan.Patches
{
    [RocketPatch(typeof(Pawn_TimetableTracker), nameof(Pawn_TimetableTracker.GetAssignment))]
    public static class Pawn_TimetableTracker_GetAssignment_Patch
    {
        private static Exception Finalizer(Exception __exception, Pawn_TimetableTracker __instance, int hour,
            ref TimeAssignmentDef __result)
        {
            if (__exception != null)
            {
                Log.Warning($"ROCKETMAN: Prevented a <color=orange>game breaking bug</color> due to a possible mod removal. <color=white>This has nothing todo with RocketMan but is only here as an game tweak for those who remove modded time assignments mid game to improve the overall usefulness of RocketMan. RocketMan detects this error and fix it by setting TimeAssignmentDef to TimeAssignmentDefOf.Anything</color> <color=red>{__exception}</color>");
                Log.TryOpenLogWindow();
                try
                {
                    __result = TimeAssignmentDefOf.Anything;
                    __instance.SetAssignment(hour, TimeAssignmentDefOf.Anything);
                }
                catch
                {
                    return __exception;
                }
            }
            return null;
        }
    }
}