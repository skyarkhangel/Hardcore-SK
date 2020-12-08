using HarmonyLib;
using RimWorld;
using Verse;

namespace ExperimentalOptimizations.Fix
{
    [FixOn(InitStage.ModInit)]
    public class CSL
    {
        public static void Patch()
        {
            if (ModLister.GetActiveModWithIdentifier("Dylan.CSL") != null)
            {
                var maxLevel = AccessTools.Property(typeof(Need_Food), nameof(Need_Food.MaxLevel)).GetGetMethod();
                maxLevel.Unpatch(HarmonyPatchType.Prefix, "Children");
                maxLevel.Patch(prefix: typeof(CSL).Method(nameof(MaxLevel_Pre)).ToHarmonyMethod());
                Log.Message($"[ExperimentalOptimizations] Children.ChildrenHarmony.Need_Food_Patch:MaxLevel_Pre fixed");
            }
        }

        private static bool MaxLevel_Pre(Need_Food __instance, ref float __result)
        {
            var pawn = __instance.pawn;
            if (pawn == null)
                return true;

            int curLifeStageIdx = pawn.ageTracker.CurLifeStageIndex;
            var lifeStageAges = pawn.RaceProps.lifeStageAges;
            if (curLifeStageIdx >= lifeStageAges.Count)
                return true;

            var curLifeStage = lifeStageAges[curLifeStageIdx].def;

            // pawn.ageTracker.CurLifeStage.foodMaxFactor;
            float foodMaxFactor = curLifeStage.foodMaxFactor;

            // pawn.BodySize * foodMaxFactor
            __result = curLifeStage.bodySizeFactor * pawn.RaceProps.baseBodySize * foodMaxFactor;
            if (pawn.ageTracker.AgeBiologicalYears >= 13)
            {
                if (__result < 0.85f)
                {
                    __result = 0.85f;
                }
            }
            else if (__result < 0.6f)
            {
                __result = 0.6f;
            }
            return false;

            /*
            public int CurLifeStageIndex {
			    if (this.cachedLifeStageIndex < 0)
				    this.RecalculateLifeStageIndex();
				return this.cachedLifeStageIndex;
		    }
		    public LifeStageDef CurLifeStage => this.CurLifeStageRace.def;
		    public LifeStageAge CurLifeStageRace => this.pawn.RaceProps.lifeStageAges[this.CurLifeStageIndex];
            public float BodySize => this.ageTracker.CurLifeStage.bodySizeFactor * this.RaceProps.baseBodySize;
             */
        }
    }
}