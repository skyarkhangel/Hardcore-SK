using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace SK_LaserWeapons
{
    public class Projectile_FusionGrenade : Projectile
    {
        private int ticksToDetonation;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.ticksToDetonation, "ticksToDetonation", 0, false);
        }
        public override void Tick()
        {
            base.Tick();
            if (this.ticksToDetonation > 0)
            {
                this.ticksToDetonation--;
                if (this.ticksToDetonation <= 0)
                {
                    this.Explode();
                }
            }
        }
        protected override void Impact(Thing hitThing)
        {
            if (this.def.projectile.explosionDelay == 0)
            {
                this.Explode();
                return;
            }
            this.landed = true;
            this.ticksToDetonation = this.def.projectile.explosionDelay;
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
            for (int i = 0; i < 4; i++)
			{
                ThrowSmokeRed(explosionInfo.center.ToVector3Shifted() + Gen.RandomHorizontalVector(explosionInfo.radius * 0.7f), explosionInfo.radius * 0.6f);
                ThrowMicroSparksRed(explosionInfo.center.ToVector3Shifted() + Gen.RandomHorizontalVector(explosionInfo.radius * 0.7f));
			}
        }
        public static void ThrowSmokeRed(Vector3 loc, float size)
        {
            if (!loc.ShouldSpawnMotesAt() || MoteCounter.SaturatedLowPriority)
            {
                return;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_SmokeRed"), null);
            moteThrown.ScaleUniform = Rand.Range(1.5f, 2.5f) * size;
            moteThrown.exactRotationRate = Rand.Range(-0.5f, 0.5f);
            moteThrown.exactPosition = loc;
            moteThrown.SetVelocityAngleSpeed((float)Rand.Range(30, 40), Rand.Range(0.008f, 0.012f));
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
        }
        public static void ThrowMicroSparksRed(Vector3 loc)
        {
            if (!loc.ShouldSpawnMotesAt() || MoteCounter.Saturated)
            {
                return;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_MicroSparksRed"), null);
            moteThrown.ScaleUniform = Rand.Range(0.8f, 1.2f);
            moteThrown.exactRotationRate = Rand.Range(-0.2f, 0.2f);
            moteThrown.exactPosition = loc;
            moteThrown.exactPosition -= new Vector3(0.5f, 0f, 0.5f);
            moteThrown.exactPosition += new Vector3(Rand.Value, 0f, Rand.Value);
            moteThrown.SetVelocityAngleSpeed((float)Rand.Range(35, 45), Rand.Range(0.02f, 0.02f));
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
        }
    }
}
