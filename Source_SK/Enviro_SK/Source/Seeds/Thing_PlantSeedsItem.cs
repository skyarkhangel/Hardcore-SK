using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using RimWorld;
using Verse;

namespace SK_Enviro.Seeds
{
    class Thing_PlantSeedsItem : ThingWithComps
    {
        // this isn't actually needed after all as the description needs to be changed in the ThingDef for it to register in the trade window
        // but leaving this commented out for now for later reference.

        ThingDef_PlantSeedItem m_CustomSeedDef = null;

        //public PlantGenome genome;

        public override void PostMake()
        {
            base.PostMake();
            m_CustomSeedDef = def as ThingDef_PlantSeedItem;
            //genome = ((ThingDef_PlantWithSeeds)m_CustomSeedDef.PlantProducedDef).genome.RandomGenome();
        }

        public override string GetDescription()
        {
            string sDescString = base.GetDescription();

            //if ( m_CustomSeedDef != null && m_CustomSeedDef.PlantProducedDef != null )
            //{
            //    sDescString = m_CustomSeedDef.PlantProducedDef.description + "\n\n" + sDescString + "\n\n" + "Genome".Translate() + "\n" + genome.ToString();
            //}

            return sDescString;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Deep.LookDeep<PlantGenome>(ref genome, "genome");
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
        }

        public override Thing SplitOff(int count)
        {
            var split = base.SplitOff(count);
            //((Thing_PlantSeedsItem)split).genome = genome;
            return split;
        }

        public override bool CanStackWith(Thing other)
        {
            if (!base.CanStackWith(other))
            {
                return false;
            }
            if (!(other is Thing_PlantSeedsItem))
            {
                return false;
            }
            //if ((other as Thing_PlantSeedsItem).genome.Equals(genome))
            //{
            //	return true;
            //}
            return true;// false;
        }
    }
}


