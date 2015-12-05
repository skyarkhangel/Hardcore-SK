using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;         // Always needed
using Verse;               // RimWorld universal objects are here (like 'Building')
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')

namespace Core_SK
{
    class Building_RefrigeratedStorage : Building_Storage, ISlotGroupParent
    {
        public CompPowerTrader powerComp;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.powerComp = this.GetComp<CompPowerTrader>();
        }
        
        public override void TickRare()
        {
            if (this.powerComp.PowerOn)
            {
                CompRottable aCompRot;
                foreach (ThingWithComps aThing in this.slotGroup.HeldThings)
                {
                    foreach (ThingComp aComp in aThing.AllComps)
                    {
                        if (aComp.GetType() == typeof(CompRottable))
                        {
                            aCompRot = (CompRottable)aComp;
                            aCompRot.rotProgress = 0;
                            aThing.HitPoints = aThing.MaxHitPoints;
                        }
                    }
                }
            }
        }
    }
}