using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 

namespace Minerals
{
    /// <summary>
    /// RiverRock class
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class RiverRock : DynamicMineral
    {

        public override float GrowthRate
        {
            get
            {
                float rate = 1f - submersibleFactor();
                return rate * rate;
            }
        }


    }       


    /// <summary>
    /// ThingDef_StaticMineral class.
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class ThingDef_RiverRock : ThingDef_DynamicMineral
    {
    
        public override float GrowthRateAtPos(Map aMap, IntVec3 aPosition, bool includePerMapEffects = true) 
        {
            TerrainDef myTerrain = aMap.terrainGrid.TerrainAt(aPosition);
            if (myTerrain.defName.Contains("Water") || myTerrain.defName.Contains("water"))
            {
                return 1f;
            }
            else
            {
                return 0f;
            }

        }

    }

}
