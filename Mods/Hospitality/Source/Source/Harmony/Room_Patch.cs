using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    internal static class Room_Patch
    {
        [HarmonyPatch(typeof(Room), "Owners")]
        [HarmonyPatch(MethodType.Getter)]
        public class Owners
        {
            [HarmonyPrefix]
            public static bool Replacement(Room __instance, out IEnumerable<Pawn> __result)
            {
                __result = GetOwnersInternal(__instance);
                return false;
            }

            // Copied
            private static IEnumerable<Pawn> GetOwnersInternal(Room room)
            {
                // Extracted room types
                if (room.TouchesMapEdge || room.IsHuge || WrongRoomType(room)) yield break;

                Pawn pawn = null;
                Pawn secondOwner = null;
                foreach (Building_Bed containedBed in room.ContainedBeds)
                {
                    if (containedBed.def.building.bed_humanlike)
                    {
                        foreach (var owner in containedBed.Owners())
                        {
                            if (pawn == null)
                            {
                                pawn = owner;
                            }
                            else
                            {
                                if (secondOwner != null)
                                {
                                    yield break;
                                }

                                secondOwner = owner;
                            }
                        }
                    }
                }

                if (pawn != null)
                {
                    if (secondOwner == null)
                    {
                        yield return pawn;
                    }
                    else if (LovePartnerRelationUtility.LovePartnerRelationExists(pawn, secondOwner))
                    {
                        yield return pawn;
                        yield return secondOwner;
                    }
                }
            }

            private static bool WrongRoomType(Room room)
            {
                // Added guest room
                return room.Role != BedUtility.roleDefGuestRoom && room.Role != RoomRoleDefOf.Bedroom && room.Role != RoomRoleDefOf.PrisonCell && room.Role != RoomRoleDefOf.Barracks && room.Role != RoomRoleDefOf.PrisonBarracks;
            }
        }
    }
}
