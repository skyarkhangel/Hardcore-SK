using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Hospitality.Harmony
{
    internal static class Pawn_Patch 
    {
        /// <summary>
        /// Remove the pawn from CompUtility
        /// </summary>
        [HarmonyPatch(typeof(Pawn), "DeSpawn")]
        public class DeSpawn
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn __instance)
            {
                CompUtility.OnPawnRemoved(__instance);
            }
        }

        [HarmonyPatch(typeof(Pawn), "GiveSoldThingToPlayer")]
        public class GiveSoldThingToPlayer
        {
            [HarmonyPrefix]
            internal static bool Prefix(Pawn __instance, Thing toGive)
            {
                if (!__instance.IsGuest()) return true;
                var lord = __instance.GetLord();
                var toil = lord?.CurLordToil as LordToil_VisitPoint;

                // We got a proper guest
                toil?.OnPlayerBoughtItem(toGive);
                return true;
            }
        }


        [HarmonyPatch(typeof(Pawn), "GiveSoldThingToTrader")]
        public class GiveSoldThingToTrader
        {
            [HarmonyPrefix]
            internal static bool Prefix(Pawn __instance, Thing toGive)
            {
                if (!__instance.IsGuest()) return true;
                var lord = __instance.GetLord();
                var toil = lord?.CurLordToil as LordToil_VisitPoint;

                // We got a proper guest
                toil?.OnPlayerSoldItem(toGive);
                return true;
            }
        }

        /// <summary>
        /// Suppress "Pawn destination reservation manager failed to clean up properly" error, that doesn't seem to cause further problems.
        /// The error is due to guests doing work, apparently? Or the new JobGiver_StandAndBeSociallyActive
        /// </summary>
        [HarmonyPatch(typeof(Pawn), "VerifyReservations")]
        public class VerifyReservations
        {
            [HarmonyPrefix]
            internal static bool Prefix(Pawn __instance)
            {
                if (!__instance.IsGuest()) return true;

                // COPIED
                if (__instance.jobs == null)
                {
                    return true;
                }
                if (__instance.CurJob != null || __instance.jobs.jobQueue.Count > 0 || __instance.jobs.startingNewJob)
                {
                    return true;
                }
                bool flag = false;
                List<Map> maps = Find.Maps;
                // TO HERE, THEN SIMPLIFIED
                foreach (var map in maps)
                {
                    IntVec3 obj3 = map.pawnDestinationReservationManager.FirstObsoleteReservationFor(__instance);
                    if (obj3.IsValid)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag) __instance.ClearAllReservations();
                return true;
            }
        }

        [HarmonyPatch(typeof(Pawn), "SetFaction")]
        public class SetFaction
        {
            [HarmonyPrefix]
            public static bool Prefix(ref Faction newFaction, Pawn recruiter, Pawn __instance)
            {
                // When a guest recruits a prisoner, make sure he's recruited to the player's faction.
                if (recruiter != null && recruiter.Faction != Faction.OfPlayer && recruiter.HostFaction == Faction.OfPlayer)
                {
                    Log.Message($"Guest {recruiter.Name.ToStringShort} recruits prisoner to player faction (instead of {newFaction}).");
                    newFaction = Faction.OfPlayer;
                }
                // When a pawn is recruited to the player's faction, turn off the rescued flag
                if (newFaction == Faction.OfPlayer)
                {
                    var compGuest = __instance.CompGuest();
                    if (compGuest != null)
                    {
                        //Log.Message($"{__instance.Name.ToStringShort} has joined the player faction. 'rescued' was {compGuest.rescued}.");
                        compGuest.rescued = false;
                    }
                }

                return true;
            }
        }
    }
}
