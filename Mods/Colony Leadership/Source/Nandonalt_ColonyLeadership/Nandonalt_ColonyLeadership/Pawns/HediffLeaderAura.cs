using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Nandonalt_ColonyLeadership
{
    public class HediffLeaderAura : HediffWithComps
    {
      private int counter = 0;
        public Pawn leader = null;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(ref this.leader, "leader", false);
        }


        public override void Notify_PawnDied()
        {
            this.pawn.health.RemoveHediff(this);
        }
        public override void Tick()
		{
			base.Tick();
          
            Pawn actor = this.pawn;
            this.counter += 1;
            bool flag = this.counter >= 100;
            if (flag )
            {
                if (leader.Map == null) this.pawn.health.RemoveHediff(this);

                bool leaderNearby = false;
                if (leader != null && leader.Map == this.pawn.Map && this.pawn.IsColonistPlayerControlled)
                {
                    string label;
                    if (this.def.defName == "leaderAura1") label = "leader1";
                    else if (this.def.defName == "leaderAura2") label = "leader2";
                    else if (this.def.defName == "leaderAura3") label = "leader3";
                    else label = "leader4";
                    /*
                    List<Thing> list = GenRadial.RadialDistinctThingsAround(actor.Position, actor.Map, 15f, true).ToList<Thing>();
                    foreach (Thing current in list)
                    {}*/
                    float distance = (this.pawn.Position - leader.Position).LengthHorizontal;
                    
                    if (distance <= 15f)
                    {
                        if (leader.GetRoom() == this.pawn.GetRoom())
                        {
                            Pawn pawn = leader;
                           
                                HediffLeader hediff = (HediffLeader)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(label));
                                if (hediff != null)
                                {
                                    leaderNearby = true;
                                    this.Severity =  Mathf.Clamp(hediff.Need.CurLevel,0.01f, 1f);
                                    if (pawn.CurJob != null && pawn.jobs.curDriver.asleep) leaderNearby = false;
                                    if (pawn.InAggroMentalState) leaderNearby = false;
                                }

                            
                        }
                    }
                }    

                if (!leaderNearby) this.pawn.health.RemoveHediff(this);
                }


            }

        }


    }

