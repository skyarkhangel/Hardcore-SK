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
	/// SolidRock class
	/// </summary>
	/// <author>zachary-foster</author>
	/// <permission>No restrictions</permission>
	public class SolidRock : StaticMineral
	{
        public new ThingDef_SolidRock attributes
        {
            get
            {
                return base.attributes as ThingDef_SolidRock;
            }
        }


	}       


	/// <summary>
	/// ThingDef_StaticMineral class.
	/// </summary>
	/// <author>zachary-foster</author>
	/// <permission>No restrictions</permission>
	public class ThingDef_SolidRock : ThingDef_StaticMineral
	{

        public override bool isRoofConditionOk(Map map, IntVec3 position)
        {
            return base.isRoofConditionOk(map, position) || (map.roofGrid.Roofed(position) && IsNearPassable(map, position));
        }


        public bool IsNearPassable(Map map, IntVec3 position, int radius = 1)
        {
            for (int xOffset = -radius; xOffset <= radius; xOffset++)
            {
                for (int zOffset = -radius; zOffset <= radius; zOffset++)
                {
                    IntVec3 checkedPosition = position + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(map))
                    {
                        if (! checkedPosition.Impassable(map))
                        {
                            return true;
                        }

                    }
                }
            }

            return false;

        }

    }

}
