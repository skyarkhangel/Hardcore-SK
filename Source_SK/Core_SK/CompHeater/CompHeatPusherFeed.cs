using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace SK_Heater
{
    public class CompHeatPusherFeed : ThingComp
    {
        public CompHeatPusherFeed()
        {
        }
        
        // Methods
        public override void CompTick()
        {
            base.CompTick();
            if (base.parent.IsHashIntervalTick(120))
            {
                Building_Fed parent = base.parent as Building_Fed;
                CompFeed feedComp = parent.FeedComp as CompFeed;

                if (((feedComp != null) && feedComp.HasFuelInHopper) && (base.parent.Position.GetTemperature() < base.props.heatPushMaxTemperature))
                {                    
                    // Log.Message("pushing temperatures");
                    GenTemperature.PushHeat(base.parent.Position, base.props.heatPerSecond);
                }
            }
        }
    }
}
