using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SK_HC
{
    public class MatterResourceDef : Def
    {
        public int spawnRangeMin;
        public int spawnRangeMax;
        public int ticksToProduce;
        public ThingDef resourceDefName;
    }
}
