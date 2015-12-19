using TurretCollection;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
namespace TurretCollection
{
    public class Projectile_30mmGunBullet : Bullet
    {

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            MoteThrower.ThrowSmoke(base.Position.ToVector3Shifted(), 3f);
        }

        protected override void Impact(Thing hitThing)
        {
            if (hitThing != null)
            {
                if (hitThing.def.category == ThingCategory.Pawn)
                {
                    MoteThrower.ThrowText(new Vector3((float)this.Position.x + 1f, (float)this.Position.y, (float)this.Position.z + 1f), Translator.Translate("Hit"), Color.yellow);
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
            explosionInfo.dinfo = new DamageInfo(DamageDefOf.Bomb, 5, this.launcher, new BodyPartDamageInfo?(value), null);
            explosionInfo.postExplosionSpawnThingDef = this.def.projectile.postExplosionSpawnThingDef;
            explosionInfo.explosionSpawnChance = this.def.projectile.explosionSpawnChance;
            explosionInfo.explosionSound = this.def.projectile.soundExplode;
            explosionInfo.projectile = this.def;
            explosionInfo.DoExplosion();
        }
    }
}
