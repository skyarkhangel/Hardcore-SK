using System.Text;

using Verse;

namespace SK_Enviro.Seeds
{
    internal class PlantGenome : IExposable
    {
        public float growSpeedMult;
        public float harvestAmountMult;
        public float baseSeedChance;
        public float addSeedChance;
        public bool canGrowAtNight;

        public void ExposeData()
        {
            Scribe_Values.LookValue<float>(ref growSpeedMult, "growSpeedMult", 0f, false);
            Scribe_Values.LookValue<float>(ref harvestAmountMult, "harvestAmountMult", 0f, false);
            Scribe_Values.LookValue<float>(ref baseSeedChance, "baseSeedChance", 0f, false);
            Scribe_Values.LookValue<float>(ref addSeedChance, "addSeedChance", 0f, false);
            Scribe_Values.LookValue<bool>(ref canGrowAtNight, "canGrowAtNight", false, false);
        }

        public bool SameGenome(PlantGenome o)
        {
            if (growSpeedMult != o.growSpeedMult)
                return false;
            if (harvestAmountMult != o.harvestAmountMult)
                return false;
            if (baseSeedChance != o.baseSeedChance)
                return false;
            if (addSeedChance != o.addSeedChance)
                return false;
            if (canGrowAtNight != o.canGrowAtNight)
                return false;

            return true;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("GrowSpeedMult".Translate(growSpeedMult.ToString("P")));
            builder.AppendLine("HarvestAmountMult".Translate(harvestAmountMult.ToString("P")));
            builder.AppendLine("BaseSeedChance".Translate(baseSeedChance.ToString("P")));
            builder.AppendLine("AddSeedChance".Translate(addSeedChance.ToString("P")));
            builder.AppendLine("CanGrowAtNight".Translate(canGrowAtNight ? "Yes".Translate() : "No".Translate()));
            return builder.ToString();
        }
    }
}

