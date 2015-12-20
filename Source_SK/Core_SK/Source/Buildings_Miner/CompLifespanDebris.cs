using UnityEngine;
using System.Collections;
﻿using RimWorld;
using Verse;

namespace SK_LSDP
{

    public class CompLifespanDebris: ThingComp
    {
        public int remainingTicks = -1;

        CompPowerTrader CompPowerTrader
        {
            get
            {
                return parent.TryGetComp<CompPowerTrader>();
            }
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();

            if (remainingTicks < 0)
            {
                remainingTicks = props.lifespanTicks;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue<int>(ref remainingTicks, "remainingTicks", props.lifespanTicks, true);
        }

        public override void CompTick()
        {
            base.CompTick();
            TickDown(1);
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            TickDown(250);
        }

        public void TickDown(int down)
        {
            if (!CompPowerTrader.PowerOn)
            {
                return;
            }

            remainingTicks -= down;
            if (remainingTicks > 0)
            {
                return;
            }

            parent.Destroy();
            GenSpawn.Spawn(ThingDef.Named("ExtractorDebris"), this.parent.Position);
            Find.LetterStack.ReceiveLetter("LetterLabelExhaustedMine".Translate(), "ExhaustedMine".Translate(), LetterType.BadNonUrgent, this.parent.Position, null);
        }

        public override string CompInspectStringExtra()
        {
            if (remainingTicks > 0)
            {
                return "Exhaustresource".Translate() + " " +
                    remainingTicks.TickstoDaysAndHoursString() + "\n" +
                    base.CompInspectStringExtra();
            }
            return base.CompInspectStringExtra();
        }

    }

}