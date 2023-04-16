namespace PickUpAndHaul;
public class PawnUnloadChecker
{
	public static void CheckIfPawnShouldUnloadInventory(Pawn pawn, bool forced = false)
	{
		var job = JobMaker.MakeJob(PickUpAndHaulJobDefOf.UnloadYourHauledInventory, pawn);
		var itemsTakenToInventory = pawn?.GetComp<CompHauledToInventory>();

		if (itemsTakenToInventory == null)
        {
            return;
        }

        var carriedThing = itemsTakenToInventory.GetHashSet();

		if (pawn.Faction != Faction.OfPlayerSilentFail || !Settings.IsAllowedRace(pawn.RaceProps)
			|| carriedThing == null || carriedThing.Count == 0
			|| pawn.inventory.innerContainer is not { } inventoryContainer || inventoryContainer.Count == 0)
		{
			return;
		}

		if ((forced && job.TryMakePreToilReservations(pawn, false))
			|| ((MassUtility.EncumbrancePercent(pawn) >= 0.90f || carriedThing.Count >= 1)
			&& job.TryMakePreToilReservations(pawn, false)))
		{
			pawn.jobs.jobQueue.EnqueueFirst(job, JobTag.Misc);
			return;
		}

		if (inventoryContainer.Count >= 1)
		{
			for (var i = 0; i < inventoryContainer.Count; i++)
			{
				var compRottable = inventoryContainer[i].TryGetComp<CompRottable>();

				if (compRottable?.TicksUntilRotAtCurrentTemp < 30000)
				{
					pawn.jobs.jobQueue.EnqueueFirst(job, JobTag.Misc);
					return;
				}
			}
		}

		if (Find.TickManager.TicksGame % 50 == 0 && inventoryContainer.Count < carriedThing.Count)
		{
			Verse.Log.Warning("[PickUpAndHaul] " + pawn + " inventory was found out of sync with haul index. Pawn will drop their inventory.");
			carriedThing.Clear();
			pawn.inventory.UnloadEverything = true;
		}
	}
}

[DefOf]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Has to match defName")]
public static class PickUpAndHaulJobDefOf
{
	public static JobDef UnloadYourHauledInventory;
	public static JobDef HaulToInventory;
}