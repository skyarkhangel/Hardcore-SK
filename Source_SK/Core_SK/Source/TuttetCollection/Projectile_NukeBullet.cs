using TurretCollection;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
namespace TurretCollection
{
    public class Projectile_NukeBullet : Projectile
    {

        protected override void Impact(Thing hitThing)
            {
            base.Impact(hitThing);
            if (hitThing != null)
            {
                GenSpawn.Spawn(ThingDef.Named("WarheadDeployed"), this.Position);
            }
            else
            {
                GenSpawn.Spawn(ThingDef.Named("WarheadDeployed"), this.Position);
            }
        }

    }
}
