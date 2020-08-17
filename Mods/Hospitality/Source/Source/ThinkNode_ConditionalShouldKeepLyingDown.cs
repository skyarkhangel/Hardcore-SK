using RimWorld;
using Verse;
using Verse.AI;

namespace Hospitality {
    public class ThinkNode_ConditionalShouldKeepLyingDown : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn.CurJob != null && pawn.GetPosture().Laying() && (pawn.Downed || HealthAIUtility.ShouldSeekMedicalRest(pawn) || pawn.health.hediffSet.HasNaturallyHealingInjury() || pawn.jobs.curDriver.asleep);
        }
    }
}
