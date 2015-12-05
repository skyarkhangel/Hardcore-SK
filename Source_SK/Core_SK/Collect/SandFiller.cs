using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SK_collect
{
    public class SandFiller : Building
    {
        public static readonly TerrainDef SandDef = DefDatabase<TerrainDef>.GetNamed("Sand");

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            Find.TerrainGrid.SetTerrain(this.Position, SandDef);
            this.Destroy(DestroyMode.Vanish);
        }

    }
}