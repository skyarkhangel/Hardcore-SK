using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace SK_Heater
{
    public class CompFeed : ThingComp
    {
        public Thing FedWithThings
        {
            get
            {
                ThingDef thingdef = ThingDef.Named("Kindling");
                ThingDef thingdef1 = ThingDef.Named("Coal");
                ThingDef thingdef2 = ThingDef.Named("Peat");
                ThingDef thingdef3 = ThingDef.Named("HeaterHopper");
                foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(parent))
                {
                    Thing thing = null;
                    Thing thing1 = null;
                    Thing thing2 = null;
                    Thing thing3 = null;
                    foreach (Thing current2 in Find.ThingGrid.ThingsAt(current))
                    {
                        if (current2.def == thingdef)
                        {
                            thing = current2;
                        }
                        if (current2.def == thingdef1)
                        {
                            thing1 = current2;
                        }
                        if (current2.def == thingdef2)
                        {
                            thing2 = current2;
                        }
                        if (current2.def == thingdef3)
                        {
                            thing3 = current2;
                        }
                    }
                    if (thing3 != null && thing != null)
                    {
                        return thing;
                    }
                    if (thing3 != null && thing1 != null)
                    {
                        return thing1;
                    }
                    if (thing3 != null && thing2 != null)
                    {
                        return thing2;
                    }
                }
                return null;
            }
        }

        public bool HasFuelInHopper
        {
            get
            {
                return FedWithThings != null;
            }
        }

        public CompFeed()
        {
        }
    }
}
