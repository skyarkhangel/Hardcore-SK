using RimWorld;
using System;
using UnityEngine;
using Verse;
namespace SK
{
	public class Verb_ShootShotgun : Verb_ShootCR
	{
        //shotgun shell fires 8 pellets
		protected override bool TryCastShot()
		{
            this.forcedMissRadius = this.verbProps.forcedMissRadius;
			if (base.TryCastShot())
			{
                int i = 1;
                while (i < 8 && base.TryCastShot())
                {
                    i++;
                }
                return true;
			}
			return false;
		}
	}
}
