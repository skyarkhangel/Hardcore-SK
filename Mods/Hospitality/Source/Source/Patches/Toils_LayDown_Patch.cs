using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using Verse;
using BedUtility = Hospitality.Utilities.BedUtility;

namespace Hospitality.Patches
{
    public class Toils_LayDown_Patch
    {
        /// <summary>
        /// So guests can think about their bedroom
        /// </summary>
        [HarmonyPatch(typeof(Toils_LayDown), nameof(Toils_LayDown.ApplyBedThoughts))]
        public class ApplyBedThoughts
        {
            [HarmonyPrefix]
            public static bool Replacement(Pawn actor)
            {
                if (actor.needs.mood == null) return false;

                Building_Bed building_Bed = actor.CurrentBed();
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOnGround);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
                if (actor.GetRoom().PsychologicallyOutdoors)
                {
                    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOutside);
                }
                if (building_Bed == null || building_Bed.CostListAdjusted().Count == 0)
                {
                    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOnGround);
                }
                if (actor.AmbientTemperature < actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin))
                {
                    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInCold);
                }
                if (actor.AmbientTemperature > actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax))
                {
                    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInHeat);
                }

                if (building_Bed == null || !AddedBedIsOwned(actor, building_Bed) || building_Bed.ForPrisoners || actor.story.traits.HasTrait(TraitDefOf.Ascetic))
                {
                    return false;
                }
                ThoughtDef thoughtDef = null;
                // ADDED:
                if (building_Bed.GetRoom().Role == InternalDefOf.GuestRoom)
                {
                    thoughtDef = building_Bed.GetRoom().OnlyOneBed() ? ThoughtDefOf.SleptInBedroom : ThoughtDefOf.SleptInBarracks;
                } ////
                else if (building_Bed.GetRoom().Role == RoomRoleDefOf.Bedroom)
                {
                    thoughtDef = ThoughtDefOf.SleptInBedroom;
                }
                else if (building_Bed.GetRoom().Role == RoomRoleDefOf.Barracks)
                {
                    thoughtDef = ThoughtDefOf.SleptInBarracks;
                }
                if (thoughtDef != null)
                {
                    int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(building_Bed.GetRoom().GetStat(RoomStatDefOf.Impressiveness));
                    if (thoughtDef.stages[scoreStageIndex] != null)
                    {
                        actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex));
                    }
                }
                return false;
            }

            // Added
            private static bool AddedBedIsOwned(Pawn pawn, Building_Bed buildingBed)
            {
                return pawn.IsArrivedGuest(out _) 
                    ? BedUtility.GetGuestBed(pawn) == buildingBed 
                    : buildingBed == pawn.ownership.OwnedBed;
            }
        }
    }
}