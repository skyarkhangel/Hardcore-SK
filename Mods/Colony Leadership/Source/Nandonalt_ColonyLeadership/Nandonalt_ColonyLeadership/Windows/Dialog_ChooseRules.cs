using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Collections;

namespace Nandonalt_ColonyLeadership
{
    public class Dialog_ChooseRules : Window
    {
        protected string curName;
        public GovType chosenLeadership;
        public int MaxSize;
        public int MinSize;
        public string MaxSizebuf;
        public string MinSizebuf;
        public bool permanent;
        
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(280f, 170f);
            }
        }

        public Dialog_ChooseRules()
        {

            this.forcePause = true;
            this.doCloseX = false;
            this.closeOnEscapeKey = false;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = false;
            this.chosenLeadership = ColonyLeadership.govtypes[0];
        }


        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);

            listing_Standard.Label("ChooseGov".Translate());

            String label = this.chosenLeadership.name;
            if (listing_Standard.ButtonText(label, null))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();

                foreach (GovType gov in ColonyLeadership.govtypes)
                {
                    list.Add(new FloatMenuOption(gov.name, delegate
                    {
                    this.chosenLeadership = gov;
                    }, MenuOptionPriority.Default, delegate () { TooltipHandler.TipRegion(inRect, gov.desc); }, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            listing_Standard.Gap(24f);
            if (listing_Standard.ButtonText("OK".Translate(), null))
            {
                GameComponent_ColonyLeadership comp = Utility.getCLComp();
                if (comp != null) comp.chosenLeadership = this.chosenLeadership;
                if (comp != null) comp.chosenGov = ColonyLeadership.govtypes.IndexOf(this.chosenLeadership);
                Find.WindowStack.TryRemove(this, true);
                if(this.chosenLeadership.name == "Dictatorship".Translate())
                {
                    foreach (Pawn p in IncidentWorker_LeaderElection.getAllColonists())
                    {
                        LeaderWindow.purgeLeadership(p);
                    }

                    Find.WindowStack.Add(new Dialog_ChooseLeader());
                }

            }
            listing_Standard.End();
        }

    }

}
