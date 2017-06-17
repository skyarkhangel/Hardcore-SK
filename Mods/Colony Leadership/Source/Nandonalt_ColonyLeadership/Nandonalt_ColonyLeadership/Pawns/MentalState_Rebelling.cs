using RimWorld;
using Verse.AI;
using Verse;
using System;

namespace Nandonalt_ColonyLeadership
{
    public class MentalState_Rebelling : MentalState
    {
        public override bool ForceHostileTo(Thing t)
        {
            return false;
        }

        public override bool ForceHostileTo(Faction f)
        {
            return false;
        }

        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }
    }
}
