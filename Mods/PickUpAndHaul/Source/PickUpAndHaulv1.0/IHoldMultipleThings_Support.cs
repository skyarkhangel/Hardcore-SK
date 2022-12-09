using Verse;

namespace PickUpAndHaul
{
    using System.Linq;

    public class HoldMultipleThings_Support
    {
        // ReSharper disable SuspiciousTypeConversion.Global
        public static bool CapacityAt(Thing thing, IntVec3 storeCell, Map map, out int capacity)
        {
            capacity = 0;

            var compOfHolding = (map.haulDestinationManager.SlotGroupParentAt(storeCell) as ThingWithComps)?
               .AllComps.FirstOrDefault(x => x is IHoldMultipleThings.IHoldMultipleThings);

            if (compOfHolding is IHoldMultipleThings.IHoldMultipleThings holderOfThings)
            {
                return holderOfThings.CapacityAt(thing, storeCell, map, out capacity);
            }

            foreach (Thing t in storeCell.GetThingList(map))
            {
                if (t is IHoldMultipleThings.IHoldMultipleThings holderOfMultipleThings)
                {
                    return holderOfMultipleThings.CapacityAt(thing, storeCell, map, out capacity);
                }
            }

            return false;
        }

        public static bool StackableAt(Thing thing, IntVec3 storeCell, Map map)
        {
            var compOfHolding = (map.haulDestinationManager.SlotGroupParentAt(storeCell) as ThingWithComps)?
               .AllComps.FirstOrDefault(x => x is IHoldMultipleThings.IHoldMultipleThings);

            if (compOfHolding is IHoldMultipleThings.IHoldMultipleThings holderOfThings)
            {
                return holderOfThings.StackableAt(thing, storeCell, map);
            }

            foreach (Thing t in storeCell.GetThingList(map))
            {
                if (t is IHoldMultipleThings.IHoldMultipleThings holderOfMultipleThings)
                {
                    return holderOfMultipleThings.StackableAt(thing, storeCell, map);
                }
            }

            return false;
        }
    }
}
