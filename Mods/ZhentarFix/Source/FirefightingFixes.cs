using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace ZhentarFix
{
	class FirefightingFixes
	{
		class PersonalShieldDetour : PersonalShield
		{
			[DetourMember]
			public override bool AllowVerbCast(IntVec3 root, TargetInfo targ)
			{
				if (targ.HasThing && targ.Thing.def.size != IntVec2.One)
				{
					return root.AdjacentTo8WayOrInside(targ.Thing);
				}
				return root.AdjacentTo8WayOrInside(targ.Cell);
			}
		}

		class Pawn_StoryTrackerDetour : Pawn_StoryTracker
		{
			public Pawn_StoryTrackerDetour(Pawn pawn) : base(pawn)
			{ }

			[DetourMember]
			public bool WorkTagIsDisabled(WorkTags w)
			{
				if (w == WorkTags.Firefighting) { w = w | WorkTags.Scary; }
				return (CombinedDisabledWorkTags & w) != WorkTags.None;
			}
		}
	}
}
