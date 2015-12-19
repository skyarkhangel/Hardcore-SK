using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace SK_Heater
{
    public abstract class Building_Fed : Building
    {
        public CompFeed FeedComp
        {
            get
            {
                return base.GetComp<CompFeed>();
            }
        }
    }
}
