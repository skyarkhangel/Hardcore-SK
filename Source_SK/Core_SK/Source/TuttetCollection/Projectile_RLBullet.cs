using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
namespace TurretCollection
{
    public class Projectile_RLBullet : Projectile
    {
        private int ticksToDetonation;
        private int Burnticks = 3;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.ticksToDetonation, "ticksToDetonation", 0, false);
        }
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            MoteThrower.ThrowSmoke(base.Position.ToVector3Shifted(), 5f);
        }
        public override void Tick()
        {
            base.Tick();
            if (--Burnticks == 0)
            {
                MoteThrower.ThrowSmoke(base.Position.ToVector3Shifted(), 1f);
                Burnticks = 3;
            }
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
            if (hitThing != null)
            {
                if (hitThing.def.category == ThingCategory.Pawn)
                {
                    MoteThrower.ThrowText(new Vector3((float)this.Position.x + 1f, (float)this.Position.y, (float)this.Position.z + 1f), Translator.Translate("Hit"), Color.red);
                }
                DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, this.def.projectile.damageAmountBase, this.launcher, this.ExactRotation.eulerAngles.y, new BodyPartDamageInfo?(new BodyPartDamageInfo(new BodyPartHeight?(), new BodyPartDepth?())), this.equipmentDef);
                hitThing.TakeDamage(dinfo);
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
            explosionInfo.dinfo = new DamageInfo(DamageDefOf.Bomb, 50, this.launcher, new BodyPartDamageInfo?(value), null);
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
