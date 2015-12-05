using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using RimWorld;
using Verse;

namespace SK_Enviro.Seeds
{
    class ThingDef_PlantSeedItem : ThingDef
    {
        public ThingDef PlantProducedDef = null;
        /*
        public override void ResolveReferences()
        {
            base.ResolveReferences();

            if ( PlantProducedDef != null )
            {
                // alter the seed's description to contain that of the plant as well to prevent a big copy/paste maintenance nightmare
                // note that this needs to be done here rather than within the thing class itself, otherwise the change in the description
                // won't impact the trade gui (I assume the items in there aren't actually instantiated).

                description = PlantProducedDef.description + "\n\n" + description;
            }
        }
        */
    }
}
