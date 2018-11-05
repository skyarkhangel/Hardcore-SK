using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace PickUpAndHaul
{
    public class WorkGiver_HaulToInventory : WorkGiver_HaulGeneral
    {
        //Thanks to AlexTD for the more dynamic search range
        //And queueing
        //And optimizing
        float searchForOthersRangeFraction = 0.5f;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return base.ShouldSkip(pawn) || pawn.Faction != Faction.OfPlayer || (!pawn.RaceProps.Humanlike) || pawn.TryGetComp<CompHauledToInventory>() == null; //hospitality check + misc robots & animals
        }

        public static bool GoodThingToHaul(Thing t, Pawn pawn) => t.Spawned
                && !t.IsInValidBestStorage()
                && !t.IsForbidden(pawn)
                && !(t is Corpse)
                && pawn.CanReserve(t);

        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced)
        {
            //bulky gear (power armor + minigun) so don't bother.
            if (MassUtility.GearMass(pawn) / MassUtility.Capacity(pawn) >= 0.8f) return false;
            
            if (!GoodThingToHaul(thing, pawn) || !HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, thing, forced))
                return false;

            StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
            return StoreUtility.TryFindBestBetterStoreCellFor(thing, pawn, pawn.Map, currentPriority, pawn.Faction, out IntVec3 storeCell, true);
        }

        //pick up stuff until you can't anymore,
        //while you're up and about, pick up something and haul it
        //before you go out, empty your pockets

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            //bulky gear (power armor + minigun) so don't bother.
            if (MassUtility.GearMass(pawn) / MassUtility.Capacity(pawn) >= 0.8f) return null;
            
            if (!GoodThingToHaul(thing, pawn) || !HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, thing, forced)) return null;

            StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
            if (StoreUtility.TryFindBestBetterStoreCellFor(thing, pawn, pawn.Map, currentPriority, pawn.Faction, out IntVec3 storeCell, true))
            {
                //since we've gone through all the effort of getting the loc, might as well use it.
                //Don't multi-haul food to hoppers.
                if (thing.def.IsNutritionGivingIngestible)
                {
                    if (thing.def.ingestible.preferability == FoodPreferability.RawBad || thing.def.ingestible.preferability == FoodPreferability.RawTasty)
                    {
                        List<Thing> thingList = storeCell.GetThingList(thing.Map);
                        for (int i = 0; i < thingList.Count; i++)
                        {
                            if (thingList[i].def == ThingDefOf.Hopper)
                                return HaulAIUtility.HaulToStorageJob(pawn, thing);
                        }
                    }
                }
            }
            else
            {
                JobFailReason.Is("NoEmptyPlaceLower".Translate());
                return null;
            }

            if (MassUtility.EncumbrancePercent(pawn) >= 0.90f)
            {
                Job haul = HaulAIUtility.HaulToStorageJob(pawn, thing);
                return haul;
            }

            //credit to Dingo
            int capacityStoreCell = CapacityAt(thing.def, storeCell, pawn.Map);

            if (capacityStoreCell == 0) return HaulAIUtility.HaulToStorageJob(pawn, thing);

            Job job = new Job(PickUpAndHaulJobDefOf.HaulToInventory, null, storeCell);   //Things will be in queues
            Log.Message($"-------------------------------------------------------------------");
            Log.Message($"------------------------------------------------------------------");//different size so the log doesn't count it 2x
            Log.Message($"{pawn} job found to haul: {thing} to {storeCell}:{capacityStoreCell}, looking for more now");

            //Find extra things than can be hauled to inventory, queue to reserve them
            DesignationDef HaulUrgentlyDesignation = DefDatabase<DesignationDef>.GetNamed("HaulUrgentlyDesignation", false);
            bool isUrgent = false;
            if (ModCompatibilityCheck.AllowToolIsActive &&
                pawn.Map.designationManager.DesignationOn(thing)?.def == HaulUrgentlyDesignation)
                isUrgent = true;

            Func<Thing, bool> validatorExtra = (Thing t) =>
                !job.targetQueueA.Contains(t) &&
                (!isUrgent || pawn.Map.designationManager.DesignationOn(t)?.def == HaulUrgentlyDesignation) &&
                GoodThingToHaul(t, pawn) && HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, false);//forced is false, may differ from first thing


            //Find what fits in inventory, set nextThingLeftOverCount to be 
            int nextThingLeftOverCount = 0;
            float encumberance = MassUtility.EncumbrancePercent(pawn);
            job.targetQueueA = new List<LocalTargetInfo>(); //more things
            job.targetQueueB = new List<LocalTargetInfo>(); //more storage; keep in mind the job doesn't use it, but reserve it so you don't over-haul
            job.countQueue = new List<int>();//thing counts


            //TODO check CE along with encumberance
            //float usedBulkByPct = 1f;
            //float usedWeightByPct = 1f;

            //try
            //{
            //    ((Action)(() =>
            //    {
            //        if (ModCompatibilityCheck.CombatExtendedIsActive)
            //        {
            //            CombatExtended.CompInventory ceCompInventory = pawn.GetComp<CombatExtended.CompInventory>();
            //            usedWeightByPct = ceCompInventory.currentWeight / ceCompInventory.capacityWeight;
            //            usedBulkByPct = ceCompInventory.currentBulk / ceCompInventory.capacityBulk;
            //        }
            //    }))();
            //}
            //catch (TypeLoadException) { }

            float distanceToHaul = (storeCell - thing.Position).LengthHorizontal * searchForOthersRangeFraction;
            float distanceToSearchMore = Math.Max(12f, distanceToHaul);

            List<Thing> haulables = pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling()
                .Where(validatorExtra).ToList();
            
            Thing nextThing = thing;
            Thing lastThing = thing;

            Dictionary<IntVec3, CellAllocation> storeCellCapacity = new Dictionary<IntVec3, CellAllocation>();
            storeCellCapacity[storeCell] = new CellAllocation(nextThing, capacityStoreCell);
            skipCells = new HashSet<IntVec3>() { storeCell };

            do
            {
                haulables.Remove(nextThing);
                if (AllocateThingAtCell(storeCellCapacity, pawn, nextThing, job))
                {
                    lastThing = nextThing;
                    encumberance += AddedEnumberance(pawn, nextThing);

                    if (encumberance > 1)// || usedBulkByPct >= 0.7f || usedWeightByPct >= 0.8f))//TODO: CE also
                    {
                        //can't CountToPickUpUntilOverEncumbered here, pawn doesn't actually hold these things yet
                        nextThingLeftOverCount = CountPastCapacity(pawn, nextThing, encumberance);
                        Log.Message($"Inventory allocated, will carry {nextThing}:{nextThingLeftOverCount}");
                        break;
                    }
                }
            }
            while ((nextThing = GenClosest.ClosestThingReachable(lastThing.Position, thing.Map, ThingRequest.ForUndefined(),
                PathEndMode.ClosestTouch, TraverseParms.For(pawn), distanceToSearchMore, null, haulables))
                is Thing);

            if (nextThing == null)
            {
                skipCells = null;
                return job;
            }

            //Find what can be carried
            //this doesn't actually get pickupandhauled, but will hold the reservation so others don't grab what this pawn can carry
            haulables.RemoveAll(t => !t.CanStackWith(nextThing));

            int carryCapacity = pawn.carryTracker.MaxStackSpaceEver(nextThing.def) - nextThingLeftOverCount;
            if (carryCapacity == 0)
            {
                Log.Message("Can't carry more, nevermind!");
                skipCells = null;
                return job;
            }
            Log.Message($"Looking for more like {nextThing}");

            while ((nextThing = GenClosest.ClosestThingReachable(nextThing.Position, thing.Map, ThingRequest.ForUndefined(),
                PathEndMode.ClosestTouch, TraverseParms.For(pawn), 8f, null, haulables))    //8f hardcoded in CheckForGetOpportunityDuplicate
                is Thing)
            {
                haulables.Remove(nextThing);
                carryCapacity -= nextThing.stackCount;

                if (AllocateThingAtCell(storeCellCapacity, pawn, nextThing, job))
                    break;

                if (carryCapacity <= 0)
                {
                    int lastCount = job.countQueue.Pop() + carryCapacity;
                    job.countQueue.Add(lastCount);
                    Log.Message($"Nevermind, last count is {lastCount}");
                    break;
                }
            }

            skipCells = null;
            return job;
        }

        public class CellAllocation
        {
            public Thing allocated;
            public int capacity;

            public CellAllocation(Thing a, int c)
            {
                allocated = a;
                capacity = c;
            }
        }

        public static int CapacityAt(ThingDef def, IntVec3 storeCell, Map map)
        {
            int capacity = def.stackLimit;

            Thing preExistingThing = map.thingGrid.ThingAt(storeCell, def);
            if (preExistingThing != null)
                capacity = def.stackLimit - preExistingThing.stackCount;

            return capacity;
        }

        public static bool AllocateThingAtCell(Dictionary<IntVec3, CellAllocation> storeCellCapacity, Pawn pawn, Thing nextThing, Job job)
        {
            Map map = pawn.Map;
            KeyValuePair<IntVec3, CellAllocation> allocation = storeCellCapacity.FirstOrDefault(kvp => 
                kvp.Key.GetSlotGroup(map).parent.Accepts(nextThing) && 
                kvp.Value.allocated.CanStackWith(nextThing));
            IntVec3 storeCell = allocation.Key;

            //Can't stack with allocated cells, find a new cell:
            if (storeCell == default(IntVec3))
            {
                StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(nextThing);
                if (TryFindBestBetterStoreCellFor(nextThing, pawn, map, currentPriority, pawn.Faction, out IntVec3 nextStoreCell))
                {
                    storeCell = nextStoreCell;
                    job.targetQueueB.Add(storeCell);

                    storeCellCapacity[storeCell] = new CellAllocation(nextThing, CapacityAt(nextThing.def, storeCell, map));

                    Log.Message($"New cell for unstackable {nextThing} = {nextStoreCell}");
                }
                else
                {
                    Log.Message($"{nextThing} can't stack with allocated cells");
                    return false;
                }
            }

            job.targetQueueA.Add(nextThing);
            int count = nextThing.stackCount;
            storeCellCapacity[storeCell].capacity -= count;
            Log.Message($"{pawn} allocating {nextThing}:{count}, now {storeCell}:{storeCellCapacity[storeCell].capacity}");
            

            while (storeCellCapacity[storeCell].capacity <= 0)
            {
                int capacityOver = -storeCellCapacity[storeCell].capacity;
                storeCellCapacity.Remove(storeCell);

                Log.Message($"{pawn} overdone {storeCell} by {capacityOver}");

                if (capacityOver == 0)
                    break;  //don't find new cell, might not have more of this thing to haul
                
                StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(nextThing);
                if (TryFindBestBetterStoreCellFor(nextThing, pawn, map, currentPriority, pawn.Faction, out IntVec3 nextStoreCell))
                {
                    storeCell = nextStoreCell;
                    job.targetQueueB.Add(storeCell);

                    int capacity = CapacityAt(nextThing.def, storeCell, map) - capacityOver;
                    storeCellCapacity[storeCell] = new CellAllocation(nextThing, capacity);
                    
                    Log.Message($"New cell {storeCell}:{capacity}, allocated extra {capacityOver}");
                }
                else
                {
                    count -= capacityOver;
                    job.countQueue.Add(count);
                    Log.Message($"Nowhere else to store, allocated {nextThing}:{count}");
                    return false;
                }
            }
            job.countQueue.Add(count);
            Log.Message($"{nextThing}:{count} allocated");
            return true;
        }

        public static HashSet<IntVec3> skipCells;
        public static bool TryFindBestBetterStoreCellFor(Thing thing, Pawn carrier, Map map, StoragePriority currentPriority, Faction faction, out IntVec3 foundCell)
        {
            foreach (SlotGroup slotGroup in map.haulDestinationManager.AllGroupsListInPriorityOrder
                .Where(s => s.Settings.Priority > currentPriority && s.parent.Accepts(thing)))
            {
                if (slotGroup.CellsList.Except(skipCells).FirstOrDefault(c => StoreUtility.IsGoodStoreCell(c, map, thing, carrier, faction)) is IntVec3 cell
                    && cell != default(IntVec3))
                {
                    foundCell = cell;
                    
                    skipCells.Add(cell);

                    return true;
                }
            }
            foundCell = IntVec3.Invalid;
            return false;
        }

        public static float AddedEnumberance(Pawn pawn, Thing thing)
        {
            return thing.stackCount * thing.GetStatValue(StatDefOf.Mass, true) / MassUtility.Capacity(pawn);
        }

        public static int CountPastCapacity(Pawn pawn, Thing thing, float encumberance)
        {
            return (int)Math.Ceiling((encumberance - 1) * MassUtility.Capacity(pawn) / thing.GetStatValue(StatDefOf.Mass, true));
        }

    }
}