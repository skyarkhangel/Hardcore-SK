using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace Hospitality
{
    public class ChoiceLetter_GuestJoinRequest : ChoiceLetter
    {
        public Pawn guest;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref guest, "guest");
        }

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (lookTargets.IsValid())
                {
                    yield return Option_Accept;
                    yield return Option_JumpToLocation;
                }

                yield return Option_Reject;
            }
        }

        private new DiaOption Option_JumpToLocation
        {
            get
            {
                var option = base.Option_JumpToLocation;
                option.action = () => CameraJumper.TryJumpAndSelect(guest);
                return option;
            }
        }

        private DiaOption Option_Accept =>
            new DiaOption("RansomDemand_Accept".Translate()) // This should be fine, it just says "Accept"
            {
                action = delegate {
                    DontGetUpset();
                    guest.Adopt();
                    CameraJumper.TryJump(guest.Position, guest.Map);
                    Find.LetterStack.RemoveLetter(this);
                },
                resolveTree = true
            };

        private void DontGetUpset()
        {
            if (guest.GetLord().ownedPawns.Count == 1) ((LordJob_VisitColony) guest.GetLord().LordJob).getUpsetWhenLost = false;
        }

        public override bool CanShowInLetterStack => base.CanShowInLetterStack && guest.IsValidPawn();
    }
}
