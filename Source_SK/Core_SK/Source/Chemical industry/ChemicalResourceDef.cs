using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SK_OR
{
    public class ChemicalResourceDef : Def
    {
        public int spawnRangeMin;
        public int spawnRangeMax;
        public int ticksToProduce;
        public ThingDef resourceDefName;
    }
}
