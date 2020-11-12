using System;
using System.Linq;
using Gastronomy.Dining;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Gastronomy
{
    internal static class RestaurantUtility
    {
        public static bool HasDiningQueued(this Pawn patron)
        {
            if (patron?.CurJobDef == DiningUtility.dineDef) return true;
            return patron?.jobs.jobQueue?.Any(j => j.job.def == DiningUtility.dineDef) == true;
        }

        public static RestaurantController GetRestaurant([NotNull]this Thing thing)
        {
            return thing.Map.GetComponent<RestaurantController>();
        }

        public static void GetRequestGroup(Thing thing)
        {
            foreach (ThingRequestGroup group in Enum.GetValues(typeof(ThingRequestGroup)))
            {
                if (@group == ThingRequestGroup.Undefined) continue;
                if (thing.Map.listerThings.ThingsInGroup(@group).Contains(thing))
                    Log.Message($"DiningSpot group: {@group}");
            }
        }

        public static bool IsRegionDangerous(Pawn pawn, Region region = null)
        {
            if (region == null) region = pawn.GetRegion();
            return region.DangerFor(pawn) > Danger.None;
        }

        public static bool IsGuest(this Pawn pawn)
        {
            var faction = pawn.GetLord()?.faction;
            //Log.Message($"{pawn.NameShortColored}: Faction = {faction?.GetCallLabel()} Is player = {faction?.IsPlayer} Hostile = {faction?.HostileTo(Faction.OfPlayer)}");
            return faction != null && !faction.IsPlayer && !faction.HostileTo(Faction.OfPlayer);
            //var isGuest = AccessTools.Method("Hospitality.GuestUtility:IsGuest");
            //Log.Message($"isGuest == null? {isGuest == null}");
            //if(isGuest != null)
            //{
            //    return (bool) isGuest.Invoke(null, new object[] {pawn, false});
            //}
            //return false;
        }

        public static int GetSilver(this Pawn pawn)
        {
            if (pawn?.inventory?.innerContainer == null) return 0;
            return pawn.inventory.innerContainer.Where(s => s.def == ThingDefOf.Silver).Sum(s => s.stackCount);
        }

        public static float GetPrice(this ThingDef mealDef, RestaurantController restaurant)
        {
            if (mealDef == null) return 0;
            return mealDef.BaseMarketValue * restaurant.guestPricePercentage;
        }
    }
}
