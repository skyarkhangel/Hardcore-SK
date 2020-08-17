using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Hospitality.Spacer
{
    public abstract class IncidentWorker_VisitorGroupSpacer : IncidentWorker_VisitorGroup
    {
        protected override float ChanceToKnowEachPawn => 0.15f;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return false; // TODO: Remove when ready for random events
        }

        protected override IEnumerable<Pawn> GenerateNewPawns(IncidentParms parms, int preferredAmount)
        {
            // TODO: Use custom ship def class to specify PawnGenOptions
            return GeneratePawns(preferredAmount, new List<PawnGenOption>());
            // TODO: Generate relations among guests?
        }

        private static IEnumerable<Pawn> GeneratePawns(int amount, List<PawnGenOption> options)
        {
            for (int i = 0; i < amount; i++)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(options.RandomElementByWeight(o => o.selectionWeight).kind, Faction.OfAncients, PawnGenerationContext.NonPlayer, -1, false, false, false, false, false, false, 0,
                    true, true, true, false, true);
                yield return PawnGenerator.GeneratePawn(request);
            }
        }
    }
}
