using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System.Text;

namespace Nandonalt_ColonyLeadership
{
    public class Need_LeaderLevel : Need
    {
        public bool chosenToStay = false;
        private int lastGainTick = -999;
        private int lastLoseTick = -999;

        public Need_LeaderLevel(Pawn pawn) : base(pawn)
        {
            this.threshPercents = new List<float>();
            this.threshPercents.Add(0.33f);
            this.threshPercents.Add(0.66f);
         }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.chosenToStay, "chosenToStay", false);
        }

        public override void SetInitialLevel()
        {
            this.CurLevel = Rand.Range(0.5f, 0.8f);
        }

        public float opinion;
        private int ticks;


        public float getSkillFactor(Pawn current)
        {
     
            HediffLeader h1 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            HediffLeader h2 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            HediffLeader h3 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            HediffLeader h4 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
            float score = IncidentWorker_LeaderElection.getBotanistScore(current);

            if (h2 != null) { h1 = h2; score = IncidentWorker_LeaderElection.getWarriorScore(current); }
            if (h3 != null) { h1 = h3; score = IncidentWorker_LeaderElection.getCarpenterScore(current); }
            if (h4 != null) { h1 = h4; score = IncidentWorker_LeaderElection.getScientistScore(current); }

            return ((score / 20f) * 0.3f) + 1f;

        }



        public override void NeedInterval()
        {
            if (ticks >= 5)
            {
                float totalOpinion = 0;
                int totalPawns = 0;
                foreach (Pawn p in IncidentWorker_LeaderElection.getAllColonists())
                {
                    if (p != this.pawn && !p.Dead)
                    {
                        totalOpinion += p.relations.OpinionOf(this.pawn);
                        totalPawns++;
                    }
                }
                float medium = (totalOpinion / totalPawns);

                this.opinion = medium;
                float newlevel = (Mathf.Clamp(medium, 0f, 100f) / 100f) * getSkillFactor(this.pawn);



                if (this.curLevelInt < newlevel)
                {
                    this.lastGainTick = Find.TickManager.TicksGame;

                }
                else if (this.curLevelInt > newlevel)
                {
                    this.lastLoseTick = Find.TickManager.TicksGame;

                }
                if (newlevel > 1f) newlevel = 1f;
                this.curLevelInt = newlevel; ;



                if (totalPawns == 0) this.curLevelInt = 0.5f * getSkillFactor(this.pawn);
                ticks = 0;
                bool flag5 = false;
                if (!Utility.isDictatorship) { 
                if (!this.chosenToStay && opinion < -40 && opinion > -80)
                {
                    if (Rand.MTBEventOccurs(1.25f, 60000f, 150f))
                    {
                        flag5 = true;
                        StringBuilder str = new StringBuilder();
                        String s = "she";
                        if (this.pawn.gender == Gender.Male) s = "he";

                        String b = "her";
                        if (this.pawn.gender == Gender.Male) b = "his";

                        str.AppendLine("UnpopularM".Translate(new object[] { this.pawn.Name.ToStringFull }));
                        str.AppendLine("");
                        str.AppendLine("ResignLetterText".Translate(new object[] { this.pawn.Name.ToStringFull }));
                        str.AppendLine("");
                        str.AppendLine("OnlyOnce".Translate());
                        Action removeLeader = delegate
                        {
                            Find.LetterStack.ReceiveLetter("LeaderEndLetter".Translate(), "LeaderEndLetterDesc".Translate(new object[] { pawn.Name.ToStringFull }), LetterDefOf.BadNonUrgent, this.pawn, null);
                            this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("LeaderEnd"));
                            IncidentWorker_Rebellion.removeLeadership(this.pawn);
                        };
                        Action keepLeader = delegate
                        {
                            this.chosenToStay = true;
                        };


                        Dialog_MessageBox window = new Dialog_MessageBox(str.ToString(), "Resign".Translate(), removeLeader, "DontTesign".Translate(), keepLeader, "ResignationProposal".Translate(), false);
                        Find.WindowStack.Add(window);
                    }
                }

                if (!flag5 && opinion < -60)
                {
                    float mtb = 0.50f;
                    if (opinion < -70) mtb = 0.35f;
                    if (opinion < -80) mtb = 0.20f;
                    if (opinion < -90) mtb = 0.10f;

                    if (Rand.MTBEventOccurs(mtb, 60000f, 150f))
                    {
                        bool flag = false;
                        foreach (Pawn pa in IncidentWorker_LeaderElection.getAllColonists())
                        {
                            if (pa != null && pa.MentalState != null && pa.MentalState.def == DefDatabase<MentalStateDef>.GetNamed("Rebelling")) flag = true;
                        }
                        if (!flag)
                        {
                            IncidentParms parms = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, IncidentDef.Named("RebellionL").category, (this.pawn.Map));
                            if (IncidentDef.Named("RebellionL").Worker.TryExecute(parms))
                            {
                                String s = "her";
                                if (this.pawn.gender == Gender.Male) s = "his";
                                Find.LetterStack.ReceiveLetter("RebellionLetter".Translate(), "RebellionLetterDesc".Translate(new object[] { this.pawn.Name.ToStringFull }), LetterDefOf.BadNonUrgent, this.pawn, null);
                            }
                        }
                    }
                }
            }
            }
            ticks++;
        }

        public override int GUIChangeArrow
        {
            get
            {
                if (Find.TickManager.TicksGame < this.lastGainTick + 20)
                {
                    return 1;
                }
                else if(Find.TickManager.TicksGame < this.lastLoseTick + 20)
                {
                    return -1;
                }
                
                return 0;
            }
        }

    


        public int getLeaderLevel()
        {
            if (this.curLevelInt <= 0.33f)
            {
                return 1;
            }
            else if (this.curLevelInt <= 0.66)
            {
                return 2;

            }
            return 3;
        }


        public override string GetTipString()
        {
            return base.GetTipString() + "\n" + "LeaderLevel".Translate() + this.getLeaderLevel().ToString() + "\n" + "AverageOpinion".Translate() + this.opinion.ToString();
        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
        {
            if (rect.height > 70f)
            {
                float num = (rect.height - 70f) / 2f;
                rect.height = 70f;
                rect.y += num;
            }
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }
            if (doTooltip)
            {
                TooltipHandler.TipRegion(rect, new TipSignal(() => this.GetTipString(), rect.GetHashCode()));
            }
            float num2 = 14f;
            float num3 = (customMargin < 0f) ? (num2 + 15f) : customMargin;
            if (rect.height < 50f)
            {
                num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
            }
            Text.Font = ((rect.height <= 55f) ? GameFont.Tiny : GameFont.Small);
            Text.Anchor = TextAnchor.LowerLeft;
            Rect rect2 = new Rect(rect.x + num3 + rect.width * 0.1f, rect.y, rect.width - num3 - rect.width * 0.1f, rect.height / 2f);
            Widgets.Label(rect2, this.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
            rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
            Widgets.FillableBar(rect3, this.CurLevelPercentage, ModTextures.GreenTex);
            if (drawArrows)
            {
                Widgets.FillableBarChangeArrows(rect3, this.GUIChangeArrow);
            }
            if (this.threshPercents != null)
            {
                for (int i = 0; i < Mathf.Min(this.threshPercents.Count, maxThresholdMarkers); i++)
                {
                    this.DrawBarThreshold(rect3, this.threshPercents[i]);
                }
            }
            float curInstantLevelPercentage = this.CurInstantLevelPercentage;
            if (curInstantLevelPercentage >= 0f)
            {
                this.DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage);
            }
            if (!this.def.tutorHighlightTag.NullOrEmpty())
            {
                UIHighlighter.HighlightOpportunity(rect, this.def.tutorHighlightTag);
            }
            Text.Font = GameFont.Small;
        }

        private void DrawBarThreshold(Rect barRect, float threshPct)
        {
            float num = (float)((barRect.width <= 60f) ? 1 : 2);
            Rect position = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
            Texture2D image;
            if (threshPct < this.CurLevelPercentage)
            {
                image = BaseContent.BlackTex;
                GUI.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else
            {
                image = BaseContent.GreyTex;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }
            GUI.DrawTexture(position, image);
            GUI.color = Color.white;
        }

        public override float CurInstantLevel
        {
            get
            {
                 return this.curLevelInt;
            }
        }
    }
}
