using System.Linq;

namespace PickUpAndHaul;
public class JobDriver_UnloadYourHauledInventory : JobDriver
{
	private int _countToDrop = -1;
	private int _unloadDuration = 3;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look<int>(ref _countToDrop, "countToDrop", -1);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

	/// <summary>
	/// Find spot, reserve spot, pull thing out of inventory, go to spot, drop stuff, repeat.
	/// </summary>
	/// <returns></returns>
	public override IEnumerable<Toil> MakeNewToils()
	{
		var takenToInventory = pawn.TryGetComp<CompHauledToInventory>();
		var carriedThing = takenToInventory.GetHashSet();

		if (ModCompatibilityCheck.ExtendedStorageIsActive)
		{
			_unloadDuration = 20;
		}

		var wait = Toils_General.Wait(_unloadDuration);
		var celebrate = Toils_General.Wait(_unloadDuration);

		yield return wait;
		yield return new()
		{
			initAction = () =>
			{
				var unloadableThing = FirstUnloadableThing(pawn);

				if (unloadableThing.Count == 0 && carriedThing.Count == 0)
				{
					EndJobWith(JobCondition.Succeeded);
				}

				if (unloadableThing.Count != 0)
				{
					//StoragePriority currentPriority = StoreUtility.StoragePriorityAtFor(pawn.Position, unloadableThing.Thing);
					if (!StoreUtility.TryFindStoreCellNearColonyDesperate(unloadableThing.Thing, pawn, out var c))
					{
						pawn.inventory.innerContainer.TryDrop(unloadableThing.Thing, ThingPlaceMode.Near, unloadableThing.Thing.stackCount, out var _);
						EndJobWith(JobCondition.Succeeded);
					}
					else
					{
						job.SetTarget(TargetIndex.A, unloadableThing.Thing);
						job.SetTarget(TargetIndex.B, c);
						_countToDrop = unloadableThing.Thing.stackCount;
					}
				}
			}
		};

		yield return Toils_Reserve.Reserve(TargetIndex.B);

		yield return new()
		{
			initAction = () =>
			{
				var thing = job.GetTarget(TargetIndex.A).Thing;
				if (thing == null || !pawn.inventory.innerContainer.Contains(thing))
				{
					carriedThing.Remove(thing);
					pawn.jobs.curDriver.JumpToToil(wait);
					return;
				}
				if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !thing.def.EverStorable(false))
				{
					pawn.inventory.innerContainer.TryDrop(thing, ThingPlaceMode.Near, _countToDrop, out thing);
					EndJobWith(JobCondition.Succeeded);
					carriedThing.Remove(thing);
				}
				else
				{
					pawn.inventory.innerContainer.TryTransferToContainer(thing, pawn.carryTracker.innerContainer, _countToDrop, out thing);
					job.count = _countToDrop;
					job.SetTarget(TargetIndex.A, thing);
					carriedThing.Remove(thing);
				}

				if (ModCompatibilityCheck.CombatExtendedIsActive)
				{
					 CompatHelper.UpdateInventory(pawn);
				}

				thing.SetForbidden(false, false);
			}
		};

		if (TargetB.HasThing)
		{
			var carryToContainer = Toils_Haul.CarryHauledThingToContainer();
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
			yield return carryToContainer;
			yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.None);
			yield return Toils_Haul.JumpToCarryToNextContainerIfPossible(carryToContainer, TargetIndex.B);
		}
		else
		{
			var carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.Touch);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
		}

		//If the original cell is full, PlaceHauledThingInCell will set a different TargetIndex resulting in errors on yield return Toils_Reserve.Release.
		//We still gotta release though, mostly because of Extended Storage.
		yield return new()
		{
			initAction = () =>
			{
				if (pawn.Map.reservationManager.ReservedBy(job.targetB, pawn, pawn.CurJob)
				 && !ModCompatibilityCheck.HCSKIsActive)
				{
					pawn.Map.reservationManager.Release(job.targetB, pawn, pawn.CurJob);
				}
			}
		};
		yield return Toils_Jump.Jump(wait);
		yield return celebrate;
	}

	private static ThingCount FirstUnloadableThing(Pawn pawn)
	{
		var itemsTakenToInventory = pawn.TryGetComp<CompHauledToInventory>();
		var carriedThings = itemsTakenToInventory.GetHashSet();
		var innerPawnContainer = pawn.inventory.innerContainer;

		foreach (var thing in carriedThings.OrderBy(t => t.def.FirstThingCategory?.index).ThenBy(x => x.def.defName))
		{
			//find the overlap.
			if (!innerPawnContainer.Contains(thing))
			{
				//merged partially picked up stacks get a different thingID in inventory
				var stragglerDef = thing.def;
				carriedThings.Remove(thing);

				//we have no method of grabbing the newly generated thingID. This is the solution to that.
				for (var i = 0; i < innerPawnContainer.Count; i++)
				{
					var dirtyStraggler = innerPawnContainer[i];
					if (dirtyStraggler.def == stragglerDef)
					{
						return new ThingCount(dirtyStraggler, dirtyStraggler.stackCount);
					}
				}
			}
			return new ThingCount(thing, thing.stackCount);
		}
		return default;
	}
}