using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
namespace SK_OR
{
	public class Alert_GCNeedsHopper : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
                foreach (Building_OilRefinery current in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_OilRefinery>())
				{
					bool flag = false;
                    ThingDef thingDef = ThingDef.Named("OilFeeder");
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
        public Alert_GCNeedsHopper()
		{
            this.baseLabel = Translator.Translate("NeedOilFeeder");
            this.baseExplanation = Translator.Translate("NeedOilFeederDesc");
		}
	}
}
