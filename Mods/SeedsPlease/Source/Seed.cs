using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace SeedsPlease
{
    public class Seed : ThingWithComps
    {

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            if (def is SeedDef seedDef) {

                StatDrawEntry[] extraStats = {
                    new StatDrawEntry (StatCategoryDefOf.PawnWork, ResourceBank.StringPlantMinFertlity , seedDef.plant.plant.fertilityMin.ToString ("P0"), string.Empty, 0),
                    new StatDrawEntry (StatCategoryDefOf.PawnMisc, ResourceBank.StringPlantFertilitySensitivity, seedDef.plant.plant.fertilitySensitivity.ToString ("P0"), string.Empty, 0),
                    new StatDrawEntry (StatCategoryDefOf.PawnMisc, ResourceBank.StringPlantGrowDays, seedDef.plant.plant.growDays.ToString ("F1"), string.Empty, 0),
                    new StatDrawEntry (StatCategoryDefOf.PawnMisc, ResourceBank.StringHarvestMultiplier , seedDef.seed.harvestFactor.ToString ("P0"), string.Empty, 0),
                    new StatDrawEntry (StatCategoryDefOf.PawnMisc, ResourceBank.StringSeedMultiplier, seedDef.seed.seedFactor.ToString ("P0"), string.Empty, 0),
                    new StatDrawEntry (StatCategoryDefOf.PawnMisc, ResourceBank.StringSeedBaseChance, seedDef.seed.baseChance.ToString ("P0"), string.Empty, 0),
                    new StatDrawEntry (StatCategoryDefOf.PawnMisc, ResourceBank.StringSeedExtraChance, seedDef.seed.extraChance.ToString ("P0"), string.Empty, 0),
                };
                return base.SpecialDisplayStats().Concat(extraStats);
            }

            return base.SpecialDisplayStats();
        }

        public override string GetInspectString ()
        {
            var inspectString = base.GetInspectString ();

            if (def is SeedDef seedDef && seedDef.plant.plant.fertilityMin > 1.0f) {
                inspectString += ResourceBank.StringPlantMinFertlity + " : " + seedDef.plant.plant.fertilityMin.ToString("P0");
            }
            return inspectString;
        }
    }
}

