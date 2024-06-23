namespace LetsGoExplore
{
    using System.Linq;
    using RimWorld;
    using Verse;
    using Verse.AI;
    using Verse.AI.Group;

    public class LordJob_RelaxAsAPrisoner : LordJob
    {
        private IntVec3 locus;

        //empty ctor for ExposeData
        public LordJob_RelaxAsAPrisoner()
        {
        }

        public LordJob_RelaxAsAPrisoner(IntVec3 locus)
        {
            this.locus = locus;
        }

        public override bool CanOpenAnyDoor(Pawn p)
            => lord.CurLordToil is LordToil_ExitMap;

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_MillAboutCloseBy chillax = new LordToil_MillAboutCloseBy(locus);
            stateGraph.AddToil(chillax);
            LordToil_ExitMap leaveMap = new LordToil_ExitMap(LocomotionUrgency.Jog);

            Transition signalToLeave = new Transition(chillax, leaveMap);
            signalToLeave.AddTrigger(new Trigger_PawnRecruited());
            signalToLeave.AddTrigger(new Trigger_Custom((signal) => signal.type == TriggerSignalType.FactionRelationsChanged));
            signalToLeave.AddTrigger(new Trigger_FightWon());
            signalToLeave.AddTrigger(new Trigger_Memo("SetFree")); //triggered by ThingComp_RescueMe
            //signalToLeave.AddPostAction(new TransitionAction_Message("prisoners leaving.")); <- in case you want to use that one instead, for added clarity (more generic than PrisonersFreedLGE)
            signalToLeave.AddPostAction(new TransitionAction_WakeAll());
            signalToLeave.AddPostAction(new TransitionAction_Custom(() =>
            {
                Find.World.worldObjects.SiteAt(lord.Map.Tile)?.GetComponent<PrisonerRescueQuestComp>()?.GiveRewardsAndSendLetter();
            }));
            signalToLeave.AddPostAction(new TransitionAction_EndAllJobs());

            stateGraph.AddTransition(signalToLeave);
            stateGraph.AddToil(leaveMap);

            return stateGraph;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.locus, "locus");
        }
    }

    public class Trigger_PawnRecruited : Trigger
    {
        public override bool ActivateOn(Lord lord, TriggerSignal signal)
            => signal.condition == PawnLostCondition.MadePrisoner
            || signal.condition == PawnLostCondition.ChangedFaction;
    }

    public class Trigger_FightWon : Trigger
    {
        private const float defaultFleeTrigger = 0.5f;

        public override bool ActivateOn(Lord lord, TriggerSignal signal)
        {
            Lord enemyLord = lord.lordManager.lords.Where(otherLord => otherLord != lord && lord.faction.def.humanlikeFaction).FirstOrDefault();

            if (enemyLord != null)
            {
                if (enemyLord.numPawnsLostViolently >= enemyLord.numPawnsEverGained * defaultFleeTrigger)
                {
                    Messages.Message("PrisonersFreedLGE".Translate(), lord.ownedPawns[0], MessageTypeDefOf.NeutralEvent);
                    return true;
                }
            }
            return false;
        }
    }
}
