namespace PickUpAndHaul;

public class Settings : ModSettings
{
	private static bool _allowCorpses;
	private static bool _allowAnimals = true;
	private static bool _allowMechanoids = true;
	private static float _maximumOccupiedCapacityToConsiderHauling = 0.8f;

	public static bool AllowCorpses => _allowCorpses;
	public static bool AllowAnimals => _allowAnimals;
	public static bool AllowMechanoids => _allowMechanoids;
	public static float MaximumOccupiedCapacityToConsiderHauling => _maximumOccupiedCapacityToConsiderHauling;

	public static bool IsAllowedRace(RaceProperties props) => props.Humanlike || (AllowAnimals && props.Animal) || (AllowMechanoids && props.IsMechanoid);

	public static void DoSettingsWindowContents(Rect inRect)
	{
		var ls = new Listing_Standard();
		ls.Begin(inRect);
		ls.CheckboxLabeled("PUAH.allowCorpses".Translate(), ref _allowCorpses, "PUAH.allowCorpsesTooltip".Translate());
		ls.CheckboxLabeled("PUAH.allowAnimals".Translate(), ref _allowAnimals, "PUAH.allowAnimalsTooltip".Translate());
		ls.CheckboxLabeled("PUAH.allowMechanoids".Translate(), ref _allowMechanoids, "PUAH.allowMechanoidsTooltip".Translate());
		var minimumFreeInventorySpace = (float)Math.Round((1 - _maximumOccupiedCapacityToConsiderHauling) * 100f);
		var previousAlignment = Text.Anchor;
		Text.Anchor = TextAnchor.MiddleRight;
		Widgets.Label(inRect with { y = ls.curY, height = Text.CalcHeight("20%", ls.ColumnWidth) }, $"{Convert.ToInt32(minimumFreeInventorySpace)}%");
		Text.Anchor = previousAlignment;
		ls.Label("PUAH.minimumFreeInventorySpace".Translate(), tooltip: "PUAH.minimumFreeInventorySpaceTooltip".Translate());
		var newFreeInventorySpaceValue = Math.Round(ls.Slider(minimumFreeInventorySpace, 0, 100));
		if (newFreeInventorySpaceValue != minimumFreeInventorySpace)
        {
            _maximumOccupiedCapacityToConsiderHauling = (float)Math.Round((100d - newFreeInventorySpaceValue) * 0.01, 2);
        }

        ls.End();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref _allowCorpses, "allowCorpses");
		Scribe_Values.Look(ref _allowAnimals, "allowAnimals", true);
		Scribe_Values.Look(ref _allowMechanoids, "allowMechanoids", true);
		Scribe_Values.Look(ref _maximumOccupiedCapacityToConsiderHauling, "maximumOccupiedCapacityToConsiderHauling", 0.8f);
	}
}