using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace SK_FuelPowerplant
{
    public class Building_FuelPowerplant : Building
    {
        private int burnDelay;
        private int pufferSmoke;
        public CompPowerTrader powerComp;
        public CompGlower glowerComp;

        public bool IsFlareActive
        {
            get
            {
                return !Find.MapConditionManager.ConditionIsActive(MapConditionDefOf.SolarFlare);
            }
        }

        public bool CanBurnNow
        {
            get
            {
                return this.powerComp.PowerOn && this.HasFuelInHopper && IsFlareActive;
            }
        }

        public bool HasFuelInHopper
        {
            get
            {
                return this.FuelInHopper != null;
            }
        }

        public Building AnyAdjacentHopper
        {
            get
            {
                ThingDef thingDef = ThingDef.Named("FuelFeeder");
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
        private Thing FuelInHopper
        {
            get
            {
                ThingDef thingdef = ThingDef.Named("Fuel");
                ThingDef thingdef2 = ThingDef.Named("FuelFeeder");
                foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(this))
                {
                    Thing thing = null;
                    Thing thing2 = null;
                    foreach (Thing current2 in Find.ThingGrid.ThingsAt(current))
                    {
                        if (current2.def == thingdef)
                        {
                            thing = current2;
                        }
                        if (current2.def == thingdef2)
                        {
                            thing2 = current2;
                        }
                    }
                    if (thing2 != null && thing != null)
                    {
                        return thing;
                    }
                }
                return null;
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            powerComp = base.GetComp<CompPowerTrader>();
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
                this.powerComp.PowerOutput = 0f;
                burnDelay = 0;
            }
            else
                if (pufferSmoke <= 0)
                {
                    MoteThrower.ThrowSmoke(this.TrueCenter(), 2f);
                    pufferSmoke = 120;
                    if (this.burnDelay <= 0)
                    {
                        glowerComp.Lit = true;
                        int num = 0;
                        List<ThingDef> list = new List<ThingDef>();
                        Thing FuelInHopper = this.FuelInHopper;
                        int num2 = Mathf.Min(FuelInHopper.stackCount, 1);
                        num += num2;
                        list.Add(FuelInHopper.def);
                        FuelInHopper.SplitOff(num2);
                        FuelInHopper = this.FuelInHopper;
                        this.powerComp.PowerOutput = 2800f;
                        this.burnDelay = 800;
                    }
                }
        }
    }
}