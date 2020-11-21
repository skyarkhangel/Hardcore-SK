using Verse;

namespace Gastronomy.Waiting
{
    public static class WaitingUtility
    {
        public static readonly JobDef takeOrderDef = DefDatabase<JobDef>.GetNamed("Gastronomy_TakeOrder");
        public static readonly JobDef serveDef = DefDatabase<JobDef>.GetNamed("Gastronomy_Serve");
        public static readonly JobDef makeTableDef = DefDatabase<JobDef>.GetNamed("Gastronomy_MakeTable");

        public static readonly WorkTypeDef waitDef = DefDatabase<WorkTypeDef>.GetNamed("Gastronomy_Waiting");
    }
}
