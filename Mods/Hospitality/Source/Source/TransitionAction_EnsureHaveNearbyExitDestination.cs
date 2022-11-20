using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Hospitality
{
	public class TransitionAction_EnsureHaveNearbyExitDestination : TransitionAction
	{
		public override void DoAction(Transition trans)
		{
			var target = (LordToil_Travel) trans.target;
			if (target.HasDestination()) return;
			var searcher = target.lord.ownedPawns.RandomElement();

			var closestTarget = IntVec3.Invalid;
			var roots = target.lord.ownedPawns.Select(p => p.Position).ToArray();

			bool Processor(IntVec3 x)
			{
				if (x.OnEdge(searcher.Map))
				{
					closestTarget = x;
					return true;
				}

				return false;
			}

			bool PassCheck(IntVec3 x, Danger maxDanger)
			{
				return x.Walkable(searcher.Map) && x.GetDangerFor(searcher, searcher.Map) <= maxDanger && (!(x.GetEdifice(searcher.Map) is Building_Door edifice) || edifice.CanPhysicallyPass(searcher));
			}

			// First try, for no danger
			searcher.Map.floodFiller.FloodFill(searcher.Position, x => PassCheck(x, Danger.None), Processor, int.MaxValue, false, roots);

			// Second try, some danger
			if (!closestTarget.IsValid)
			{
				searcher.Map.floodFiller.FloodFill(searcher.Position, x => PassCheck(x, Danger.Some), Processor, int.MaxValue, false, roots);
			}

			// Third try, any edge cell
			if (!closestTarget.IsValid)
			{
				//Log.Message($"{searcher.NameShortColored}: Couldn't find path to edge cell.");
				RCellFinder.TryFindRandomPawnEntryCell(out closestTarget, searcher.Map, 0.0f);
			}
			//Log.Message($"{searcher.NameShortColored}: Found closest edge cell: {closestTarget}");

			target.SetDestination(closestTarget);
		}
	}
}
