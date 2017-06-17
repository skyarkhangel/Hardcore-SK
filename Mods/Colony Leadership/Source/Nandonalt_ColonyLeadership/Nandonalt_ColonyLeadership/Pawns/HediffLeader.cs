using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Nandonalt_ColonyLeadership
{
    public class HediffLeader : HediffWithComps
    {
        private int counter = 0;
        static int ticksPeriod = GenDate.TicksPerYear;
        //static int ticksPeriod = GenDate.TicksPerHour;
        public int ticksLeader = ticksPeriod;


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksLeader, "ticksLeader", ticksPeriod);
         
        }
                
        public override string TipStringExtra
     {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (!Utility.isDictatorship) { stringBuilder.AppendLine("TimeLeftL".Translate() + (ticksLeader.ToStringTicksToPeriod(true))); }
                    stringBuilder.AppendLine("----------");
                    stringBuilder.Append(base.TipStringExtra); 
                return stringBuilder.ToString();
            }
        }

        public override void Notify_PawnDied()
        {
            if (!isThisExpired())
            {
                Messages.Message("LeaderDied".Translate(new object[] { this.def.label, this.pawn.Name.ToStringFull }), MessageSound.Negative);
                foreach (Pawn p in IncidentWorker_LeaderElection.getAllColonists())
                {
                    int num2 = p.relations.OpinionOf(this.pawn);
                    if (num2 <= -20)
                    {
                        p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("LeaderDiedRival"), this.pawn);
                    }
                    else
                    {
                        p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("LeaderDied"), this.pawn);
                    }
                }
                if(Utility.isDictatorship)Find.WindowStack.Add(new Dialog_ChooseLeader());
            }
         
            this.pawn.health.RemoveHediff(this);
            
        }

        public override string LabelBase
        {
            get
            {
                string lab = this.def.label;
                char[] MyChar = "Leader".Translate().ToCharArray();
                string NewString = lab.TrimStart(MyChar);
                GovType gov = Utility.getGov();
                if (gov == null) return this.def.label;
                if (this.pawn.gender == Gender.Female) return gov.nameMale + NewString;
                return gov.nameFemale + NewString;
            }
        }


        public bool isThisExpired()
        {
            return this.def.defName == "leaderExpired";
        }

        public Need_LeaderLevel Need
        {
            get
            {
                if (this.pawn.Dead)
                {
                    return null;
                }
                return (Need_LeaderLevel)this.pawn.needs.AllNeeds.Find((Need x) => x.def == DefDatabase<NeedDef>.GetNamed("LeaderLevel"));
            }
        }



        public override void Tick()
        {
            base.Tick();
             ticksLeader--;
            if(ticksLeader == 0 && !Utility.isDictatorship)
            {
                this.pawn.health.RemoveHediff(this);
                Find.LetterStack.ReceiveLetter("LeaderEndLetter".Translate(), "LeaderEndLetterDesc".Translate(new object[] { pawn.Name.ToStringFull }), LetterDefOf.BadNonUrgent, this.pawn, null);
            }
            if (!isThisExpired()) {
                this.Severity = Mathf.Clamp(this.Need.CurLevel,0.01f, 1f);
           
           Pawn actor = this.pawn;
           if (actor.CurJob != null && actor.jobs.curDriver.asleep) return;
           if (actor.InAggroMentalState) return;
           if (!actor.Spawned && actor.CarriedBy != null) return;
                if (actor.Map == null) return;
           this.counter += 1;
            bool flag = this.counter >= 100;

                if (flag)
                {
                    string label;
                    if (this.def.defName == "leader1") label = "leaderAura1";
                    else if (this.def.defName == "leader2") label = "leaderAura2";
                    else if (this.def.defName == "leader3") label = "leaderAura3";
                    else label = "leaderAura4";

                    List<Thing> list = GenRadial.RadialDistinctThingsAround(actor.Position, actor.Map, 10f, true).ToList<Thing>();
                    foreach (Thing current in list)
                    {
                        Pawn pawn = current as Pawn;
                        bool flag2 = pawn != null;
                        if (flag2)
                        {
                            if (pawn.IsColonistPlayerControlled && !pawn.Dead && pawn != actor)
                            {
                                if (pawn.GetRoom() == this.pawn.GetRoom())
                                {
                                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(label));
                                    if (hediff != null)
                                    {
                                        //hediff.Severity = Need.CurLevel;
                                    }
                                    else
                                    {
                                        hediff = HediffMaker.MakeHediff(HediffDef.Named(label), pawn, null);
                                        hediff.Severity = Mathf.Clamp(this.Need.CurLevel, 0.01f, 1f);
                                        HediffLeaderAura hl = (HediffLeaderAura)hediff;
                                        hl.leader = this.pawn;
                                        pawn.health.AddHediff(hl, null, null);
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }
    }
}