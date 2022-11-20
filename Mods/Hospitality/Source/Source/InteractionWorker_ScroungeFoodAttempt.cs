using System.Collections.Generic;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality
{
    public class InteractionWorker_ScroungeFoodAttempt : InteractionWorker
    {
        public override void Interacted(Pawn guest, Pawn target, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterDef = null;
            letterLabel = null;
            letterText = null;
            lookTargets = null;

            guest.ScroungedFoodFrom(target, false);
        }
    }
}