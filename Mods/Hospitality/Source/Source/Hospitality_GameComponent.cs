using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality
{
	public class Hospitality_GameComponent : GameComponent
	{
		public FoodRestriction defaultFoodRestriction;

		// ReSharper disable once UnusedParameter.Local
		public Hospitality_GameComponent(Game _)
		{
			// Requires constructor
		}

		public override void StartedNewGame()
		{
			base.StartedNewGame();
			defaultFoodRestriction ??= new FoodRestriction(600, "Hospitality_Guests"); // Arbitrary ID
			ApplyFoodFilters(defaultFoodRestriction);
		}

		public override void ExposeData()
		{
			// Exposed, so it doesn't give the compressed away warning
			Scribe_Deep.Look(ref defaultFoodRestriction, "defaultFoodRestriction");
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				defaultFoodRestriction ??= new FoodRestriction(600, "Hospitality_Guests");
				ApplyFoodFilters(defaultFoodRestriction);
			}
		}

		private static void ApplyFoodFilters(FoodRestriction foodRestriction)
		{
			foodRestriction.filter.SetAllow(ThingCategoryDefOf.Foods, true);
			foodRestriction.filter.SetAllow(ThingCategoryDefOf.Drugs, true);
			//foodRestriction.filter.SetAllow(ThingCategoryDefOf.MeatRaw, false);
			foodRestriction.filter.SetAllow(ThingCategoryDefOf.CorpsesHumanlike, false);
			foodRestriction.filter.SetAllow(ThingCategoryDefOf.CorpsesAnimal, false);
			foodRestriction.filter.SetAllow(InternalDefOf.AllowRotten, false);
			//Log.Message($"Guest food restriction: {foodRestriction.filter.allowedDefs.Where(d=>foodRestriction.Allows(d)).Select(d=>d.label).ToCommaList()}");
		}
	}
}
