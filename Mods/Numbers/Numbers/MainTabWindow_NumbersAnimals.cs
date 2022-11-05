namespace Numbers
{
    using System.Collections.Generic;
    using RimWorld;
    using Verse;

    public class MainTabWindow_NumbersAnimals : MainTabWindow_Numbers
    {
        public override void PostOpen()
        {
            pawnTableDef = NumbersDefOf.Numbers_Animals;

            if (Find.World.GetComponent<WorldComponent_Numbers>().sessionTable
                    .TryGetValue(pawnTableDef, out List<PawnColumnDef> listPawnColumDef))
            {
                pawnTableDef.columns = listPawnColumDef;
            }

            UpdateFilter();
            Notify_ResolutionChanged();
            base.PostOpen();
        }
    }
}