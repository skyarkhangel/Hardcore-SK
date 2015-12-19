using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace SK_Radiation
{
    public class Hediff_Radiation : Hediff_Injury
	{
		public override void Tick()
		{
			base.Tick();
			if (Gen.IsHashIntervalTick((Thing) this.pawn, GenDate.TicksPerYear/100))
				this.Severity -= 0.01f;
		}
	}
}

