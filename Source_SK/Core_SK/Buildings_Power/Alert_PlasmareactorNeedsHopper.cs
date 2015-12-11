using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
namespace SK_Plasmareactor
{
	public class Alert_PlasmareactorNeedsHopper : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
                foreach (Building_Plasmareactor current in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_Plasmareactor>())
				{
					bool flag = false;
					ThingDef thingDef = ThingDef.Named("PlasmaGenFeeder");
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
        public Alert_PlasmareactorNeedsHopper()
		{
            this.baseLabel = Translator.Translate("NeedPlasmaGenFeeder");
            this.baseExplanation = Translator.Translate("NeedPlasmaGenFeederDesc");
		}
	}
}
