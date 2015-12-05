using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using RimWorld;
using Verse;

namespace SK_Enviro.Seeds
{
    class ThingDef_PlantWithSeeds : ThingDef
    {
        public ThingDef SeedDef = null;
        public PlantGenomeProperties genome = null;
    }
}
