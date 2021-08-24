using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace XenoPrecept
{
	public class ThoughtWorker_Precept_LeaderXenoResentment : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			Faction faction = p.Faction;
			Pawn pawn = (faction != null) ? faction.leader : null;
			return pawn != null && p.def.label != pawn.def.label;
		}
	}
}
