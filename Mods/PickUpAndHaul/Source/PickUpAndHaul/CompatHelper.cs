namespace PickUpAndHaul;

internal class CompatHelper
{
	public static bool CeOverweight(Pawn pawn)
	{
		return false;
		//var ceCompInventory = pawn.GetComp<CombatExtended.CompInventory>();
		//return (ceCompInventory.currentWeight / ceCompInventory.capacityWeight) >= Settings.MaximumOccupiedCapacityToConsiderHauling;
	}

	public static int CanFitInInventory(Pawn pawn, Thing thing)
	{
		return thing.stackCount;
		//pawn.GetComp<CombatExtended.CompInventory>().CanFitInInventory(thing, out var countToPickUp);
		//return countToPickUp;
	}

	internal static void UpdateInventory(Pawn pawn)
	{
		//pawn.GetComp<CombatExtended.CompInventory>().UpdateInventory();
	}
}