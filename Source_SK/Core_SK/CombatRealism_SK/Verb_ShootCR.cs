using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
namespace SK
{
	public class Verb_ShootCR : Verse.Verb_Shoot
	{
        protected float forcedMissRadius;

        public override void WarmupComplete()
        {
            base.WarmupComplete();
        }

        //Custom virtual HitReportFor
        public virtual HitReport HitReportForCustom(TargetInfo target)
        {
            return base.HitReportFor(target);
        }

        /// <summary>
        /// Fires a projectile using a custom HitReportFor() method to override the vanilla one, as well as better collateral hit detection
        /// </summary>
        /// <returns>True for successful shot</returns>
        protected override bool TryCastShot()
        {
            ShootLine shootLine;
            if (!base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out shootLine))
            {
                return false;
            }
            Vector3 casterExactPosition = this.caster.DrawPos;
            Projectile projectile = (Projectile)ThingMaker.MakeThing(this.verbProps.projectileDef, null);
            GenSpawn.Spawn(projectile, shootLine.Source);
            float lengthHorizontalSquared = (this.currentTarget.Cell - this.caster.Position).LengthHorizontalSquared;

            //Forced Miss Calculations
            if (lengthHorizontalSquared < 9f)
            {
                this.forcedMissRadius = 0f;
            }
            else
            {
                if (lengthHorizontalSquared < 25f)
                {
                    this.forcedMissRadius *= 0.5f;
                }
                else
                {
                    if (lengthHorizontalSquared < 49f)
                    {
                        this.forcedMissRadius *= 0.8f;
                    }
                }
            }
            if (this.forcedMissRadius > 0.5f)
            {
                int max = GenRadial.NumCellsInRadius(this.forcedMissRadius);
                int rand = Rand.Range(0, max);
                if (rand > 0)
                {
                    IntVec3 newTarget = this.currentTarget.Cell + GenRadial.RadialPattern[rand];
                    projectile.canFreeIntercept = true;
                    TargetInfo target = newTarget;
                    if (!projectile.def.projectile.flyOverhead)
                    {
                        target = Utility.determineImpactPosition(this.caster.Position, newTarget, (int)(this.currentTarget.Cell - this.caster.Position).LengthHorizontal / 2);
                    }
                    projectile.Launch(this.caster, casterExactPosition, target, this.ownerEquipment);
                    if (this.currentTarget.HasThing)
                    {
                        projectile.AssignedMissTarget = this.currentTarget.Thing;
                    }
                    return true;
                }
            }

            HitReport hitReport = this.HitReportForCustom(this.currentTarget);

            //Wild Shot
            if (Rand.Value > hitReport.TotalNonWildShotChance)
            {
                shootLine.ChangeDestToMissWild();
                projectile.canFreeIntercept = true;
                TargetInfo target = shootLine.Dest;
                if (!projectile.def.projectile.flyOverhead)
                {
                    target = Utility.determineImpactPosition(this.caster.Position, shootLine.Dest, (int)(this.currentTarget.Cell - this.caster.Position).LengthHorizontal / 2);
                }
                projectile.Launch(this.caster, casterExactPosition, target, this.ownerEquipment);
                return true;
            }

            //Cover Shot
            if (Rand.Value > hitReport.HitChanceThroughCover && this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn)
            {
                Thing thing = hitReport.covers.RandomElementByWeight((CoverInfo c) => c.BlockChance).Thing;
                projectile.canFreeIntercept = true;
                projectile.Launch(this.caster, casterExactPosition, new TargetInfo(thing), this.ownerEquipment);
                return true;
            }

            //Hit
            if (this.currentTarget.Thing != null)
            {
                projectile.Launch(this.caster, casterExactPosition, new TargetInfo(this.currentTarget.Thing), this.ownerEquipment);
            }
            else
            {
                projectile.Launch(this.caster, casterExactPosition, new TargetInfo(shootLine.Dest), this.ownerEquipment);
            }
            return true;
        }
	}
}
