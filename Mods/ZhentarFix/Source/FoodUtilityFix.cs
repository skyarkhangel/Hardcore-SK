using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace ZhentarFix
{
	class FoodUtilityFix
	{
		private static readonly Func<SimpleCurve> CurveGetter =
			Utils.GetStaticFieldAccessor<SimpleCurve>("FoodOptimalityEffectFromMoodCurve", typeof(FoodUtility));	

		private static SimpleCurve FoodOptimalityEffectFromMoodCurve => CurveGetter();

		//Fixes Nutrient Paste Dispenser optimality offset lookup
		[DetourMember(typeof(FoodUtility))]
		private static float FoodSourceOptimality(Pawn eater, Thing t, float dist)
		{
			float num = 300f;
			num -= dist;
			ThingDef thingDef = (!(t is Building_NutrientPasteDispenser)) ? t.def : ThingDefOf.MealNutrientPaste;
			FoodPreferability preferability = thingDef.ingestible.preferability;
			if (preferability != FoodPreferability.NeverForNutrition)
			{
				if (preferability == FoodPreferability.DesperateOnly)
				{
					num -= 150f;
				}
				CompRottable compRottable = t.TryGetComp<CompRottable>();
				if (compRottable != null)
				{
					if (compRottable.Stage == RotStage.Dessicated)
					{
						return -9999999f;
					}
					if (compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
					{
						num += 12f;
					}
				}
				if (eater.needs != null && eater.needs.mood != null)
				{
					List<ThoughtDef> list = FoodUtility.ThoughtsFromIngesting(eater, t);
					for (int i = 0; i < list.Count; i++)
					{
						num += FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].stages[0].baseMoodEffect);
					}
				}
				num += thingDef.ingestible.optimalityOffset;
				return num;
			}
			return -9999999f;
		}
	}
}
