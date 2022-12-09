namespace PickUpAndHaul;

public class CompHauledToInventory : ThingComp
{
	private HashSet<Thing> takenToInventory = new();

	public HashSet<Thing> GetHashSet()
	{
		takenToInventory.RemoveWhere(x => x == null);
		return takenToInventory;
	}

	public void RegisterHauledItem(Thing thing) => takenToInventory.Add(thing);

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Collections.Look(ref takenToInventory, "ThingsHauledToInventory", LookMode.Reference);
	}
}