using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine; 
using Verse;  
using Verse.Sound;
using RimWorld;

namespace SK_PlasmaWeapons
{
    public class Projectile_Plasma : Projectile
    {
        // Variables.
        public int tickCounter = 0;
        public Thing hitThing = null;

        // Miscellaneous.
        public const float burnChance = 0.1f;

        public override void SpawnSetup()
		{
			base.SpawnSetup();        
		}

        /// <summary>
        /// Impacts a pawn/object or the ground.
        /// </summary>
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            if (hitThing != null)
            {
                int damageAmountBase = this.def.projectile.damageAmountBase;
                BodyPartDamageInfo value = new BodyPartDamageInfo(null, null);
                DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, damageAmountBase, this.launcher, this.ExactRotation.eulerAngles.y, new BodyPartDamageInfo?(value), this.equipmentDef);
                hitThing.TakeDamage(dinfo);
                Pawn pawn = hitThing as Pawn;
                if (pawn != null && !pawn.Downed && Rand.Value < Projectile_Plasma.burnChance)
                {
                    MoteThrower.ThrowMicroSparks(this.destination);
                    hitThing.TryAttachFire(0.1f);
                }
            }
            else
            {
                SoundDefOf.BulletImpactGround.PlayOneShot(base.Position);
                MoteThrower.ThrowStatic(this.ExactPosition, ThingDefOf.Mote_ShotHit_Dirt, 1f);
                ThrowMicroSparksGreen(this.ExactPosition);
            }
        }
        public static void ThrowMicroSparksGreen(Vector3 loc)
        {
            if (!loc.ShouldSpawnMotesAt() || MoteCounter.Saturated)
            {
                return;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_MicroSparksGreen"), null);
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
