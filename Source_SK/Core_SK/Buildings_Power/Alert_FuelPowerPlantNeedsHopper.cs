using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
namespace SK_FuelPowerplant
{
	public class Alert_FuelPowerplantNeedsHopper : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
                foreach (Building_FuelPowerplant current in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_FuelPowerplant>())
				{
					bool flag = false;
                    ThingDef thingDef = ThingDef.Named("FuelFeeder");
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
        public Alert_FuelPowerplantNeedsHopper()
		{
            this.baseLabel = Translator.Translate("NeedFuelFeeder");
            this.baseExplanation = Translator.Translate("NeedFuelFeeder");
		}
	}
}
