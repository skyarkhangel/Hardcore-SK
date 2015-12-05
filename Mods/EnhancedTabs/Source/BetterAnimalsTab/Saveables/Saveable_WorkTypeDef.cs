using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Fluffy
{
    class Saveable_WorkTypeDef : IExposable
    {
        public string defName;
        public int priority;
        public List<Saveable_WorkGiverDef> workGivers;

        // empty constructor for scribe
        public Saveable_WorkTypeDef()
        {
            // tralalalalala, in the morning!   
        }

        public Saveable_WorkTypeDef(WorkTypeDef def)
        {
            defName = def.defName;
            priority = def.naturalPriority;
            workGivers = def.workGiversByPriority.Select(wg => new Saveable_WorkGiverDef(wg)).ToList();
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref defName, "defName");
            Scribe_Values.LookValue(ref priority, "priority");
            Scribe_Collections.LookList(ref workGivers, "workGivers", LookMode.Deep);
        }
    }
}
