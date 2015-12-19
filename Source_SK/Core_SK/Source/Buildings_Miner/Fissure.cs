using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace SK_Industry
{
    public class Fissure : ThingWithComps
    {
        public FissureSize size;
        private static readonly Texture2D icon = ContentFinder<Texture2D>.Get("Terrain/Fissure");

        public Fissure()
        {
            int num = Rand.Range(1, 4);
            switch (num)
            {
                case 1:
                    this.size = FissureSize.Small;
                    break;
                case 2:
                    this.size = FissureSize.Medium;
                    break;
                case 3:
                    this.size = FissureSize.Large;
                    break;
                default:
                    this.size = FissureSize.Small;
                    break;
            }
        }
        public Fissure(FissureSize size)
        {
            this.size = size;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<FissureSize>(ref this.size, "fissureSize", 0, true);
        }

        public override string GetInspectString()
        {
            var str = new StringBuilder();
            str.Append(base.GetInspectString() + Environment.NewLine);
            str.Append("Fissure size: " + this.size.ToString());
            return str.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var c in base.GetGizmos())
            {
                yield return c;
            }
            //Delete the fissure
            var command = new Command_Action
            {

                icon = Fissure.icon,
                defaultDesc = "Fills the fissure in",
                defaultLabel = "Fill in fissure",
                activateSound = SoundDef.Named("Click"),
                action = delegate
                {
                    Find.WindowStack.Add(new Dialog_Confirm(
                        "Are you sure you wish to fill in this fissure?",
                        delegate { DeSpawn(); }));
                },
                disabled = HasMiner,
                groupKey = 313740005
            };
            yield return command;
        }

        public bool HasMiner
        {
            get
            {
                Building b = Find.ThingGrid.ThingAt<Building_Extractor>(this.Position);
                return b != null;
            }
        }
    }
}
