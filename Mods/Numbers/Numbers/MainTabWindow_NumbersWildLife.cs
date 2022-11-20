namespace Numbers
{
    using System.Collections.Generic;
    using RimWorld;
    using Verse;

    public class MainTabWindow_NumbersWildLife : MainTabWindow_Numbers
    {
        public override void PostOpen()
        {
            pawnTableDef = NumbersDefOf.Numbers_WildAnimals;

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