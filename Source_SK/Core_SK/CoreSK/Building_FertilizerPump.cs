using System;
using Verse;
using RimWorld;
using UnityEngine;

namespace Core_SK
{
    public class Building_FertilizerPump : Building
    {
        private int ticksSinceExpand;
        private int timesExpanded;
        private CompPowerTrader powerComp;
        private TerrainDef soilDef;
        private int TicksPerCell = 5000;
        private int MaxCellsToAffect = 25;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.soilDef = DefDatabase<TerrainDef>.GetNamed("Soil");
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.ticksSinceExpand, "ticksSinceExpand", 0, false);
            Scribe_Values.LookValue<int>(ref this.timesExpanded, "timesExpanded", 0, false);
        }
        public override void TickRare()
        {
            if (this.powerComp.PowerOn)
            {
                this.ticksSinceExpand += 250;
                if (this.ticksSinceExpand >= this.TicksPerCell)
                {
                    this.ExpandFertilizerField();
                    this.ticksSinceExpand = 0;
                }
            }
        }
        private void ExpandFertilizerField()
        {
            this.timesExpanded++;
            if (this.timesExpanded > this.MaxCellsToAffect)
            {
                return;
            }
            for (int i = 0; i < this.timesExpanded; i++)
            {
                IntVec3 intVec = base.Position + GenRadial.RadialPattern[i];
                if (intVec.InBounds())
                {
                    TerrainDef terrain = intVec.GetTerrain();
                    if (terrain.fertility < this.soilDef.fertility && terrain.changeable)
                    {
                        Find.TerrainGrid.SetTerrain(intVec, this.soilDef);
                    }
                }
            }
        }
    }
}
