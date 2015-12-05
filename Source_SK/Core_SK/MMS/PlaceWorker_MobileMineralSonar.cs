using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace MobileMineralSonar
{
    /// <summary>
    /// Mobile mineral sonar custom place worker class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class PlaceWorker_MobileMineralSonar : PlaceWorker
    {
        public const int minDistanceBetweenTwoDeepdrillerMachines = 10;

        /// <summary>
        /// Display the scan range of built mobile mineral sonar and the max scan range at the tested position.
        /// Allow placement nearly anywhere.
        /// </summary>
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            IEnumerable<Building> mobileMineralSonarList = Find.ListerBuildings.AllBuildingsColonistOfDef(ThingDef.Named("MobileMineralSonar"));

            if (mobileMineralSonarList != null)
            {
                foreach (Building mobileMineralSonar in mobileMineralSonarList)
                {
                    (mobileMineralSonar as Building_MobileMineralSonar).DrawMaxScanRange();
                }
            }

            if (Find.ResearchManager.IsFinished(ResearchProjectDef.Named("ResearchMineralSonarEnhancedScan")) == false)
            {
                Material scanRange30 = MaterialPool.MatFrom("Effects/ScanRange30");
                Vector3 scanRangeScale30 = new Vector3(60f, 1f, 60f);
                Matrix4x4 scanRangeMatrix30 = default(Matrix4x4);
                // The 10f offset on Y axis is mandatory to be over the fog of war.
                scanRangeMatrix30.SetTRS(loc.ToVector3Shifted() + new Vector3(0f, 10f, 0f) + Altitudes.AltIncVect, (0f).ToQuat(), scanRangeScale30);
                Graphics.DrawMesh(MeshPool.plane10, scanRangeMatrix30, scanRange30, 0);
            }
            else
            {
                Material scanRange50 = MaterialPool.MatFrom("Effects/ScanRange50");
                Vector3 scanRangeScale50 = new Vector3(100f, 1f, 100f);
                Matrix4x4 scanRangeMatrix50 = default(Matrix4x4);
                // The 10f offset on Y axis is mandatory to be over the fog of war.
                scanRangeMatrix50.SetTRS(loc.ToVector3Shifted() + new Vector3(0f, 10f, 0f) + Altitudes.AltIncVect, (0f).ToQuat(), scanRangeScale50);
                Graphics.DrawMesh(MeshPool.plane10, scanRangeMatrix50, scanRange50, 0);
            }

            return true;
        }
    }
}
