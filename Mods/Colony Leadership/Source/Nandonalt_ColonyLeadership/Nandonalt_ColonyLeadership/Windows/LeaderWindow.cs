using System;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace Nandonalt_ColonyLeadership
{
    public class LeaderWindow : MainTabWindow
    {
        private const float FactionColorRectSize = 15f;

        private const float FactionColorRectGap = 10f;

        private const float RowMinHeight = 80f;

        private const float LabelRowHeight = 50f;

        private const float TypeColumnWidth = 100f;

        private const float NameColumnWidth = 220f;

        private const float RelationsColumnWidth = 100f;

        private const float NameLeftMargin = 15f;
        private bool pawnListDirty;


        protected List<Pawn> pawns = new List<Pawn>();


        private Vector2 scrollPosition = Vector2.zero;

        private float scrollViewHeight;

        public override void PreOpen()
        {
            base.PreOpen();
            this.BuildPawnList();
        }
        public List<Pawn> getAllColonists()
        {

            List<Pawn> pawns = new List<Pawn>();
            pawns.AddRange(PawnsFinder.AllMapsCaravansAndTravelingTransportPods_FreeColonists);
            return pawns;
        }

        public override void PostOpen()
        {
            base.PostOpen();

            if (ColonyLeadership.lastReadVersion != ColonyLeadership.newVersion)
            {
     
                Dialog_MessageBox window = new Dialog_MessageBox(ColonyLeadership.updateNotes, "Thanks!", null, null, null, null, false);
                window.soundAmbient = SoundDefOf.RadioComms_Ambience;
                Find.WindowStack.Add(window);
                ColonyLeadership.lastReadVersion = ColonyLeadership.newVersion;
                ColonyLeadership.Save();
                DefDatabase<MainButtonDef>.GetNamed("LeaderTab").ClearCachedData();
                DefDatabase<MainButtonDef>.GetNamed("LeaderTab").label = "Leadership";

            }
                        
        }


        public static void purgeLeadership(Pawn current)
        {
            HediffLeader h1 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            HediffLeader h2 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            HediffLeader h3 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            HediffLeader h4 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
            HediffLeader h5 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leaderExpired"));

           if (h1 != null) current.health.RemoveHediff(h1);
            if (h2 != null) current.health.RemoveHediff(h2);
            if (h3 != null) current.health.RemoveHediff(h3);
            if (h4 != null) current.health.RemoveHediff(h4);
            if (h5 != null) current.health.RemoveHediff(h5);
            current.needs.AddOrRemoveNeedsAsAppropriate();

        }

        protected virtual void BuildPawnList()
        {
            this.pawns.Clear();
            this.pawns.AddRange(getAllColonists());
            List<Pawn> tpawns = new List<Pawn>();
            foreach(Pawn current in pawns)
            {
                Hediff h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
                Hediff h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
                Hediff h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
                Hediff h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
  
                if (h1 != null || h2 != null || h3 != null || h4 != null ) { }
                else
                {
                    tpawns.Add(current);
                }
            }
            foreach(Pawn current in tpawns)
            {
                this.pawns.Remove(current);
            }
            this.pawnListDirty = false;
        }

        public void Notify_PawnsChanged()
        {
            this.pawnListDirty = true;
        }

        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(500f, 250f);
            }
        }
         

        public override void DoWindowContents(Rect fillRect)
        {
            base.DoWindowContents(fillRect);
            this.BuildPawnList();
            // GenDraw.DrawRadiusRing(pawns[0].Position, 10f);
            Rect position = new Rect(0f, 0f, fillRect.width, fillRect.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 50f, position.width, position.height - 50f);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(5f, 5f, 140f, 30f), "Leaders");
            Text.Font = GameFont.Small;
            List<Pawn> pawnCount = new List<Pawn>();
            bool flag = false;
             pawnCount.AddRange(getAllColonists());
            if (Prefs.DevMode)
            {
                if (this.pawns.NullOrEmpty())
                {
                    flag = true;
                }
                if (this.pawns.Count <= 1 && pawnCount.Count >= 5)
                {
                    flag = true;
                }

                {
                    //if (flag == true && Find.VisibleMap != null)
                    //{
                        Rect button = new Rect(90f, 5f, 150f, 40f);
                        if (Widgets.ButtonText(button, "DEV: Reset Leadership Type", true, false, true))
                        {
                            Find.WindowStack.Add(new Dialog_ChooseRules());
                            /*
                            TooltipHandler.TipRegion(button, "Gather colonists to vote for their leaders. Will start a election on the visible map.");
                            if (Find.VisibleMap.lordManager.lords.Find(x => x.LordJob.GetType() == typeof(LordJob_Joinable_LeaderElection)) != null)
                            {
                                Messages.Message("The colony is already gathering for an election.", MessageSound.RejectInput);
                            }
                            else
                            {
                                List<Pawn> canBeVoted = new List<Pawn>();
                                canBeVoted.AddRange(getAllColonists());
                                List<Pawn> tpawns2 = new List<Pawn>();
                                foreach (Pawn current in canBeVoted)
                                {
                                    Hediff h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
                                    Hediff h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
                                    Hediff h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
                                    Hediff h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
                                    Hediff h5 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leaderExpired"));
                                    if (h1 != null || h2 != null || h3 != null || h4 != null || h5 != null) { tpawns2.Add(current); }
                                    if (current.story.WorkTagIsDisabled(WorkTags.Social)) { tpawns2.Add(current); }
                                }
                                foreach (Pawn current in tpawns2)
                                {
                                    canBeVoted.Remove(current);
                                }
                                if (canBeVoted.NullOrEmpty())
                                {
                                    Messages.Message("No colonist is able to be a leader.", MessageSound.Negative);
                                }
                                else
                                {

                                    //IncidentDef.Named("LeaderElection").Worker.TryExecute(parms);
                                    IncidentWorker_LeaderElection.TryStartGathering(Find.VisibleMap);
                                }

                            }
                            */
                      //  }
                    }
                }
            }



            Rect button3 = new Rect(250f, 5f, 100f, 40f);
            String stg = "DEV: Add Leader";
            if (Utility.isDictatorship) stg = "SetDictator".Translate();
            if ((Prefs.DevMode || (Utility.isDictatorship && pawns.Count() <= 0)) && Widgets.ButtonText(button3, stg, true, false, true))
            {
                Find.WindowStack.Add(new Dialog_ChooseLeader());
            }


            Rect button2 = new Rect(360f, 5f, 100f, 40f);
            if (Prefs.DevMode && Widgets.ButtonText(button2, "DEV: Purge Leaders", true, false, true))
            {
                foreach(Pawn p in getAllColonists())
                {
                    purgeLeadership(p);
                    this.BuildPawnList();
                }

            }

            if (pawns.NullOrEmpty())
            {
             
                if(Utility.isDemocracy)Widgets.Label(new Rect(5f, 50f, 200f, 200f), "BuildBallotBox".Translate());
                
            }

            Rect rect = new Rect(0f, 0f, position.width - 16f, this.scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, rect);
            float num = 0f;
            foreach (Pawn current in pawns)
            {
                    GUI.color = new Color(1f, 1f, 1f, 0.2f);
                    Widgets.DrawLineHorizontal(0f, num, rect.width);
                    GUI.color = Color.white;
              
                num += this.DrawLeaderRow(current, num, rect);

            }
            if (Event.current.type == EventType.Layout)
            {
                this.scrollViewHeight = num;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private String leaderType(Pawn current)
        {
            Hediff h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            if (h1 != null) return h1.Label;
            h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            if (h1 != null) return h1.Label;
            h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            if (h1 != null) return h1.Label;
            h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
            if (h1 != null) return h1.Label;
            return "Error?";
        }

        private float DrawLeaderRow(Pawn leader, float rowY, Rect fillRect)
        {
            Rect rect = new Rect(40f, rowY, 300f, 80f);
            Need_LeaderLevel need = (Need_LeaderLevel)leader.needs.AllNeeds.Find((Need x) => x.def == DefDatabase<NeedDef>.GetNamed("LeaderLevel"));
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");               
            string text = stringBuilder.ToString();
            float width = fillRect.width - rect.xMax;
            float num = Text.CalcHeight(text, width);
            float num2 = Mathf.Max(80f, num);
            Rect position = new Rect(8f, rowY + 12f, 30f, 30f);
            Rect rect2 = new Rect(0f, rowY, fillRect.width, num2);
            if (Mouse.IsOver(rect2))
            {
                StringBuilder stringBuilder2 = new StringBuilder();
                stringBuilder2.AppendLine("AverageOpinionAbout".Translate() + need.opinion);


                if (need.opinion < -60 && !Utility.isDictatorship) stringBuilder2.AppendLine("UnpopularLeader".Translate());
                else if (need.opinion < -20) stringBuilder2.AppendLine("UnlikedLeader".Translate());
                TooltipHandler.TipRegion(rect2, stringBuilder2.ToString());
                GUI.DrawTexture(rect2, TexUI.HighlightTex);
                if(Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0)
                    {
                        CameraJumper.TryJumpAndSelect(leader);
                    }
                    }
            }
            Text.Font = GameFont.Medium;
        
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.ThingIcon(position, leader, 1f);
         
            //Widgets.DrawRectFast(position, Color.white, null);
            string label = string.Concat(new string[]
            {
                leader.Name.ToStringFull,
                "\n",
                "   ",
                leaderType(leader),
                "\n"
            });
            if (need.opinion < -20) GUI.color = Color.yellow;
            if (need.opinion < -60) GUI.color = Color.red;
            Widgets.Label(rect, label);
            GUI.color = Color.white;
            return num2;
        }

    }
}
