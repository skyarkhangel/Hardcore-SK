using Verse;

namespace SK_Enviro.Seeds
{
    internal class PlantGenomeProperties
    {
        public FloatRange growSpeedMult;
        public FloatRange harvestAmountMult;
        public FloatRange baseSeedChance;
        public FloatRange addSeedChance;
        public float canGrowAtNight = 0.5f;
        public float terminatorChance = 0.1f;

        public PlantGenome RandomGenome()
        {
            PlantGenome genome = new PlantGenome();
            genome.growSpeedMult = growSpeedMult.RandomInRange;
            genome.harvestAmountMult = harvestAmountMult.RandomInRange;
            genome.baseSeedChance = baseSeedChance.RandomInRange;
            if (genome.baseSeedChance >= 1f)
                genome.baseSeedChance = 1f;
            
            genome.addSeedChance = addSeedChance.RandomInRange;
            if (genome.addSeedChance >= 1f)
                genome.addSeedChance = 1f;
            
            if (Rand.Value < terminatorChance)
            {
                genome.baseSeedChance = 0f;
                genome.addSeedChance = 0f;
            }
            genome.canGrowAtNight = Rand.Value < canGrowAtNight;

            return genome;
        }
    }
}

