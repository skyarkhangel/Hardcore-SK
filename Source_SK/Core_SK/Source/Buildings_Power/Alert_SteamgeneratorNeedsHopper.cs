using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
namespace SK_Steamgenerator
{
	public class Alert_SteamgeneratorNeedsHopper : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
                foreach (Building_Steamgenerator current in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_Steamgenerator>())
				{
					bool flag = false;
					ThingDef thingDef = ThingDef.Named("GenHopper");
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
        public Alert_SteamgeneratorNeedsHopper()
		{
			this.baseLabel = Translator.Translate("NeedGenHopper");
            this.baseExplanation = Translator.Translate("NeedGenHopperDesc");
		}
	}
}
