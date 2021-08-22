using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace XenoPrecept
{
	public class ThoughtWorker_Precept_XenoDiversity_Social : ThoughtWorker_Precept_Social
	{
		protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
		{
			return p.Faction == otherPawn.Faction && p.def.label != otherPawn.def.label ;
		}
	}
}
