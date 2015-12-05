using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SK_GH
{
    public static class Util_GH
    {
        public static ThingDef GHDef
        {
            get
            {
                return (ThingDef.Named("GH"));
            }
        }
    }
    public class Building_GeothermalHeater : Building
    {
        private int Burnticks = 50;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            Thing thing = Find.ThingGrid.ThingAt(this.Position, ThingDefOf.SteamGeyser);
            if (thing != null)
            {
                Find.ThingGrid.ThingAt(this.Position, ThingDef.Named("SteamGeyser")).DeSpawn();
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (--Burnticks == 0)
            {
                MoteThrower.ThrowSmoke(base.Position.ToVector3Shifted(), 1f);
                Burnticks = 50;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            GenSpawn.Spawn(ThingDef.Named("SteamGeyser"), this.Position);
        }
    }
}