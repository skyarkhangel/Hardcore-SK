using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI.Group;
using System.Diagnostics;

namespace LetsGoExplore
{
    public class ShipCoreStartupSite : Site
    {
        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            //Check what base method has to say about map removal
            bool returnValue = base.ShouldRemoveMapNow(out alsoRemoveWorldObject);

            IEnumerable<Thing> cryoPodList = (IEnumerable<Thing>)this.Map.listerThings.ThingsOfDef(ThingDefOf.Ship_CryptosleepCasket);
            foreach(Thing t in cryoPodList)
            {
                Building_Casket casket = t as Building_Casket;
                if (casket.HasAnyContents)
                {
                    returnValue = false;
                    break;
                }
            }

            return returnValue;
        }
    }
}
