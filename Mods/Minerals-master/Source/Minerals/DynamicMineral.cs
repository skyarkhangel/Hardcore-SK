
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;      // RimWorld specific functions 
using RimWorld.Planet;
using Verse;         // RimWorld universal objects 

namespace Minerals
{
    /// <summary>
    /// Mineral class
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class DynamicMineral : StaticMineral
    {
        // Controls how often occasional checks are done, like distance to nearby things
        private int tickCounter = Rand.Range(0, 1000);

        public new ThingDef_DynamicMineral attributes
        {
            get
            {
                return base.attributes as ThingDef_DynamicMineral;
            }
        }


        public override float distFromNeededTerrain
        {
            get
            {
                int ticksPerUpdate = 20;
                if (myDistFromNeededTerrain == null || tickCounter % ticksPerUpdate == 0) // not yet set
                {
                    myDistFromNeededTerrain = attributes.posDistFromNeededTerrain(Map, Position);
                }

                return (float)myDistFromNeededTerrain;
            }

            set
            {
                myDistFromNeededTerrain = value;
            }
        }



        public virtual float GrowthRate
        {
            get
            {
                float output = 1f; // If there are no growth rate factors, grow at full speed

                // Get growth rate factors
                List<float> rateFactors = allGrowthRateFactors;
                List<float> positiveFactors = rateFactors.FindAll(fac => fac >= 0);
                List<float> negativeFactors = rateFactors.FindAll(fac => fac < 0);

                // if any factors are negative, add them together and ignore positive factors
                if (negativeFactors.Count > 0)
                {
                    output = negativeFactors.Sum();
                }
                else if (positiveFactors.Count > 0) // if all positive, multiply them
                {
                    output = positiveFactors.Aggregate(1f, (acc, val) => acc * val);
                }


                return output * MineralsMain.Settings.mineralGrowthSetting;
            }
        }



        public float GrowthPerTick
        {
            get
            {
                float growthPerTick = (1f / (GenDate.TicksPerDay * attributes.growDays));
                return growthPerTick * GrowthRate;
            }
        }


        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString());
            stringBuilder.AppendLine("Growth rate: " + GrowthRate.ToStringPercent());
            if (DebugSettings.godMode)
            {
                foreach (growthRateModifier mod in attributes.allRateModifiers)
                {
                    stringBuilder.AppendLine(mod.GetType().Name + ": " + mod.growthRateFactorAtPos(this));
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }


        public override void TickLong()
        {
            // Half the time, dont do anything
            if (Rand.Bool)
            {
                return;
            }

            // Try to grow
            float GrowthThisTick = GrowthPerTick;
            size += GrowthThisTick * 4000; // 1 long tick = 2000

            // Try to reproduce
            if (GrowthThisTick > 0 && size > attributes.minReproductionSize && Rand.Range(0f, 1f) < attributes.reproduceProp * GrowthRate * MineralsMain.Settings.mineralReproductionSetting)
            {
                attributes.TryReproduce(Map, Position);
            }

            // Refresh appearance if apparent size has changed
            float apparentSize = printSize();
            float sizeDiff = Math.Abs(sizeWhenLastPrinted - apparentSize);
            if (sizeDiff > 0.1f || (sizeDiff > 0.02f && attributes.fastGraphicRefresh))
            {
                sizeWhenLastPrinted = apparentSize;
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
                initializeTextureLocations();
            }

            // Count ticks for occasional updates, like dist to nearby terrain 
            tickCounter += 1;

            // Try to die
            if (size <= 0 && Rand.Range(0f, 1f) < attributes.deathProb)
            {
                Destroy(DestroyMode.Vanish);
            }

        }
            
        public List<float> allGrowthRateFactors 
        {
            get
            {
                return attributes.allRateModifiers.Select(mod => mod.growthRateFactorAtPos(this)).ToList();
            }
        }


    }       





    /// <summary>
    /// ThingDef_StaticMineral class.
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class ThingDef_DynamicMineral : ThingDef_StaticMineral
    {
        // The number of days it takes to grow at max growth speed
        public float growDays = 100f;


        public float minReproductionSize = 0.8f;
        public float reproduceProp = 0.001f;
        public float deathProb = 0.001f;
        public float spawnProb = 0.0001f; // chance of spawning de novo each tick
        public tempGrowthRateModifier tempGrowthRateModifer;  // Temperature effects on growth rate
        public rainGrowthRateModifier rainGrowthRateModifer;  // Rain effects on growth rate
        public lightGrowthRateModifier lightGrowthRateModifer; // Light effects on growth rate
        public fertGrowthRateModifier fertGrowthRateModifer;  // Fertility effects on growth rate
        public distGrowthRateModifier distGrowthRateModifer;  // Distance to needed terrain effects on growth rate
        public sizeGrowthRateModifier sizeGrowthRateModifer;  // Current size effects on growth rate
        public bool fastGraphicRefresh = false; // If true, the graphics are regenerated more often
        public int minSpawnClusterSize = 1; // The minimum number of crystals in clusters that are spawned during gameplay, not map creation
        public int maxSpawnClusterSize = 1; // The maximum number of crystals in clusters that are spawned during gameplay, not map creation


        public List<growthRateModifier> allRateModifiers 
        {
            get 
            {
                List<growthRateModifier> output = new List<growthRateModifier>{
                    tempGrowthRateModifer,
                    rainGrowthRateModifer,
                    lightGrowthRateModifer,
                    fertGrowthRateModifer,
                    distGrowthRateModifer,
                    sizeGrowthRateModifer
                };
                output.RemoveAll(item => item == null);
                return output;
            }
        }

        public List<growthRateModifier> mapRateModifiers
        {
            get
            {
                List<growthRateModifier> output = new List<growthRateModifier>{
                    tempGrowthRateModifer,
                    rainGrowthRateModifer,
                    lightGrowthRateModifer,
                    fertGrowthRateModifer,
                    distGrowthRateModifer,
                    sizeGrowthRateModifer
                };
                output.RemoveAll(item => item == null || (!item.wholeMapEffect));
                return output;
            }
        }

        public List<growthRateModifier> posRateModifiers
        {
            get
            {
                List<growthRateModifier> output = new List<growthRateModifier>{
                    tempGrowthRateModifer,
                    rainGrowthRateModifer,
                    lightGrowthRateModifer,
                    fertGrowthRateModifer,
                    distGrowthRateModifer,
                    sizeGrowthRateModifer
                };
                output.RemoveAll(item => item == null || item.wholeMapEffect);
                return output;
            }
        }


        public override void InitNewMap(Map map, float scaling = 1)
        {
            scaling = scaling * GrowthRateMapRecent(map);
            base.InitNewMap(map, scaling);
        }


        // ======= Growth rate factors ======= //
        public virtual float combineGrowthRateFactors(List<float> rateFactors)
        {
            List<float> positiveFactors = rateFactors.FindAll(fac => fac >= 0);
            List<float> negativeFactors = rateFactors.FindAll(fac => fac < 0);

            // if any factors are negative, add them together and ignore positive factors
            if (negativeFactors.Count > 0)
            {
                return negativeFactors.Sum();
            }

            // if all positive, multiply them
            if (positiveFactors.Count > 0)
            {
                return positiveFactors.Aggregate(1f, (acc, val) => acc * val);
            }

            // If there are no growth rate factors, grow at full speed
            return 1f;
        }

        public virtual List<float> allGrowthRateFactorsAtPos(IntVec3 aPosition, Map aMap, bool includePerMapEffects = true)
        {
            if (includePerMapEffects)
            {
                return allRateModifiers.Select(mod => mod.growthRateFactorAtPos(this, aPosition, aMap)).ToList();
            }
            else
            {
                return posRateModifiers.Select(mod => mod.growthRateFactorAtPos(this, aPosition, aMap)).ToList();
            }
        }

        public virtual List<float> allGrowthRateFactorsAtMap(Map aMap)
        {
            return mapRateModifiers.Select(mod => mod.growthRateFactorAtMap(aMap)).ToList();
        }

        public virtual List<float> allGrowthRateFactorsAtMapMean(Map aMap)
        {
            return allRateModifiers.Select(mod => mod.growthRateFactorMapMean(aMap)).ToList();
        }
        public virtual List<float> allGrowthRateFactorsMapRecent(Map aMap)
        {
            //foreach (growthRateModifier mod in allRateModifiers)
            //{
            //    Log.Message("GrowthRateMapRecent: " + mod.GetType().Name + ": " + mod.growthRateFactorMapRecent(this, aMap));
            //}
            return allRateModifiers.Select(mod => mod.growthRateFactorMapRecent(this, aMap)).ToList();
        }

        //Growth rate for a given position at the current time
        public virtual float GrowthRateAtPos(Map aMap, IntVec3 aPosition, bool includePerMapEffects = true)
        {
            return combineGrowthRateFactors(allGrowthRateFactorsAtPos(aPosition, aMap, includePerMapEffects));
        }

        //Growth rate for the map at the current  time
        public virtual float GrowthRateAtMap(Map aMap)
        {
            return combineGrowthRateFactors(allGrowthRateFactorsAtMap(aMap));
        }

        //Growth rate for the map on average
        public virtual float GrowthRateMapMean(Map aMap)
        {
            return combineGrowthRateFactors(allGrowthRateFactorsAtMapMean(aMap));
        }

        public virtual float GrowthRateMapRecent(Map aMap)
        {
            return combineGrowthRateFactors(allGrowthRateFactorsMapRecent(aMap));
        }

        public override float tileHabitabilitySpawnFactor(int tile)
        {
            return 1f;
        }

        public override void SpawnInitialCluster(Map map, IntVec3 position, float size, int count)
        {
            base.SpawnInitialCluster(map, position, size, count);
        }

    }


    public abstract class growthRateModifier
    {
        public float aboveMaxDecayRate;  // How quickly it decays when above maxStableFert
        public float maxStable; // Will decay above this level
        public float maxGrow; // Will not grow above this level
        public float maxIdeal; // Grows fastest at this level
        public float minIdeal; // Grows fastest at this level
        public float minGrow; // Will not grow below this level
        public float minStable; // Will decay below this fertility level
        public float belowMinDecayRate;  // How quickly it decays when below minStableFert
        public bool wholeMapEffect = false; // If a whole-map attribute can be used instead of a per-position attribute (faster)

        public abstract float valueAtPos(DynamicMineral aMineral);
        public abstract float valueAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap);
        public abstract float valueAtMap(Map aMap);
        public abstract float valueAtMapSeasonal(Map aMap);
        public abstract float valueAtMapMean(Map aMap);
        public abstract float valueAtTile(World world, int worldTile);

        public virtual float growthRateFactor(float myValue)
        {
            // decays if too high or low
            float stableRangeSize = maxStable - minStable;
            if (myValue > maxStable)
            {
                return -aboveMaxDecayRate * (myValue - maxStable) / stableRangeSize;
            }
            if (myValue < minStable)
            {
                return -belowMinDecayRate * (minStable - myValue) / stableRangeSize;
            }

            // does not grow if too high or low
            if (myValue < minGrow || myValue > maxGrow)
            {
                return 0f;
            }

            // slowed growth if too high or low
            if (myValue < minIdeal)
            {
                return 1f - ((minIdeal - myValue) / (minIdeal - minGrow));
            }
            if (myValue > maxIdeal)
            {
                return 1f - ((myValue - maxIdeal) / (maxGrow - maxIdeal));
            }

            return 1f;
        }

        public virtual float growthRateFactorAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap)
        {
            return growthRateFactor(valueAtPos(myDef, aPosition, aMap));
        }

        public virtual float growthRateFactorAtPos(DynamicMineral aMineral)
        {
            return growthRateFactor(valueAtPos(aMineral));
        }

        public virtual float growthRateFactorAtMap(Map aMap)
        {
            return growthRateFactor(valueAtMap(aMap));
        }

        public virtual float growthRateFactorMapMean(Map aMap)
        {
            return growthRateFactor(valueAtMapMean(aMap));
        }
        public virtual float growthRateFactorMapSeason(Map aMap)
        {
            return growthRateFactor(valueAtMapSeasonal(aMap));
        }
        public virtual float growthRateFactorMapRecent(ThingDef_DynamicMineral myDef, Map aMap)
        {
            float mapMean = growthRateFactorMapMean(aMap);
            float mapSeason = growthRateFactorMapSeason(aMap);
            float meanWeight = (myDef.growDays * 2f) / 60f;
            if (meanWeight > 1f)
            {
                meanWeight = 1f;
            }
            if (meanWeight < 0f)
            {
                meanWeight = 0f;
            }
            return mapMean * meanWeight + mapSeason * (1 - meanWeight);
        }

    }

    public class tempGrowthRateModifier : growthRateModifier
    {
        public override float valueAtPos(DynamicMineral aMineral)
        {
            return aMineral.Position.GetTemperature(aMineral.Map);
        }
        public override float valueAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap)
        {
            return aPosition.GetTemperature(aMap);
        }
        public override float valueAtMap(Map aMap)
        {
            return aMap.mapTemperature.OutdoorTemp;
        }
        public override float valueAtTile(World world, int worldTile)
        {
            return world.tileTemperatures.GetOutdoorTemp(worldTile);
        }
        public override float valueAtMapMean(Map aMap)
        {
            return aMap.TileInfo.temperature;
        }
        public override float valueAtMapSeasonal(Map aMap)
        {
            return aMap.mapTemperature.SeasonalTemp;
        }
        public override float growthRateFactorMapMean(Map aMap)
        {
            return (growthRateFactor(valueAtMapMean(aMap) + 15f) + growthRateFactor(valueAtMapMean(aMap)) + growthRateFactor(valueAtMapMean(aMap) - 15f)) / 3f;
        }
    }

    public class rainGrowthRateModifier : growthRateModifier
    {
        private float rainfallToRain(float rainfall)
        {
            float rainProxy = rainfall / 1000f;
            if (rainProxy > 3f)
            {
                rainProxy = 3f;
            }
            return rainProxy;
        }

        public override float valueAtPos(DynamicMineral aMineral)
        {
            return aMineral.Map.weatherManager.curWeather.rainRate;
        }
        public override float valueAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap)
        {
            return aMap.weatherManager.curWeather.rainRate;
        }
        public override float valueAtMap(Map aMap)
        {
            return aMap.weatherManager.curWeather.rainRate;
        }
        public override float valueAtTile(World world, int worldTile)
        {
            return rainfallToRain(world.grid.tiles[worldTile].rainfall);
        }
        public override float valueAtMapMean(Map aMap)
        {
            return rainfallToRain(aMap.TileInfo.rainfall);
        }
        public override float valueAtMapSeasonal(Map aMap)
        {
            //Log.Message("valueAtMapSeasonal: valueAtMapMean(aMap): " + valueAtMapMean(aMap));
            //Log.Message("valueAtMapSeasonal: growthRateFactor(valueAtMapMean(aMap) * 0.5f): " + growthRateFactor(valueAtMapMean(aMap) * 0.5f));
            //Log.Message("valueAtMapSeasonal: growthRateFactor(valueAtMapMean(aMap) * 1.5f): " + growthRateFactor(valueAtMapMean(aMap) * 1.5f));
            return (growthRateFactor(valueAtMapMean(aMap) * 0.5f) + growthRateFactor(valueAtMapMean(aMap) * 1.5f)) / 2f;
        }
        public override float growthRateFactorMapMean(Map aMap)
        {
            return (growthRateFactor(valueAtMapMean(aMap) * 0.5f) + growthRateFactor(valueAtMapMean(aMap) * 1.5f) + growthRateFactor(valueAtMapMean(aMap))) / 3f;
        }

    }

    public class lightGrowthRateModifier : growthRateModifier
    {
        public float lightByBiome(BiomeDef biome)
        {
            if (biome.defName == "AB_RockyCrags")
            {
                return 0f;
            }
            else
            {
                return 1f;
            }

        }
        public override float valueAtPos(DynamicMineral aMineral)
        {
            return aMineral.Map.glowGrid.GameGlowAt(aMineral.Position);
        }
        public override float valueAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap)
        {
            return aMap.glowGrid.GameGlowAt(aPosition);
        }
        public override float valueAtMap(Map aMap)
        {
            throw new InvalidOperationException("lightGrowthRateModifier cannot be used with 'wholeMapEffect'");
        }
        public override float valueAtTile(World world, int worldTile)
        {
            return lightByBiome(world.grid.tiles[worldTile].biome);
        }
        public override float valueAtMapMean(Map aMap)
        {
            return lightByBiome(aMap.Biome);
        }
        public override float valueAtMapSeasonal(Map aMap)
        {
            return lightByBiome(aMap.Biome);
        }
    }


    public class fertGrowthRateModifier : growthRateModifier
    {
        public override float valueAtPos(DynamicMineral aMineral)
        {
            return aMineral.Map.fertilityGrid.FertilityAt(aMineral.Position);
        }
        public override float valueAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap)
        {
            return aMap.fertilityGrid.FertilityAt(aPosition);
        }
        public override float valueAtMap(Map aMap)
        {
            throw new InvalidOperationException("fertGrowthRateModifier cannot be used with 'wholeMapEffect'");
        }
        public override float valueAtTile(World world, int worldTile)
        {
            return 1f;
        }
        public override float valueAtMapMean(Map aMap)
        {
            return 1f;
        }
        public override float valueAtMapSeasonal(Map aMap)
        {
            return 1f;
        }
        public override float growthRateFactorMapMean(Map aMap)
        {
            return 1f;
        }
        public override float growthRateFactorMapSeason(Map aMap)
        {
            return 1f;
        }
    }

    public class distGrowthRateModifier : growthRateModifier
    {
        public override float valueAtPos(DynamicMineral aMineral)
        {
            return aMineral.distFromNeededTerrain;
        }

        public override float valueAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap)
        {
            return myDef.posDistFromNeededTerrain(aMap, aPosition);
        }
        public override float valueAtMap(Map aMap)
        {
            throw new InvalidOperationException("distGrowthRateModifier cannot be used with 'wholeMapEffect'");
        }
        public override float valueAtTile(World world, int worldTile)
        {
            return 1f;
        }
        public override float valueAtMapMean(Map aMap)
        {
            return 1f;
        }
        public override float valueAtMapSeasonal(Map aMap)
        {
            return 1f;
        }
        public override float growthRateFactorMapMean(Map aMap)
        {
            return 1f;
        }
        public override float growthRateFactorMapSeason(Map aMap)
        {
            return 1f;
        }
    }


    public class sizeGrowthRateModifier : growthRateModifier
    {
        public override float valueAtPos(DynamicMineral aMineral)
        {
            return aMineral.size;
        }

        public override float valueAtPos(ThingDef_DynamicMineral myDef, IntVec3 aPosition, Map aMap)
        {
            return 0.01f;
        }

        public override float valueAtMap(Map aMap)
        {
            throw new InvalidOperationException("sizeGrowthRateModifier cannot be used with 'wholeMapEffect'");
        }
        public override float valueAtTile(World world, int worldTile)
        {
            return 0.5f;
        }
        public override float valueAtMapMean(Map aMap)
        {
            return 0.5f;
        }
        public override float valueAtMapSeasonal(Map aMap)
        {
            return 1f;
        }
        public override float growthRateFactorMapMean(Map aMap)
        {
            return 1f;
        }
        public override float growthRateFactorMapSeason(Map aMap)
        {
            return 1f;
        }
    }



    public class DynamicMineralWatcher : MapComponent
    {

        public static int ticksPerLook = 1000; // 100 is about once a second on 1x speed
        public int tick_counter = 1;

        public DynamicMineralWatcher(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            // Run each class' watcher
            tick_counter += 1;
            if (tick_counter > ticksPerLook)
            {
                tick_counter = 1;
                Look();
            }
        }

        // The main function controlling what is done each time the map is looked at
        public void Look()
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            SpawnDynamicMinerals();
            //watch.Stop();
            //Log.Message("========== SpawnDynamicMinerals() took: " + watch.ElapsedMilliseconds);
        }


        public void SpawnDynamicMinerals() 
        {
            foreach (ThingDef_DynamicMineral mineralType in DefDatabase<ThingDef_DynamicMineral>.AllDefs)
            {
                //var watch = System.Diagnostics.Stopwatch.StartNew();
                //Log.Message("Trying to spawn " + mineralType.defName);


                // Check that the map type is ok
                if (! mineralType.CanSpawnInBiome(map))
                {
                    continue;
                }
                //Log.Message("   Biome OK");

                // Get number of positions to check
                float perMapGrowthFactor = mineralType.GrowthRateAtMap(map);
                //Log.Message("   perMapGrowthFactor: " + perMapGrowthFactor);
                //Log.Message("   spawnProb: " + mineralType.spawnProb);
                float numToCheck = map.Area * mineralType.spawnProb * perMapGrowthFactor * MineralsMain.Settings.mineralSpawningSetting;
                if (numToCheck <= 0)
                {
                    continue;
                }

                // If less than one cell should be checked, randomly decide to check one or none
                if (numToCheck < 1)
                {
                    if (Rand.Range(0f, 1f) < numToCheck)
                    {
                        numToCheck = 1;
                    } else
                    {
                        continue;
                    }
                }

                // Never check more than 1/10 of the map (performance failsafe)
                if (numToCheck > map.Area / 10)
                {
                    numToCheck = map.Area / 10;
                }

                // Round to integer
                numToCheck = (float) Math.Round(numToCheck);

                //Log.Message("   numToCheck: " + numToCheck);

                // Try to spawn in a subset of positions
                for (int i = 0; i < numToCheck; i++)
                {
                    // Pick a random location
                    IntVec3 aPos = CellIndicesUtility.IndexToCell(Rand.RangeInclusive(0, map.Area - 1), map.Size.x);

                    // Dont always spawn if growth rate is not good
                    if (Rand.Range(0f, 1f) > mineralType.GrowthRateAtPos(map, aPos, false))
                    {
                        continue;
                    }

//                    // If it is an associated ore, find a position nearby
//                    if (mineralType.PosIsAssociatedOre(map, aPos))
//                    {
//                        IntVec3 dest;
//                        if (mineralType.TryFindReproductionDestination(map, aPos, out dest))
//                        {
//                            aPos = dest;
//                        }
//                    }


                    // Try to spawn at that location
                    //Log.Message("Trying to spawn " + mineralType.defName);
                    //mineralType.TrySpawnAt(aPos, map, 0.01f);
                    mineralType.TrySpawnCluster(map, aPos, Rand.Range(0.01f, 0.05f), Rand.Range(mineralType.minSpawnClusterSize, mineralType.maxSpawnClusterSize));

                }

                //watch.Stop();
                //Log.Message("Spawning " + mineralType.defName + " took: " + watch.ElapsedMilliseconds);

            }
        }
    }
}



