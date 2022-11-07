using System.Linq;
using System.Text;
using Hospitality.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hospitality
{
	public class JobGiver_OptimizeApparel_Guest : JobGiver_OptimizeApparel
	{
		public override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.IsGuest())
			{
				// For compatibility with Combat Extended which randomly calls up OptimizeApparel in all kinds of think trees and then expects a valid result
				return base.TryGiveJob(pawn);
			}

			pawn.outfits ??= new Pawn_OutfitTracker(pawn);

			if (!DebugViewSettings.debugApparelOptimize)
			{
				if (Find.TickManager.TicksGame < pawn.mindState.nextApparelOptimizeTick) return null;
			}
			else
			{
				debugSb = new StringBuilder();
				debugSb.AppendLine($"Scanning for {pawn} at {pawn.Position}");
			}

			if (Find.TickManager.TicksGame < pawn.mindState.nextApparelOptimizeTick) return null;

			var wornApparel = pawn.apparel.WornApparel;

			Apparel bestChoice = null;
			var highestApparelScoreGain = 0f;
			var list = pawn.inventory.innerContainer.OfType<Apparel>().ToArray();
			if (list.Length == 0)
			{
				SetNextOptimizeTick(pawn);
				return null;
			}
			neededWarmth = PawnApparelGenerator.CalculateNeededWarmth(pawn, pawn.Map.Tile, GenLocalDate.Twelfth(pawn));
			wornApparelScores.Clear();
			foreach (var apparel in wornApparel)
			{
				wornApparelScores.Add(ApparelScoreRaw(pawn, apparel));
			}
			foreach (var apparel in list)
			{
				if (!apparel.IsBurning() && (apparel.def.apparel.gender == Gender.None || apparel.def.apparel.gender == pawn.gender) && !apparel.CoversHead())
				{
					float apparelScoreGain = ApparelScoreGain(pawn, apparel, wornApparelScores);
					if (DebugViewSettings.debugApparelOptimize)
					{
						debugSb.AppendLine($"{apparel.LabelCap}: {apparelScoreGain:F2}");
					}
					if (!(apparelScoreGain < 0.05f) && !(apparelScoreGain < highestApparelScoreGain) && (!CompBiocodable.IsBiocoded(apparel) || CompBiocodable.IsBiocodedFor(apparel, pawn)) && ApparelUtility.HasPartsToWear(pawn, apparel.def) && pawn.CanReserveAndReach(apparel, PathEndMode.OnCell, pawn.NormalMaxDanger()))
					{
						bestChoice = apparel;
						highestApparelScoreGain = apparelScoreGain;
					}
				}
			}
			if (DebugViewSettings.debugApparelOptimize)
			{
				debugSb.AppendLine($"BEST: {bestChoice}");
				Log.Message(debugSb.ToString());
				debugSb = null;
			}
			if (bestChoice == null)
			{
				SetNextOptimizeTick(pawn);
				return null;
			}

			pawn.inventory.innerContainer.Remove(bestChoice);
			pawn.apparel.Wear(bestChoice, false);
			return null;
		}
	}
}
