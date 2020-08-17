using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hospitality
{
    public class JobDriver_GuestImproveRelationship : JobDriver_GuestBase
    {
        public Toil TryImproveRelationship(Pawn recruiter, Pawn guest)
        {
            var toil = new Toil
            {
                initAction = () => {
                    if (!recruiter.CanTalkTo(guest)) return;
                    InteractionDef intDef = DefDatabase<InteractionDef>.GetNamed("GuestDiplomacy"); 
                    recruiter.interactions.TryInteractWith(guest, intDef);
                    PawnUtility.ForceWait(guest, 200, recruiter);
                },
                socialMode = RandomSocialMode.Off,
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 350
            };
            //toil.AddFailCondition(FailCondition);
            return toil;
        }

        protected override IEnumerable<Toil> Perform()
        {
            yield return TryImproveRelationship(pawn, Talkee);
        }

        protected override bool FailCondition()
        {
            return base.FailCondition() || !Talkee.ImproveRelationship();
        }

        protected override InteractionDef InteractionDef => InteractionDefOf.BuildRapport;
    }
}