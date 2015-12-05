using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
namespace TurretCollection
{
    public class Projectile_SMBullet : Projectile
    {
        protected override void Impact(Thing hitThing)
        {
            if (hitThing != null)
            {
                this.Explode();
            }
            else
            {
                this.Explode();
            }
        }
        protected virtual void Explode()
        {
            this.Destroy(DestroyMode.Vanish);
            BodyPartDamageInfo value = new BodyPartDamageInfo(null, new BodyPartDepth?(BodyPartDepth.Outside));
            ExplosionInfo explosionInfo = default(ExplosionInfo);
            explosionInfo.center = base.Position;
            explosionInfo.radius = this.def.projectile.explosionRadius;
            explosionInfo.dinfo = new DamageInfo(this.def.projectile.damageDef, 999, this.launcher, new BodyPartDamageInfo?(value), null);
            explosionInfo.postExplosionSpawnThingDef = this.def.projectile.postExplosionSpawnThingDef;
            explosionInfo.explosionSpawnChance = this.def.projectile.explosionSpawnChance;
            explosionInfo.explosionSound = this.def.projectile.soundExplode;
            explosionInfo.projectile = this.def;
            explosionInfo.DoExplosion();
            ThrowBigExplode(explosionInfo.center.ToVector3Shifted() + Gen.RandomHorizontalVector(explosionInfo.radius * 0.7f), explosionInfo.radius * 0.6f);
        }
        public static void ThrowBigExplode(Vector3 loc, float size)
        {
            if (!loc.ShouldSpawnMotesAt())
            {
                return;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_BigExplode"), null);
            moteThrown.ScaleUniform = Rand.Range(5f, 6f) * size;
            moteThrown.exactRotationRate = Rand.Range(0f, 0f);
            moteThrown.exactPosition = loc;
            moteThrown.SetVelocityAngleSpeed((float)Rand.Range(6, 8), Rand.Range(0.002f, 0.003f));
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
        }
    }
}
