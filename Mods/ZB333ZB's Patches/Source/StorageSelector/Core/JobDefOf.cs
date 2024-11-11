using RimWorld;
using Verse;

namespace StorageSelector.Core
{
    [DefOf]
    public static class JobDefOf
    {
        public static JobDef StorageHaul;

        static JobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
        }
    }
}
