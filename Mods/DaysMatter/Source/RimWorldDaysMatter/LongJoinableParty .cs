using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimWorldDaysMatter
{
    public class LongJoinableParty : JoinableParty
    {
        public LongJoinableParty(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef, List<Pawn> invited = null)
            : base(spot, organizer, gatheringDef, invited)
        {
            durationTicks = Rand.RangeInclusive(20000, 30000);
        }
    }
}
