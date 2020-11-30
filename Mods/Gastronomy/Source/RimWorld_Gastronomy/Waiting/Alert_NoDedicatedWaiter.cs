using System.Linq;
using RimWorld;
using Verse;

namespace Gastronomy.Waiting
{
	public class Alert_NoDedicatedWaiter : Alert
	{
		protected string explanationKey;
		private int nextCheckTick;
		private bool getReport;

		// ReSharper disable once PublicConstructorInAbstractClass
		public Alert_NoDedicatedWaiter()
		{
			defaultLabel = "NoDedicatedWaiter".Translate();
			defaultExplanation = "NoDedicatedWaiterExplanation".Translate();
			defaultPriority = AlertPriority.High;
		}
		public override string GetLabel()
		{
			return "NoDedicatedWaiter".Translate();
		}

		public override AlertReport GetReport()
		{
			if (GenTicks.TicksGame > nextCheckTick)
			{
				nextCheckTick = GenTicks.TicksGame + 90;
				CheckMaps();
			}

			return getReport;
		}

		private void CheckMaps()
		{
			getReport = false;

			if (!Settings.showAlertNoDedicatedWaiter) return;

			foreach (var map in Find.Maps)
			{
				if (!map.IsPlayerHome || !map.mapPawns.AnyColonistSpawned) continue;
				RestaurantController restaurant = map.GetComponent<RestaurantController>();
				if (restaurant?.IsOpenedRightNow == false) continue;
				if (!MapHasDiningSpots(map)) continue;
				if (!MapHasDedicatedWaiterRightNow(map))
				{
					getReport = true;
					break;
				}
			}
		}

		private static bool MapHasDedicatedWaiterRightNow(Map map)
		{
			return map.mapPawns.FreeColonistsSpawned.Where(c => c.workSettings.WorkIsActive(WaitingUtility.waitDef)).Any(HasToWorkRightNow);
		}

		private static bool HasToWorkRightNow(Pawn pawn)
		{
			var assignment = pawn.timetable.CurrentAssignment;
			return !assignment.allowJoy && !assignment.allowRest;
		}

		private static bool MapHasDiningSpots(Map map)
		{
			return map.GetComponent<RestaurantController>()?.diningSpots.Any() == true;
		}
	}
}
