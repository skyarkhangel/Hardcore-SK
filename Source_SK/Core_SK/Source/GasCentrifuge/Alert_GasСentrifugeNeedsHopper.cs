using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
namespace SK_GC
{
	public class Alert_GCNeedsHopper : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
                foreach (Building_GC current in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_GC>())
				{
					bool flag = false;
                    ThingDef thingDef = ThingDef.Named("CentrifugeFeeder");
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
            this.baseLabel = Translator.Translate("NeedCentrifugeFeeder");
            this.baseExplanation = Translator.Translate("NeedCentrifugeFeederDesc");
		}
	}
}
