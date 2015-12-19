using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
namespace SK_HC
{
	public class Alert_MFNeedsHopper : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
                foreach (Building_HadronCollider current in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_HadronCollider>())
				{
					bool flag = false;
                    ThingDef thingDef = ThingDef.Named("HadronColliderFeeder");
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
        public Alert_MFNeedsHopper()
		{
            this.baseLabel = Translator.Translate("NeedHadronColliderFeeder");
            this.baseExplanation = Translator.Translate("NeedHadronColliderFeederDesc");
		}
	}
}
