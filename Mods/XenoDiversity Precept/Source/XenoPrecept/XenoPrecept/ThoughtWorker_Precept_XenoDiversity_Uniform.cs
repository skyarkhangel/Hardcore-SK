using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace XenoPrecept
{
    class ThoughtWorker_Precept_XenoDiversity_Uniform : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (p.Faction == null || !p.IsColonist)
			{
				return false;
			}
			List<Pawn> list = p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction);
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != p && list[i].RaceProps.Humanlike && !list[i].IsSlave && !list[i].IsQuestLodger())
				{
					if (list[i].def.label != p.def.label)
					{
						return false;
					}
					num++;
				}
			}
			return num > 0;
		}
	}
}
