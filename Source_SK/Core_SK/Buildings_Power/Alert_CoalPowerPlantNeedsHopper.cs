using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
namespace SK_CoalPowerplant
{
	public class Alert_CoalPowerplantNeedsHopper : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
                foreach (Building_CoalPowerplant current in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_CoalPowerplant>())
				{
					bool flag = false;
                    ThingDef thingDef = ThingDef.Named("CoalFeeder");
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
        public Alert_CoalPowerplantNeedsHopper()
		{
            this.baseLabel = Translator.Translate("NeedCoalFeeder");
            this.baseExplanation = Translator.Translate("NeedCoalFeeder");
		}
	}
}
