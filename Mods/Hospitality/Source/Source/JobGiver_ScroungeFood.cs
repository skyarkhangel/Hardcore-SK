using System.Linq;
using Hospitality.Utilities;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using FoodUtility = RimWorld.FoodUtility;
using GuestUtility = Hospitality.Utilities.GuestUtility;

namespace Hospitality
{
    public class JobGiver_ScroungeFood : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            if (!pawn.IsArrivedGuest(out _)) return 0;

            var need = pawn.needs.food;
            if (need == null) return 0;

            if (pawn.needs.food.CurCategory < HungerCategory.Starving && FoodUtility.ShouldBeFedBySomeone(pawn)) return 0;

            var requiresFoodFactor = GuestUtility.GetRequiresFoodFactor(pawn);
            if (requiresFoodFactor > 0.35f)
            {
                return requiresFoodFactor * 6;
            }
            var priority = requiresFoodFactor;
            //if(priority > 0) Log.Message($"{pawn.NameShortColored} scrounge food priority: {priority:F2}; factor = {requiresFoodFactor}");
            return priority;
        }

        public override Job TryGiveJob(Pawn guest)
        {
            if (guest.needs.food == null) return null;

            var guestComp = guest.CompGuest();
            if (guestComp == null) return null;
            
            if (GenTicks.TicksGame < guestComp.lastFoodCheckTick) return null;
            guestComp.lastFoodCheckTick = GenTicks.TicksGame + 500; // Recheck ever x ticks

            bool canManipulateTools = guest.RaceProps.ToolUser && guest.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            var food = canManipulateTools ? BestFoodInInventory(guest, guest) : null;
            if (food != null) return null;

            var pressure = (guest.needs.food.CurCategory) switch
            {
                HungerCategory.Fed => -25,
                HungerCategory.Hungry => 25,
                HungerCategory.UrgentlyHungry => 50,
                HungerCategory.Starving => 75,
                _ => 0
            };

            var maxStealOpinion = -pressure;
            if (guest.story.traits.HasTrait(TraitDefOf.Greedy)) maxStealOpinion += 25;
            if (guest.story.traits.HasTrait(TraitDefOf.Jealous)) maxStealOpinion += 25;
            if (guest.story.traits.HasTrait(TraitDefOf.Kind)) maxStealOpinion -= 100;
            if (guest.story.traits.HasTrait(TraitDefOf.Wimp)) maxStealOpinion -= 50;
            var target = FindTarget(guest, maxStealOpinion);
            var swipe = target?.Awake() == false;
            //Log.Message($"{guest.LabelCap} tried to find scroungable food. Found {target?.Label}. pressure = {pressure} maxStealOpinion = {maxStealOpinion} swipe = {swipe}");
            if (target == null) return null;
            return new Job(swipe ? InternalDefOf.SwipeFood : InternalDefOf.ScroungeFood, target) { overeat = swipe }; // overeat stores swiping
        }

        private static Pawn FindTarget(Pawn guest, int maxStealOpinion)
        {
            var lord = guest.GetLord();
            var targetPawn = TryFindGroupPawn(guest, maxStealOpinion, lord);
            if (targetPawn != null) return targetPawn;
            targetPawn = guest.MapHeld.lordManager.lords.Where(l => l != lord).Select(l => TryFindGroupPawn(guest, maxStealOpinion, lord)).FirstOrDefault();
            if (targetPawn != null) return targetPawn;
            targetPawn = guest.MapHeld.mapPawns.pawnsSpawned.FirstOrDefault(p => Qualifies(p, guest, maxStealOpinion));
            return targetPawn;
        }

        private static Pawn TryFindGroupPawn(Pawn guest, int maxStealOpinion, Lord lord)
        {
            return lord.ownedPawns.FirstOrDefault(p => Qualifies(p, guest, maxStealOpinion));
        }

        private static bool Qualifies(Pawn target, Pawn guest, int maxStealOpinion)
        {
            if (target == guest) return false;
            if (target.inventory == null) return false;

            var awake = target.Awake();
            if (!awake && guest.relations.OpinionOf(target) > maxStealOpinion) return false;

            var minAwakeOpinion = 0;
            if (target.story.traits.HasTrait(TraitDefOf.Kind)) minAwakeOpinion -= 35;
            if (target.story.traits.HasTrait(TraitDefOf.Kind)) minAwakeOpinion += 50;
            if (awake && target.relations.OpinionOf(guest) < minAwakeOpinion) return false;

            var food = BestFoodInInventory(target, guest);
            return food != null;
        }

        internal static Thing BestFoodInInventory(Pawn holder, Pawn eater)
        {
            if (holder.inventory == null) return null;

            var innerContainer = holder.inventory.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                var thing = innerContainer.innerList[i];
                if (thing.def.IsNutritionGivingIngestible && thing.IngestibleNow && eater.WillEat(thing, eater) && (int)thing.def.ingestible.preferability >= (int)FoodPreferability.RawBad && !thing.def.IsDrug) return thing;
            }
            return null;
        }
    }
}