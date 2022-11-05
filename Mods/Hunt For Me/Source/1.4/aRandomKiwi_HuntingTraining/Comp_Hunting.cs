using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;

namespace aRandomKiwi.HFM
{
    public class Comp_Hunting : ThingComp
    {
        public CompProps_Hunting Props
        {
            get
            {
                return (CompProps_Hunting)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.huntingForceNextGT, "huntingForceNextGT", 0, false);
            Scribe_Values.Look<bool>(ref this.huntingState, "huntingState", true, false);
            Scribe_Values.Look<bool>(ref this.huntingMode, "huntingMode", true, false);
            Scribe_Values.Look<bool>(ref this.huntingPreyMode, "huntingPreyMode", true, false);
            Scribe_Values.Look<bool>(ref this.huntingCanAssist, "huntingCanAssist", true, false);
            Scribe_Values.Look<bool>(ref this.huntingNotified, "huntingNotified", true, false);
            Scribe_Values.Look<int>(ref this.huntingPreyBusy, "huntingPreyBusy", 0, false);
            Scribe_Values.Look<bool>(ref this.huntingArrivedToWaitingPoint, "huntingArrivedToWaitingPoint", false, false);
            Scribe_Values.Look<IntVec3>(ref this.huntingWaitingPoint, "huntingWaitingPoint", new IntVec3(-1,-1,-1), false);
            Scribe_Collections.Look<Pawn>(ref huntingPackMembers, "huntingPackMembers", LookMode.Reference, new object[0]);
            Scribe_References.Look<Pawn>(ref huntingPackMaster, "huntingPackMaster", false);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void CompTick()
        {
            base.CompTick();

            //Notification that the pawn can again receive hunting orders
            if (!huntingNotified && nextGTOK())
            {
                if (!((Pawn)parent).Downed)
                {
                    if (Settings.cats.Contains(((Pawn)parent).kindDef.defName))
                        Messages.Message("HuntingForMe_NotificationCanForceKittyHunt".Translate(parent.LabelCap), new LookTargets(parent), MessageTypeDefOf.NeutralEvent);
                    else
                        Messages.Message("HuntingForMe_NotificationCanForceHunt".Translate(parent.LabelCap), new LookTargets(parent), MessageTypeDefOf.NeutralEvent);
                }
                huntingNotified = true;
            }
        }

        public override string CompInspectStringExtra()
        {
            base.CompInspectStringExtra();

            string ret = "";

            //IF waiting coordinate defined AND untingArrivedToWaitingPoint == false ==> in the process of joining the pack
            if (huntingWaitingPoint.x >= 0 && !huntingArrivedToWaitingPoint)
                ret = "HuntForMe_JoiningPackToHunt".Translate();
            //IF waiting coordinate defined AND untingArrivedToWaitingPoint == true ==> arrived at the training point ==> waiting for other members
            if (huntingWaitingPoint.x >= 0 && huntingArrivedToWaitingPoint)
                ret = "HuntForMe_WaintingForOtherToHunt".Translate();
            return ret;
        }

        public void setNextGT(bool notif=true)
        {
           huntingForceNextGT = Find.TickManager.TicksGame + Settings.timeToWaitBeforeTryHunt;
            if(notif)
                huntingNotified = false;
            else
                huntingNotified = true;
        }

        public void resetNextGT()
        {
            huntingForceNextGT = 1;
        }

        public bool nextGTOK()
        {
            return huntingForceNextGT < Find.TickManager.TicksGame;
        }

        public bool huntingArrivedToWaitingPoint = false;
        public IntVec3 huntingWaitingPoint = new IntVec3(-1,-1,-1);
        public List<Pawn> huntingPackMembers = new List<Pawn>();
        public Pawn huntingPackMaster;

        public bool huntingMode = true;
        public bool huntingNotified = true;
        public bool huntingPreyMode = true;
        public bool huntingCanAssist = true;
        //Serves as an indicator allowing you to know if the prey is already assigned to a hunter / squad (contains the number of hunter assigned to it)
        public int huntingPreyBusy = 0;

        public bool huntingState = true;

        public int huntingForceNextGT = 0;
    }
}