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

        protected override ThoughtDef Thought => DefDatabase<ThoughtDef>.GetNamed("GuestCantAffordBed");
    }
}
