using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hospitality.Patches
{
    internal static class WorkGiver_RescueDowned_Patch
    {
        [HarmonyPatch(typeof(WorkGiver_RescueDowned), nameof(WorkGiver_RescueDowned.ShouldSkip))]
        public class ShouldSkip_Patch
        {
            public static void Postfix(Pawn pawn, ref bool __result)
            {
                //Log.Message($"Adjusting should skip... all downed: {pawn.Map.mapPawns.SpawnedDownedPawns.Count()}");
                if (!__result) return;
                foreach (var guest in pawn.Map.mapPawns.SpawnedDownedPawns)
                {
                    if(!(guest.GuestStatus == GuestStatus.Guest && guest.HostFaction == pawn.Faction)) continue;
                    //Log.Message($"Checking guest {guest.NameShortColored}: Downed: {guest.Downed}");
                    if (guest.Downed && !guest.InBed())
                    {
                        __result = false;
                        return;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(WorkGiver_RescueDowned), nameof(WorkGiver_RescueDowned.HasJobOnThing))]
        public class HasJobOnThing_Patch
        {
            public static bool Prefix(Pawn pawn, Thing t, bool forced, ref bool __result)
            {
                Pawn guest = t as Pawn;
                if (guest?.GuestStatus != GuestStatus.Guest) return true;
                if (!guest.Downed || guest.HostFaction != pawn.Faction || guest.InBed() || !pawn.CanReserve(guest, 1, -1, null, forced) || GenAI.EnemyIsNear(guest, 40f))
                {
                    __result = false;
                    return false;
                }
                var bed = RestUtility.FindBedFor(guest, pawn, false, false, guest.GuestStatus);
                __result = bed != null && guest.CanReserve(bed);
                return false;
            }
        }
    }
}
