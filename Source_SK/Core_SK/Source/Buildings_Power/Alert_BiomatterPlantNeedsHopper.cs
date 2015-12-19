using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
namespace SK_Biogenerator
{
	public class Alert_BiogeneratorNeedsHopper : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
                foreach (Building_Biogenerator current in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_Biogenerator>())
				{
					bool flag = false;
					ThingDef thingDef = ThingDef.Named("BioGenHopper");
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
		public Alert_BiogeneratorNeedsHopper()
		{
			this.baseLabel = Translator.Translate("NeedGenBioHopper");
            this.baseExplanation = Translator.Translate("NeedGenBioHopperDesc");
		}
	}
}
