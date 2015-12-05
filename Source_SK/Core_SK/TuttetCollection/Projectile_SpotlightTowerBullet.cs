using TurretCollection;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
namespace TurretCollection
{
    public class Projectile_SpotlightTowerBullet : Projectile
    {

        protected override void Impact(Thing hitThing)
            {
            base.Impact(hitThing);
            if (hitThing != null)
            {
                GenSpawn.Spawn(ThingDef.Named("SpotlightDeployed"), this.Position);
            }
            else
            {
                GenSpawn.Spawn(ThingDef.Named("SpotlightDeployed"), this.Position);
            }
        }

    }
}
