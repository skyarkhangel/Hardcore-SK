using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 
using Verse.AI.Group;

namespace MineralsFrozen
{
    /// <summary>
    /// SaltCrystal class
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class SnowDrift : Minerals.DynamicMineral
    {

        public override float GrowthRate
        {
            get
            {
                return attributes.GrowthRateAtPos(Map, Position);
            }
        }

        public override void TickLong()
        {
            // Always make the ground blow snow until it melts
            float minSnowDepth = size;
            if (minSnowDepth > 1)
            {
                minSnowDepth = 1f;
            }
            if (Map.snowGrid.GetDepth(Position) < minSnowDepth)
            {
                Map.snowGrid.SetDepth(Position, minSnowDepth);
            }
           
            base.TickLong();
        }


    }


    /// <summary>
    /// ThingDef_SnowDrift class.
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class ThingDef_Snow : Minerals.ThingDef_DynamicMineral
    {

        // How much faster it melts in still water
        public float stillWaterMeltFactor = 10f;
        // How much faster it melts in moving water
        public float movingWaterMeltFactor = 100f;
        // How much faster it melts in rain water
        public float rainMeltFactor = 5f;

        public virtual float growthRateFactor(IntVec3 aPosition, Map aMap, float rate)
        {
            float factor = 1f;

           
            if (rate < 0)
            {
                // Melts faster in water
                if (aMap.terrainGrid.TerrainAt(aPosition).defName.Contains("Water"))
                {
                    if (aMap.terrainGrid.TerrainAt(aPosition).defName.Contains("Moving"))
                    {

                        factor = factor * movingWaterMeltFactor; // melt even faster in moving water
                    }
                    else
                    {
                        factor = factor * stillWaterMeltFactor;
                    }
                }

                // Melts faster in rain
                if (! aMap.roofGrid.Roofed(aPosition))
                {
                    factor = factor * (1 + aMap.weatherManager.curWeather.rainRate * rainMeltFactor);
                }
            }

            return factor;
        }

        public override float GrowthRateAtPos(Map aMap, IntVec3 aPosition, bool includePerMapEffects = true)
        {
            float rate = base.GrowthRateAtPos(aMap, aPosition);
            return rate * growthRateFactor(aPosition, aMap, rate);
        }

    }

    public class ThingDef_DeepSnow : ThingDef_Snow
    {
        // How much each nearby obstruction reduces growth rate. The effect is multiplicative
        public float obstructionGrowthFactor = 0.5f;
        // The maximum radius obstructions are looked for
        public int obstructionSearchRadius = 2;


        public override bool PlaceIsBlocked(Map map, IntVec3 position, bool initialSpawn)
        {
            if (!position.InBounds(map))
            {
                return true;
            }

            if (base.PlaceIsBlocked(map, position, initialSpawn))
            {
                return true;
            }

            // dont spawn on big things
            if (! position.Standable(map))
            {
                return true;
            }

            // Cant spawn on water
            if (position.GetTerrain(map).defName.Contains("Water"))
            {
                return true;
            }

            // dont spawn  next to stuff
            if (Rand.Range(0f, 1f) > obstructionGrowthRateFactor(position, map))
            {
                return true;
            }

            // Cant spawn where there is too little snow
            if ((! initialSpawn) && map.snowGrid.GetDepth(position) < 0.9f)
            {
                return true;
            }

            return false;
        }

        public virtual float obstructionGrowthRateFactor(IntVec3 aPosition, Map aMap)
        {
            float factor = 1f;

            // Roofs block snow
            if (aPosition.Roofed(aMap))
            {
                return 0f;
            }

            // Nearby Buldings slow growth
            for (int xOffset = -obstructionSearchRadius; xOffset <= obstructionSearchRadius; xOffset++)
            {
                for (int zOffset = -obstructionSearchRadius; zOffset <= obstructionSearchRadius; zOffset++)
                {
                    IntVec3 checkedPosition = aPosition + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(aMap) && (! (xOffset == 0 && zOffset == 0)))
                    {
                        foreach (Thing thing in aMap.thingGrid.ThingsListAt(checkedPosition))
                        {
                            if ((thing is Building && thing.def.altitudeLayer == AltitudeLayer.Building) || checkedPosition.Roofed(aMap) || (thing is Plant && ! checkedPosition.Standable(aMap)))
                            {
                                // float dist = checkedPosition.DistanceTo(aPosition) / obstructionSearchRadius;
                                float dist = Math.Abs(xOffset) + Math.Abs(zOffset);
                                factor = factor * dist / (dist + obstructionGrowthFactor);
                                continue;
                            }

                        }
                    }
                }
            }

            return factor;

        }

        public override float growthRateFactor(IntVec3 aPosition, Map aMap, float rate)
        {
            float factor = base.growthRateFactor(aPosition, aMap, rate);

            // Nearby Buldings slow growth  and melting
            float obstructionFactor = obstructionGrowthRateFactor(aPosition, aMap);
            if (factor >= 0)
            {
                factor = factor * obstructionFactor;
            } else
            {
                if (obstructionFactor < 0.1f)
                {
                    factor = factor * 0.1f;
                } else
                {
                    factor = factor * obstructionFactor;
                }
            }
            

            return factor;
        }

        public override void SpawnInitialCluster(Map map, IntVec3 position, float size, int count)
        {
            base.SpawnInitialCluster(map, position, size * obstructionGrowthRateFactor(position, map), count);
        }


    }

    public class ThingDef_SnowDrift : ThingDef_Snow
    {
        // How much each nearby obstruction increases growth rate. The effect is additive
        public float obstructionGrowthBonus = 0.25f;
        // The maximum radius obstructions are looked for
        public int obstructionSearchRadius = 1;

        public override bool PlaceIsBlocked(Map map, IntVec3 position, bool initialSpawn)
        {
//            Log.Message("PlaceIsBlocked: drift");
            if (! position.InBounds(map))
            {
                return true;
            }
//            Log.Message("PlaceIsBlocked: drift in bounds");

            // Cant spawn on water
            if (position.GetTerrain(map).defName.Contains("Water"))
            {
                return true;
            }
//            Log.Message("PlaceIsBlocked: drift not water");

            // dont spawn in the open
            if (Rand.Range(0f, 1f) > obstructionGrowthRateFactor(position, map))
            {
                return true;
            }

            // Cant spawn where there is too little snow
            if ((! initialSpawn) && map.snowGrid.GetDepth(position) < 0.9f)
            {
                return true;
            }


            //            Log.Message("PlaceIsBlocked: drift not blocked");

            return base.PlaceIsBlocked(map, position, initialSpawn);
        }

        public virtual float obstructionGrowthRateFactor(IntVec3 aPosition, Map aMap)
        {
            float factor = 0f;

            // Roofs block snow
            if (aPosition.Roofed(aMap))
            {
                return 0f;
            }

            // Nearby Buldings or other snow drifts allow growth
            for (int xOffset = -obstructionSearchRadius; xOffset <= obstructionSearchRadius; xOffset++)
            {
                for (int zOffset = -obstructionSearchRadius; zOffset <= obstructionSearchRadius; zOffset++)
                {
                    IntVec3 checkedPosition = aPosition + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(aMap))
                    {
                        foreach (Thing thing in aMap.thingGrid.ThingsListAt(checkedPosition))
                        {
                            if ((thing is Building && thing.def.altitudeLayer == AltitudeLayer.Building) || thing.def is ThingDef_SnowDrift)
                            {
                                factor = factor + obstructionGrowthBonus;
                            }

                        }
                    }
                }
            }

            return factor;

        }

        public override float growthRateFactor(IntVec3 aPosition, Map aMap, float rate)
        {
            float factor = base.growthRateFactor(aPosition, aMap, rate);

            // Nearby Buldings speed growth
            if (rate > 0f)
            {
                factor = factor * obstructionGrowthRateFactor(aPosition, aMap);
            }

            return factor;
        }

        public override void SpawnInitialCluster(Map map, IntVec3 position, float size, int count)
        {
            base.SpawnInitialCluster(map, position, size * obstructionGrowthRateFactor(position, map), count);
        }

    }
}


