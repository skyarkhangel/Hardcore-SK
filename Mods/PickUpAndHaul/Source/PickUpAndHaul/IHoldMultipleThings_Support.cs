using System.Linq;

namespace PickUpAndHaul;
public class HoldMultipleThings_Support
{
	// ReSharper disable SuspiciousTypeConversion.Global
	public static bool CapacityAt(Thing thing, IntVec3 storeCell, Map map, out int capacity)
	{
		capacity = 0;

		if ((map.haulDestinationManager.SlotGroupParentAt(storeCell) as ThingWithComps)?
		   .AllComps.FirstOrDefault(x => x is IHoldMultipleThings.IHoldMultipleThings)
		   is IHoldMultipleThings.IHoldMultipleThings compOfHolding)
		{
			return compOfHolding.CapacityAt(thing, storeCell, map, out capacity);
		}

		foreach (var t in storeCell.GetThingList(map))
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
		if ((map.haulDestinationManager.SlotGroupParentAt(storeCell) as ThingWithComps)?
		   .AllComps.FirstOrDefault(x => x is IHoldMultipleThings.IHoldMultipleThings)
		   is IHoldMultipleThings.IHoldMultipleThings compOfHolding)
		{
			return compOfHolding.StackableAt(thing, storeCell, map);
		}

		foreach (var t in storeCell.GetThingList(map))
		{
			if (t is IHoldMultipleThings.IHoldMultipleThings holderOfMultipleThings)
            {
                return holderOfMultipleThings.StackableAt(thing, storeCell, map);
            }
        }

		return false;
	}
}