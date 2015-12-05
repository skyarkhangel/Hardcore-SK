using System.Text;

using UnityEngine;

using Verse;
using RimWorld;

namespace SK_Enviro.Seeds
{
    internal class PlantWithSeeds : Plant
    {
        public PlantGenome genome;
        internal ThingDef_PlantWithSeeds m_CustomPlantDef;

        public override string GetDescription()
        {
            var sDescString = base.GetDescription();

            return sDescString + "\n\n"  + "Genome".Translate() + "\n" + genome;
        }

        public override string GetInspectString()
        {
            StringBuilder builder = new StringBuilder();
            if (LifeStage == PlantLifeStage.Growing)
            {
                builder.AppendLine("PercentGrowth".Translate((growth + 0.0001f).ToStringPercent()));
                builder.AppendLine("GrowthRate".Translate() + ": " + GrowthRate.ToStringPercent());
                if (!genome.canGrowAtNight && (GenDate.CurrentDayPercent < 0.25f || GenDate.CurrentDayPercent > 0.8f))
                    builder.AppendLine("PlantResting".Translate());
                else
                {
                    if (GrowthRateFactor_Light <= 0.001f)
                        builder.AppendLine("PlantNeedsLightLevel".Translate() + ": " + def.plant.growMinGlow.ToStringPercent());
                    
                    float num = GrowthRateFactor_Temperature;
                    if (num < 0.99f)
                    {
                        if (num < 0.01f)
                            builder.AppendLine("OutOfIdealTemperatureRangeNotGrowing".Translate());
                        else
                            builder.AppendLine("OutOfIdealTemperatureRange".Translate(Mathf.RoundToInt(num * 100f).ToString()));
                    }
                }
            }
            else if (LifeStage == PlantLifeStage.Mature)
            {
                if (def.plant.Harvestable)
                    builder.AppendLine("ReadyToHarvest".Translate());  
                else
                    builder.AppendLine("Mature".Translate());
            }
            return builder.ToString();
        }

        public override void PostMake()
        {
            base.PostMake();
            m_CustomPlantDef = def as ThingDef_PlantWithSeeds;
            if (genome == null)
                genome = m_CustomPlantDef.genome.RandomGenome();
        }

        public override void TickRare()
        {
            //float num = 8f;
            //float single1 = (this.HashOffset() * 0.01f) % num;
            if (base.Position.GetTemperature() < -2f)
                TakeDamage(new DamageInfo(DamageDefOf.Rotting, 99999, null, null, null));
            
            if (GenPlant.GrowthSeasonNow(base.Position))
            {
                WildSpawner.WildSpawnerTick();
                bool flag;
                float num2 = 0f;
                if ((LifeStage != PlantLifeStage.Growing) || (!genome.canGrowAtNight && (GenDate.CurrentDayPercent < 0.25f) || GenDate.CurrentDayPercent > 0.8f))
                    num2 = 0f;
                else
                    num2 = (1f / (30000f * def.plant.growDays)) * GrowthRate;

                if (!def.plant.Sowable)
                    flag = false;
                else
                {
                    Zone zone = Find.ZoneManager.ZoneAt(base.Position);
                    if (zone is Zone_Growing)
                        flag = true;
                    else
                    {
                        Building edifice = base.Position.GetEdifice();
                        flag = (edifice != null) && edifice.def.building.SupportsPlants;
                    }
                }
                bool flag2 = LifeStage == PlantLifeStage.Mature;
                growth += (num2 * 250f) * genome.growSpeedMult;
                if (!flag2 && LifeStage == PlantLifeStage.Mature && flag)
                    Find.MapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
                if (def.plant.LimitedLifespan)
                {
                    age += 250;
                    if (Rotting)
                    {
                        int amount = Mathf.CeilToInt(1.25f);
                        TakeDamage(new DamageInfo(DamageDefOf.Rotting, amount, null, null, null));
                    }
                }
                if (!Destroyed && !flag)
                    //GenPlantReproduction.TrySpawnSeed(Position, def, SeedTargFindMode.ReproduceSeed, this);
                    TickReproduceFrom(this);
            }
        }

        // Seems like an outdated function, but it was needed.
        private static void TickReproduceFrom(Plant plant)
        {
            if (!plant.def.plant.shootsSeeds || plant.growth < 0.600000023841858 || (!Rand.MTBEventOccurs(plant.def.plant.seedEmitMTBDays, 30000f, 250f) || !GenPlant.SnowAllowsPlanting(plant.Position)) || (!GenPlant.GrowthSeasonNow(plant.Position) || plant.Position.Roofed()))
                return;
            GenPlantReproduction.TrySpawnSeed(plant.Position, plant.def, SeedTargFindMode.ReproduceSeed, plant);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            m_CustomPlantDef = def as ThingDef_PlantWithSeeds;
            Scribe_Deep.LookDeep<PlantGenome>(ref genome, "genome");
            if (genome == null)
                genome = m_CustomPlantDef.genome.RandomGenome();
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            m_CustomPlantDef = def as ThingDef_PlantWithSeeds;
        }

        public new virtual int YieldNow()
        {
            if (!HarvestableNow)
                return 0;

            if (this.def.plant.harvestYield * this.genome.harvestAmountMult <= 1f)
                return Mathf.RoundToInt(this.def.plant.harvestYield * this.genome.harvestAmountMult);

            Mathf.InverseLerp(this.def.plant.harvestMinGrowth, 1f, this.growth);
            int num = Gen.RandomRoundToInt(this.def.plant.harvestYield * this.genome.harvestAmountMult * Mathf.Lerp(0.5f, 1f, (float)base.HitPoints / (float)base.MaxHitPoints) * Find.Storyteller.difficulty.cropYieldFactor);
            if (num < 2)
            {
                num = 2;
            }
            return num;
        }

    }
}

