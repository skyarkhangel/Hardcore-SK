using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hospitality
{
    public class JobDriver_CharmGuest : JobDriver_GuestBase
    {
        protected override InteractionDef InteractionDef => InteractionDefOf.RecruitAttempt;

        public Toil TryMakeFriends(Pawn recruiter, Pawn guest)
        {
            var toil = new Toil
            {
                initAction = () => {
                    if (!recruiter.ShouldMakeFriends(guest)) return;
                    if (!recruiter.CanTalkTo(guest)) return;
                    InteractionDef intDef = DefDatabase<InteractionDef>.GetNamed("CharmGuestAttempt");
                    recruiter.interactions.TryInteractWith(guest, intDef);
                    PawnUtility.ForceWait(guest, 200, recruiter);
                    //guest.CheckRecruitingSuccessful(recruiter);
                },
                socialMode = RandomSocialMode.Off,
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 350
            };
            toil.AddFailCondition(FailCondition);
            return toil;
        }

        protected override IEnumerable<Toil> Perform()
        {
            yield return TryMakeFriends(pawn, Talkee);
        }

        protected override bool FailCondition()
        {
            return base.FailCondition() || !Talkee.MakeFriends();
        }
    }
}