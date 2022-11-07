using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Hospitality.Spacer
{
    public abstract class IncidentWorker_VisitorGroupSpacer : IncidentWorker_VisitorGroup
    {
        protected override float ChanceToKnowEachPawn => 0.15f;

        public override bool CanFireNowSub(IncidentParms parms)
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
                PawnGenerationRequest request = new PawnGenerationRequest(
                    kind: options.RandomElementByWeight(o => o.selectionWeight).kind, 
                    faction: Faction.OfAncients, 
                    context: PawnGenerationContext.NonPlayer, 
                    tile: -1, 
                    forceGenerateNewPawn: false,
                    allowDead: false,
                    allowDowned: false,
                    canGeneratePawnRelations: false,
                    mustBeCapableOfViolence: false,
                    colonistRelationChanceFactor: 0,
                    forceAddFreeWarmLayerIfNeeded: true,
                    allowGay: true,
                    allowFood: true,
                    allowAddictions: false,
                    inhabitant: true);
                yield return PawnGenerator.GeneratePawn(request);
            }
        }
    }
}
