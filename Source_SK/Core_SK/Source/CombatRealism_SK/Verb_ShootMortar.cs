using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
namespace SK
{
	public class Verb_ShootMortar : Verb_ShootCR
	{

        //Mortar accuracy peaks at 50% of max range
		protected override bool TryCastShot()
		{
            this.forcedMissRadius = this.verbProps.forcedMissRadius;
            float targetDistance = (this.currentTarget.Cell - this.caster.Position).LengthHorizontal;
            // Log.Message("Target Distance: " + targetDistance.ToString());
            float rangePercentage = targetDistance / (this.verbProps.range / 2);
            if (rangePercentage <= 1)
            {
                this.forcedMissRadius *= 1 - rangePercentage;
               // Log.Message("Forced Miss Radius: " + this.forcedMissRadius.ToString());
            }
            else
            {
                this.forcedMissRadius *= (rangePercentage - 1) / 2;
              //  Log.Message("Forced Miss Radius: " + this.forcedMissRadius.ToString());
            }
            return base.TryCastShot();
		}
	}
}
