using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace SK
{
    class Verb_ShootRifle : Verb_ShootCR
    {
        //Adjusted for longer ranges
        public override HitReport HitReportForCustom(Verse.TargetInfo target)
        {
            IntVec3 cell = target.Cell;
            HitReport hitReport = new HitReport();
            hitReport.shotDistance = (cell - this.caster.Position).LengthHorizontal;
            hitReport.target = target;
            if (!this.verbProps.canMiss)
            {
                hitReport.hitChanceThroughPawnStat = 0.99f; //Down from 1 so turrets no longer ignore range penalties
                hitReport.covers = new List<CoverInfo>();
                hitReport.coversOverallBlockChance = 0f;
            }
            else
            {
                float f = 1f;
                if (base.CasterIsPawn)
                {
                    f = base.CasterPawn.GetStatValue(StatDefOf.ShootingAccuracy, true);
                }
                hitReport.hitChanceThroughPawnStat = Mathf.Pow(f, hitReport.shotDistance / 10); //Adjusted for better long range accuracy
                if (hitReport.hitChanceThroughPawnStat < 0.0201f)
                {
                    hitReport.hitChanceThroughPawnStat = 0.0201f;
                }
                if (base.CasterIsPawn)
                {
                    hitReport.hitChanceThroughSightEfficiency = base.CasterPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Sight);
                }
                hitReport.hitChanceThroughEquipment = this.verbProps.HitMultiplierAtDist(hitReport.shotDistance, this.ownerEquipment);
                hitReport.forcedMissRadius = this.verbProps.forcedMissRadius;
                hitReport.covers = CoverUtility.CalculateCoverGiverSet(cell, this.caster.Position);
                hitReport.coversOverallBlockChance = CoverUtility.CalculateOverallBlockChance(cell, this.caster.Position);
                hitReport.targetLighting = Find.GlowGrid.PsychGlowAt(cell);
                if (!this.caster.Position.Roofed() && !target.Cell.Roofed())
                {
                    hitReport.hitChanceThroughWeather = Find.WeatherManager.CurWeatherAccuracyMultiplier;
                }
            }
            return hitReport;
        }

        protected override bool TryCastShot()
        {
            this.forcedMissRadius = this.verbProps.forcedMissRadius;
            return base.TryCastShot();
        }
    }
}
