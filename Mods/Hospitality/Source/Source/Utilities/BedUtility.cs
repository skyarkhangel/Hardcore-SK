using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;
using static UnityEngine.Mathf;

namespace Hospitality.Utilities
{
    internal static class BedUtility
    {
        public static Building_GuestBed FindBedFor(this Pawn guest)
        {
            var silver = guest.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver);
            var money = silver?.stackCount ?? 0;

            var beds = FindAvailableBeds(guest, money);
            //Log.Message($"Found {beds.Length} guest beds that {guest.LabelShort} can afford (<= {money} silver).");
            if (!beds.Any()) return null;

            return SelectBest(beds, guest, money);
        }

        [NotNull]
        public static IReadOnlyList<Pawn> Owners([CanBeNull]this Building_Bed bed)
        {
            var comp = bed?.CompAssignableToPawn;
            if (comp == null) return Array.Empty<Pawn>();
            if (comp.AssignedPawnsForReading == null) return Array.Empty<Pawn>();
            comp.AssignedPawnsForReading.RemoveAll(p => p == null);
            return bed.CompAssignableToPawn.AssignedPawnsForReading;
        }

        private static IEnumerable<Building_GuestBed> FindAvailableBeds(Pawn guest, int money)
        {
            return guest.MapHeld.GetGuestBeds(guest.GetGuestArea()).Where(bed => 
                bed.AnyUnownedSleepingSlot 
                && bed.RentalFee <= money 
                && !bed.IsForbidden(guest) 
                && !bed.IsBurning() 
                && !bed.CompAssignableToPawn.IdeoligionForbids(guest)
                && guest.CanReserveAndReach(bed, PathEndMode.OnCell, Danger.Some));
        }

        private static Building_GuestBed SelectBest(IEnumerable<Building_GuestBed> beds, Pawn guest, int money)
        {
            return beds.MaxBy(bed => BedValue(bed, guest, money));
        }

        private static float BedValue(Building_GuestBed bed, Pawn guest, int money)
        {
            StaticBedValue(bed, out var room, out var quality, out var impressiveness, out var roomType, out var comfort, out var facilities);

            var fee = RoundToInt(money > 0 ? 250 * (bed.RentalFee / money) : 0); // 0 - 250

            //Ideology
            var ideologyNeeds = GetIdeologicalFulfillment(bed, guest); // -150 - 50

            // Royalty requires single bed?
            var royalExpectations = GetRoyalExpectations(bed, guest, room, out var title);

            // Shared
            var otherPawnOpinion = OtherPawnOpinion(bed, guest); // -150 - 0

            // Temperature
            var temperature = GetTemperatureScore(guest, room, bed); // -200 - 0

            // Traits
            if (guest.story.traits.HasTrait(TraitDefOf.Greedy))
            {
                fee *= 3;
                impressiveness -= 50;
            }

            if (guest.story.traits.HasTrait(TraitDefOf.Kind))
                fee /= 2;

            if (guest.story.traits.HasTrait(TraitDefOf.Ascetic))
            {
                impressiveness *= -1;
                roomType = 75; // We don't care, so always max
                comfort = 100; // We don't care, so always max
                facilities = 0; // We don't care, so they're ignored
            }

            if (guest.story.traits.HasTrait(TraitDefOf.Jealous))
            {
                fee /= 4;
                impressiveness -= 50;
                impressiveness *= 4;
            }

            // Tired
            int distance = 0;
            if (guest.IsTired())
            {
                distance = (int) bed.Position.DistanceTo(guest.Position);
                //Log.Message($"{guest.LabelShort} is tired. {bed.LabelCap} is {distance} units far away.");
            }

            var score = impressiveness + quality + comfort + roomType + temperature + otherPawnOpinion + royalExpectations + ideologyNeeds + facilities - distance;
            var value = CeilToInt(ScoreFactor * score - fee);
            //Log.Message($"For {guest.LabelShort} {bed.Label} at {bed.Position} has a score of {score} and value of {value}:\n"
            //            + $"impressiveness = {impressiveness}, quality = {quality}, fee = {fee}, roomType = {roomType}, opinion = {otherPawnOpinion}, temperature = {temperature}, distance = {distance}");
            return value;
        }

        private static int GetFacilityScore(Building building)
        {
            return building.TryGetComp<CompAffectedByFacilities>()?.LinkedFacilitiesListForReading.Count * 10 ?? 0;
        }
        
        private static int GetIdeologicalFulfillment(Building_Bed bed, Pawn guest)
        {
            if (!ModsConfig.IdeologyActive) return 0;
            if (guest.ideo == null || bed == null) return 0;

            int score = 0;

            // If there's someone already using this bed and we're willing to share, affect score accordingly
            score += OtherOwnerScore(bed, guest);

            var requiredFacilities = guest.Ideo.PreceptsListForReading.SelectMany(p => p.def.comps.Select(c => (c as PreceptComp_BedThought)?.requireFacility?.def)).ToList();
            if (requiredFacilities.Any())
            {
                var comp = bed.TryGetComp<CompAffectedByFacilities>();
                if(comp?.linkedFacilities != null)
                    score += 10 * comp.linkedFacilities.Count(f => requiredFacilities.Contains(f.def));
            }
            var dominance = IdeoUtility.GetStyleDominance(bed, guest.Ideo);
            return score + CeilToInt(dominance * 50);
        }

        private static int OtherOwnerScore(Building_Bed bed, Pawn guest)
        {
            var score = 0;
            foreach (var otherOwner in bed.Owners().Where(owner => owner != null && owner != guest))
                if (RimWorld.BedUtility.WillingToShareBed(guest, otherOwner))
                {
                    score += guest.relations.OpinionOf(otherOwner); // -100 to 100
                }

            return score;
        }

        private static int OtherPawnOpinion(Building_GuestBed bed, Pawn guest)
        {
            if (!BedClaimedByStranger(bed, guest)) return 0;

            // Take opinion of other into account
            var otherOwner = bed.Owners().FirstOrDefault(owner => owner != guest);
            var opinion = otherOwner != null ? (guest.relations.OpinionOf(otherOwner) - 15) * 4 : 0;
            return -150 + opinion;
        }

        private static int GetRoyalExpectations(Building_GuestBed bed, Pawn guest, Room room, out RoyalTitle title)
        {
            var royalExpectations = 0;
            title = guest.royalty?.HighestTitleWithBedroomRequirements();
            if (title != null)
            {
                if (room == null) royalExpectations -= 75;
                else
                    foreach (Building_Bed roomBeds in room.ContainedBeds)
                    {
                        if (roomBeds != bed && BedClaimedByStranger(roomBeds, guest))
                        {
                            royalExpectations -= 100;
                        }
                    }

                if (RoyalTitleUtility.BedroomSatisfiesRequirements(room, title)) royalExpectations += 100;
            }

            return royalExpectations;
        }

        private static bool BedClaimedByStranger(Building_Bed bed, Pawn guest)
        {
            return bed.Owners().Any(p => p != guest && !p.RaceProps.Animal && !LovePartnerRelationUtility.LovePartnerRelationExists(p, guest));
        }

        public static int StaticBedValue(Building_GuestBed bed, [CanBeNull] out Room room, out int quality, out int impressiveness, out int roomTypeScore, out int comfort, out int facilities)
        {
            room = bed.Map != null && bed.Map.regionAndRoomUpdater.Enabled ? bed.GetRoom() : null;

            //Facilities
            facilities = GetFacilityScore(bed); // facilityCount * 10
            quality = GetBedQuality(bed);
            impressiveness = room != null ? GetRoomImpressiveness(room) : 0;
            roomTypeScore = GetRoomTypeScore(room) * 2;
            comfort = RoundToInt(100*GetBedComfort(bed));
            return quality + impressiveness + roomTypeScore + comfort + facilities;
        }

        private static float GetBedComfort(Building_GuestBed bed)
        {
            return bed.GetStatValue(StatDefOf.Comfort);
        }

        private static int GetBedQuality(Building_Bed bed)
        {
            if (!bed.TryGetQuality(out QualityCategory category)) category = QualityCategory.Normal;
            return ((int) category - 2) * 25;
        }

        private static int GetRoomImpressiveness(Room room)
        {
            return RoundToInt(room.GetStat(RoomStatDefOf.Impressiveness));
        }

        private static float GetTemperatureScore(Pawn guest, Room room, Building_Bed bed)
        {
            if (room == null) return 0;
            var optimalTemperature = GenTemperature.ComfortableTemperatureRange(guest.def);
            var pctTemperature = Abs(optimalTemperature.InverseLerpThroughRange(room.Temperature) - 0.5f) * 2; // 0-1
            return RoundToInt(Lerp(0, -200, pctTemperature - 0.75f) * 4); // -200 - 0
        }

        private static int GetRoomTypeScore(Room room)
        {
            if (room == null) return -30;
            if (room.OutdoorsForWork) return -50;

            int roomType;
            if (room.Role == RoomRoleDefOf.Barracks) roomType = 0;
            else if (room.Role == InternalDefOf.GuestRoom) roomType = 30;
            else roomType = -30;
            if (room.OnlyOneBed()) roomType += 60;
            return roomType;
        }

        public static IEnumerable<Building_GuestBed> GetGuestBeds(this Map map, Area area = null)
        {
            if (map == null) return Array.Empty<Building_GuestBed>();
            if (area == null) return map.listerBuildings.AllBuildingsColonistOfClass<Building_GuestBed>();
            return map.listerBuildings.AllBuildingsColonistOfClass<Building_GuestBed>().Where(b => area[b.Position]);
        }

        public static Building_Bed GetGuestBed(Pawn pawn)
        {
            var compGuest = pawn.CompGuest();
            return compGuest?.bed;
        }

        public static bool IsGuestBed(this Building_Bed bed) => bed is Building_GuestBed;
        public const float ScoreFactor = 0.5f; // factor for displayed value
    }
}
