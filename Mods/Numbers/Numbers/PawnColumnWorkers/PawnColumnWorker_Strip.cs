namespace Numbers
{
    using RimWorld;
    using Verse;

    public class PawnColumnWorker_Strip : PawnColumnWorker_Designator
    {
        protected override DesignationDef DesignationType => DesignationDefOf.Strip;

        protected override bool HasCheckbox(Pawn pawn)
        {
            return StrippableUtility.CanBeStrippedByColony(pawn);
        }

        protected override void Notify_DesignationAdded(Pawn pawn)
        {
            StrippableUtility.CheckSendStrippingImpactsGoodwillMessage(pawn);
        }
    }
}
