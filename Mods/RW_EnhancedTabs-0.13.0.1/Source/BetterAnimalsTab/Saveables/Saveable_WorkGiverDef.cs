using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Fluffy
{
    class Saveable_WorkGiverDef : IExposable
    {
        public string defName;
        public int priority;

        public Saveable_WorkGiverDef()
        {
            // empty constructor for scribe.
        }

        public Saveable_WorkGiverDef(WorkGiverDef def)
        {
            defName = def.defName;
            priority = def.priorityInType;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref defName, "defName");
            Scribe_Values.LookValue(ref priority, "priority");
        }
    }
}
