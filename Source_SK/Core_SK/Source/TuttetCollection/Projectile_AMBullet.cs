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
  public class Projectile_AMBullet : Projectile
  {
    private const float StunChance = 0.1f;

    protected override void Impact(Thing hitThing)
    {
      base.Impact(hitThing);
      if (hitThing != null)
      {
        DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, this.def.projectile.damageAmountBase, this.launcher, this.ExactRotation.eulerAngles.y, new BodyPartDamageInfo?(new BodyPartDamageInfo(new BodyPartHeight?(), new BodyPartDepth?())), this.equipmentDef);
        hitThing.TakeDamage(dinfo);
        if (hitThing.def.category == ThingCategory.Pawn && hitThing.def.race.isFlesh)
        {
            ThrowBloodSplatter(this.ExactPosition);
        }
      }
      else
      {
        SoundStarter.PlayOneShot(SoundDefOf.BulletImpactGround, (SoundInfo) this.Position);
        MoteThrower.ThrowStatic(this.ExactPosition, ThingDefOf.Mote_ShotHit_Dirt, 2f);
      }
    }
    public static void ThrowBloodSplatter(Vector3 loc)
    {
        if (!loc.ShouldSpawnMotesAt())
        {
            return;
        }
        MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_BloodSplatter"), null);
        moteThrown.ScaleUniform = Rand.Range(2.4f, 2.7f);
        moteThrown.exactRotationRate = Rand.Range(0f, 0f);
        moteThrown.exactPosition = loc;
        moteThrown.exactPosition -= new Vector3(0.1f, 0f, 0.1f);
        moteThrown.exactPosition += new Vector3(Rand.Value, 0f, Rand.Value);
        moteThrown.SetVelocityAngleSpeed((float)Rand.Range(6, 8), Rand.Range(0.002f, 0.002f));
        GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
    }
  }
}