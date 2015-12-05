using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace SK
{
    /// <summary>
    /// Explosive with fragmentation effect
    /// </summary>
    public class Projectile_ExplosiveFragNoTrail : Projectile_Explosive
	{
        public static readonly SoundDef BombShreek = SoundDef.Named("MortarShreekBeforeImpact");
        public static readonly SoundDef BombWhistle = SoundDef.Named("MortarWhistleBeforeImpact");

        //frag variables
        private int fragAmountSmall = 0;
        private int fragAmountMedium = 0;
        private int fragAmountLarge = 0;

        private float fragRange = 0;

        private ThingDef fragProjectileSmall = null;
        private ThingDef fragProjectileMedium = null;
        private ThingDef fragProjectileLarge = null;

        public override void Tick()
        {
            base.Tick();
            if (this.ticksToImpact <= 18f)
            {
                //	this.sustainerIsRunning = false;
                Projectile_ExplosiveFragNoTrail.BombShreek.PlayOneShot(this.Position);
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            MoteThrower.ThrowDustPuff(base.Position.ToVector3Shifted(), 4f);
        }

        public class ExhaustFlames
        {
            public static void ThrowSmokeForRocketsandMortars(Vector3 loc, float size)
            {
                IntVec3 intVec = loc.ToIntVec3();
                if (!intVec.InBounds())
                {
                    return;
                }
                MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Smoke, null);
                moteThrown.ScaleUniform = Rand.Range(1.5f, 2.5f) * size;
                moteThrown.exactRotationRate = Rand.Range(-0.5f, 0.5f);
                moteThrown.exactPosition = loc;
                moteThrown.SetVelocityAngleSpeed((float)Rand.Range(30, 40), Rand.Range(0.008f, 0.012f));
                GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
            }
        }

        public bool getParameters()
        {
            SK.ThingDef_ProjectileFrag projectileDef = this.def as SK.ThingDef_ProjectileFrag;
            if (projectileDef.fragAmountSmall + projectileDef.fragAmountMedium + projectileDef.fragAmountLarge > 0
                && projectileDef.fragRange > 0
                && projectileDef.fragProjectileSmall != null
                && projectileDef.fragProjectileMedium != null
                && projectileDef.fragProjectileLarge != null)
            {
                this.fragAmountSmall = projectileDef.fragAmountSmall;
                this.fragAmountMedium = projectileDef.fragAmountMedium;
                this.fragAmountLarge = projectileDef.fragAmountLarge;

                this.fragRange = projectileDef.fragRange;

                this.fragProjectileSmall = projectileDef.fragProjectileSmall;
                this.fragProjectileMedium = projectileDef.fragProjectileMedium;
                this.fragProjectileLarge = projectileDef.fragProjectileLarge;

                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Scatters fragments around
        /// </summary>
        protected virtual void ScatterFragments(int radius, ThingDef projectileDef)
        {
            Projectile projectile = (Projectile)ThingMaker.MakeThing(projectileDef, null);
            int rand = Rand.Range(0, radius);
            TargetInfo targetCell = this.Position + GenRadial.RadialPattern[rand];
            TargetInfo target = Utility.determineImpactPosition(this.Position, targetCell);
            GenSpawn.Spawn(projectile, this.Position);
            projectile.Launch(this, target, this.launcher);
        }

        /// <summary>
        /// Explode and scatter fragments around
        /// </summary>
		protected override void Explode()
        {
            if (this.getParameters())
            {
                int radius = GenRadial.NumCellsInRadius(this.fragRange);

                //Spawn projectiles
                for (int i = 0; i < fragAmountSmall; i++)
                {
                    this.ScatterFragments(radius, this.fragProjectileSmall);
                }
                for (int i = 0; i < fragAmountMedium; i++)
                {
                    this.ScatterFragments(radius, this.fragProjectileMedium);
                }
                for (int i = 0; i < fragAmountLarge; i++)
                {
                    this.ScatterFragments(radius, this.fragProjectileLarge);
                }
            }
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
