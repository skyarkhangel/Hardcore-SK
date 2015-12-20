using UnityEngine;
using System.Collections;
﻿using RimWorld;
using Verse;

namespace SK_Oilfield
{
	public class CompLifespanFissure : ThingComp
	{
		public int age = -1;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<int>(ref this.age, "age", 0, false);
		}

		public override void CompTick()
		{
			this.age++;
			if (this.age >= this.props.lifespanTicks)
			{
				this.parent.Destroy(DestroyMode.Vanish);
                Find.LetterStack.ReceiveLetter("LetterLabelExhaustedFissure".Translate(), "ExhaustedFissure".Translate(), LetterType.BadNonUrgent, this.parent.Position, null);
			}
		}

		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			int num = this.props.lifespanTicks - this.age;
			if (num > 0)
			{
				text = string.Concat(new string[]
				{
					"Exhaustresource".Translate(),
					" ",
					num.TickstoDaysAndHoursString(),
					"\n",
					text
				});
			}
			return text;
		}
	}
}