using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurretCollection
{
  public class Projectile_StunBullet : Projectile
  {
    private const float StunChance = 0.1f;

    protected override void Impact(Thing hitThing)
    {
      base.Impact(hitThing);
      if (hitThing != null)
      {
          if (hitThing.def.category == ThingCategory.Pawn && hitThing.def.race.isFlesh)
          {
              DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, this.def.projectile.damageAmountBase, this.launcher, this.ExactRotation.eulerAngles.y, new BodyPartDamageInfo?(new BodyPartDamageInfo(new BodyPartHeight?(), new BodyPartDepth?())), this.equipmentDef);
              hitThing.TakeDamage(dinfo);
          }
      }
      else
      {
          SoundStarter.PlayOneShot(SoundDefOf.BulletImpactGround, (SoundInfo)this.Position);
          MoteThrower.ThrowStatic(this.ExactPosition, ThingDefOf.Mote_ShotHit_Dirt, 2f);
      }
    }
  }
}