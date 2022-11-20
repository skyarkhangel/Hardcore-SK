using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality
{
    public class Alert_GuestCantClaimBed : Alert_GuestThought
    {
        public Alert_GuestCantClaimBed()
        {
            defaultLabel = "AlertCantClaimBed".Translate();
            explanationKey = "AlertCantClaimBedDesc";
        }

        protected override ThoughtDef Thought => InternalDefOf.GuestCantAffordBed;
        private protected override int Hash => 6237;
    }
}
