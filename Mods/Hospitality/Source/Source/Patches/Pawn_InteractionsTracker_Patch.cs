using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using GuestUtility = Hospitality.Utilities.GuestUtility;

namespace Hospitality.Patches
{
    /// <summary>
    /// Allow colonists to talk to guests randomly
    /// </summary>
    internal static class Pawn_InteractionsTracker_Patch
    {
        [HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractRandomly))]
        public class TryInteractRandomly
        {
            [HarmonyPrefix]
            public static bool Replacement(Pawn_InteractionsTracker __instance, ref bool __result, Pawn ___pawn, List<Pawn> ___workingList, int ___lastInteractionTime)
            {
                // Added
                if (!IsInteractable(___pawn))
                {
                    __result = false;
                    return false;
                }

                if (InteractedTooRecentlyToInteract(___lastInteractionTime)) // Changed to own
                {
                    __result = false;
                    return false;
                }
                // BASE
                if (!InteractionUtility.CanInitiateRandomInteraction(___pawn))
                {
                    __result = false;
                    return false;
                }
                var collection = ___pawn.MapHeld.mapPawns.AllPawnsSpawned.Where(IsInteractable); // Added
                ___workingList.Clear();
                ___workingList.AddRange(collection);
                ___workingList.Shuffle();
                List<InteractionDef> allDefsListForReading = DefDatabase<InteractionDef>.AllDefsListForReading;
                foreach (var p in ___workingList)
                {
                    if (p != ___pawn && CanInteractNowWith(___pawn, p) && InteractionUtility.CanReceiveRandomInteraction(p)
                        && !___pawn.HostileTo(p))
                    {
                        var p1 = p;
                        if (
                            allDefsListForReading.TryRandomElementByWeight(
                                x => !CanInteractNowWith(___pawn, p, x) ? 0f : x.Worker.RandomSelectionWeight(___pawn, p), out var result))
                        {
                            if (__instance.TryInteractWith(p, result))
                            {
                                ___workingList.Clear();
                                __result = true;
                                return false;
                            }
                            Log.Warning($"{___pawn} failed to interact with {p}.");
                        }
                    }
                }
                ___workingList.Clear();
                __result = false;
                return false;
            }

            private static bool IsInteractable(Pawn pawn) // Added
            {
                return pawn?.Downed == false && pawn.RaceProps.Humanlike && pawn.relations != null && pawn.story?.traits != null;
            }

            private static bool CanInteractNowWith(Pawn pawn, Pawn recipient,  InteractionDef interactionDef = null) 
            {
                if (!recipient.Spawned)
                {
                    return false;
                }
                if (!InteractionUtility.IsGoodPositionForInteraction(pawn, recipient))
                {
                    return false;
                }
                if (!InteractionUtility.CanInitiateInteraction(pawn, interactionDef) || !InteractionUtility.CanReceiveInteraction(recipient, interactionDef))
                {
                    return false;
                }
                // Added
                if (!pawn.CanReserve(recipient) || !recipient.CanReserve(pawn))
                {
                    return false;
                }
                if (!GenSight.LineOfSight(pawn.Position, recipient.Position, pawn.MapHeld, true))
                {
                    return false;
                }
                return true;
            }

            // Added to change InteractIntervalAbsoluteMin
            public static bool InteractedTooRecentlyToInteract(int ___lastInteractionTime)
            {
                return Find.TickManager.TicksGame < ___lastInteractionTime + GuestUtility.InteractIntervalAbsoluteMin;
            }
        }
    }
}