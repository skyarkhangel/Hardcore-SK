using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
namespace SK_AMR
{
	public class Alert_Building_AntimatterReactorNeedsHopper : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
                foreach (Building_AntimatterReactor current in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_AntimatterReactor>())
				{
					bool flag = false;
                    ThingDef thingDef = ThingDef.Named("AntimatterReactorFeeder");
					using (IEnumerator<IntVec3> enumerator2 = GenAdj.CellsAdjacentCardinal(current).GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							Thing edifice = GridsUtility.GetEdifice(enumerator2.Current);
							if (edifice != null && edifice.def == thingDef)
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						return AlertReport.CulpritIs(current);
					}
				}
                return AlertReport.Inactive;
			}
		}
        public Alert_Building_AntimatterReactorNeedsHopper()
		{
            this.baseLabel = Translator.Translate("NeedAntimatterReactorFeeder");
            this.baseExplanation = Translator.Translate("NeedAntimatterReactorFeederDesc");
		}
	}
}
