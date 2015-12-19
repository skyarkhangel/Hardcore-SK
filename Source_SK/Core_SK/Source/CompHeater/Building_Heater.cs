using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace SK_Heater
{
    public class Building_Heater : Building_Fed
    {
        private int burnDelay;
        private int pufferSmoke;
        public CompGlower glowerComp;

        public bool CanBurnNow
        {
            get
            {                
                return this.FeedComp.HasFuelInHopper;
            }
        }        

        public Building AnyAdjacentHopper
        {
            get
            {
                ThingDef thingDef = ThingDef.Named("HeaterHopper");
                foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(this))
                {
                    Building building = current.GetEdifice();
                    if (building != null && building.def == thingDef)
                    {
                        return (Building_Storage)building;
                    }
                }
                return null;
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            glowerComp = base.GetComp<CompGlower>();
            glowerComp.Lit = false;
        }

        public override void Tick()
        {
            base.Tick();
            this.burnDelay--;
            this.pufferSmoke--;
            if (!this.CanBurnNow)
            {
                glowerComp.Lit = false;
                burnDelay = 0;
            }
            else
                if (pufferSmoke <= 0)
                {
                    MoteThrower.ThrowSmoke(this.TrueCenter(), 2f);
                    pufferSmoke = 60;
                    if (this.burnDelay <= 0)
                    {
                        glowerComp.Lit = true;
                        int num = 0;
                        List<ThingDef> list = new List<ThingDef>();
                        Thing FuelInHopper = this.FeedComp.FedWithThings;
                        int num2 = Mathf.Min(FuelInHopper.stackCount, 1);
                        num += num2;
                        list.Add(FuelInHopper.def);
                        FuelInHopper.SplitOff(num2);
                        FuelInHopper = this.FeedComp.FedWithThings;
                        this.burnDelay = 2000;
                    }
                }
        }
    }
}