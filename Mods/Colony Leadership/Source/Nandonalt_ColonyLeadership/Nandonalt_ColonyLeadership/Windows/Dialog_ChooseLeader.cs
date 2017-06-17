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
        public class Dialog_ChooseLeader : Window
        {
            protected string curName;
            public Pawn chosenPawn;
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

            public Dialog_ChooseLeader()
            {
               
                this.forcePause = true;
                this.doCloseX = true;
                this.closeOnEscapeKey = true;
                this.absorbInputAroundWindow = false;
                this.closeOnClickedOutside = true;
            this.chosenPawn = null;
                 }

            public override void DoWindowContents(Rect inRect)
            {
                Text.Font = GameFont.Small;
                bool flag = false;

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                {
                    flag = true;
                    Event.current.Use();
                }
                Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);

            listing_Standard.Label("ChooseLeader".Translate());
            String label = (this.chosenPawn == null ? "NoneL".Translate() : this.chosenPawn.Name.ToStringShort);
            if (listing_Standard.ButtonText(label, null))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                List<Pawn> tpawns2 = new List<Pawn>();

                list.Add(new FloatMenuOption("-"+ "NoneL".Translate() +"-", delegate
                {
                    this.chosenPawn = null;
                }, MenuOptionPriority.Default, null, null, 0f, null, null));

                foreach (Pawn current in IncidentWorker_LeaderElection.getAllColonists())
                {
                    Hediff h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
                    Hediff h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
                    Hediff h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
                    Hediff h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
                    Hediff h5 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leaderExpired"));
                    if (h1 == null && h2 == null && h3 == null && h4 == null && h5 == null && !current.story.WorkTagIsDisabled(WorkTags.Social))  { tpawns2.Add(current); }
                   
                }

                foreach (Pawn p in tpawns2)
                {
                    list.Add(new FloatMenuOption(p.Name.ToStringShort, delegate
                    {
                        this.chosenPawn = p;
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                if (list.Count() == 1) TooltipHandler.TipRegion(inRect, "ChooseLeader_NoAbleColonists".Translate());
                               
                Find.WindowStack.Add(new FloatMenu(list));
            }

            
            if (listing_Standard.ButtonText("OK".Translate(), null))
                {
                if (this.chosenPawn != null)
                {
                    String targetLeader = "";
                    Pawn most = this.chosenPawn;
                    float maxValue = new float[] { IncidentWorker_LeaderElection.getBotanistScore(most), IncidentWorker_LeaderElection.getWarriorScore(most), IncidentWorker_LeaderElection.getCarpenterScore(most), IncidentWorker_LeaderElection.getScientistScore(most) }.Max();
                    if (maxValue == IncidentWorker_LeaderElection.getBotanistScore(most)) targetLeader = "leader1";
                    if (maxValue == IncidentWorker_LeaderElection.getWarriorScore(most)) targetLeader = "leader2";
                    if (maxValue == IncidentWorker_LeaderElection.getCarpenterScore(most)) targetLeader = "leader3";
                    if (maxValue == IncidentWorker_LeaderElection.getScientistScore(most)) targetLeader = "leader4";
                    Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named(targetLeader), most, null);
                    IncidentWorker_LeaderElection.doElect(most, hediff, true);

                }
                Find.WindowStack.TryRemove(this, true);
                
                }
                listing_Standard.End();
            }

        }
    
}
