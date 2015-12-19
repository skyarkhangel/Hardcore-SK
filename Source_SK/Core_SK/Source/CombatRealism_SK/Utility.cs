using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace SK
{
    class Utility
    {
        /// <summary>
        /// Determines the impact location accounting for static obstructions (walls, cover) with chance of interception proportional to distance travelled.
        /// Collision occurs only after specified range, use 1 for collision detection at all ranges, 0 to disable distance effect on intercept chance.
        /// </summary>
        public static TargetInfo determineImpactPosition(IntVec3 originalPosition, TargetInfo originalTarget, int minCollisionRange)
        {
            //Flight path is divided into 1 cell segments
            Vector3 trajectory = originalTarget.Cell.ToVector3() - originalPosition.ToVector3();
            int numSegments = (int)trajectory.magnitude;
            Vector3 trajectorySegment = trajectory / trajectory.magnitude;

            Vector3 exactTestedPosition = originalPosition.ToVector3();
            IntVec3 testedPosition = originalPosition;

            //Go through flight path one segment at a time
            for (int segmentIndex = 1; segmentIndex < numSegments; segmentIndex++)
            {
                exactTestedPosition += trajectorySegment;
                testedPosition = exactTestedPosition.ToIntVec3();

                if (!exactTestedPosition.InBounds())
                {
                    break;
                }

                //Check for collision starting at minimum collision range
                if (segmentIndex >= minCollisionRange)
                {
                    List<Thing> list = Find.ThingGrid.ThingsListAt(testedPosition);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Thing currentThing = list[i];

                        //Check for walls
                        if (currentThing.def.Fillage == FillCategory.Full)
                        {
                            return new TargetInfo(currentThing);
                        }

                        float collateralChance = 0f;

                        //Check for pawns
						if (currentThing.def.category == ThingCategory.Pawn)
                        {
                            Pawn pawn = currentThing as Pawn;
                            collateralChance = 0.45f;

                            if (pawn.GetPosture() != PawnPosture.Standing)
                            {
                                collateralChance *= 0.1f;
                            }
                            collateralChance *= pawn.RaceProps.baseBodySize;
                        }
                        else
                        {
                            collateralChance = currentThing.def.fillPercent;
                        }
                        if (minCollisionRange != 0)
                        {
                            collateralChance *= segmentIndex / trajectory.magnitude;
                        }

                        if (Rand.Value < collateralChance)
                        {
                            return new TargetInfo(currentThing);
                        }
                    }
                }
            }
            return originalTarget;
        }

        public static TargetInfo determineImpactPosition(IntVec3 originalPosition, TargetInfo originalTarget)
        {
            return determineImpactPosition(originalPosition, originalTarget, 0);
        }

    }
}
