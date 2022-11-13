using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorldDaysMatter
{
    public class JoinableParty : LordJob_Joinable_Party
    {
        private Trigger_TicksPassed _timeoutTrigger;
        private IntVec3 _spot;
        private Pawn _organizer;
        private GatheringDef _gathering;
        public int durationTicks;
        private List<Pawn> _invited = null;
        
        public JoinableParty()
        { }

        public JoinableParty(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef, List<Pawn> invited = null)
            : base(spot, organizer, gatheringDef)
        {
            _spot = spot;
            _organizer = organizer;
            _gathering = gatheringDef;
            durationTicks = Rand.RangeInclusive(5000, 7300);
            if (invited != null)
                _invited = invited;
        }


        protected override bool ShouldBeCalledOff()
        {
            if (_organizer == null)
                return true;
            if (!GatheringsUtility.PawnCanStartOrContinueGathering(_organizer))
                return true;
            if (!GatheringsUtility.AcceptableGameConditionsToContinueGathering(base.Map))
                return true;
            return false;
        }

        private void ApplyOutcome(LordToil_Party toil)
        {
            List<Pawn> ownedPawns = lord.ownedPawns;
            LordToilData_Gathering lordToilData_Party = (LordToilData_Gathering)toil.data;
            for (int i = 0; i < ownedPawns.Count; i++)
            {
                Pawn pawn = ownedPawns[i];
                bool flag = pawn == _organizer;
                if (lordToilData_Party.presentForTicks.TryGetValue(pawn, out int value) && value > 0)
                {
                    if (ownedPawns[i].needs.mood != null)
                    {
                        ThoughtDef thoughtDef = flag ? OrganizerThought : AttendeeThought;
                        float num = 0.5f / thoughtDef.stages[0].baseMoodEffect;
                        float moodPowerFactor = Mathf.Min((float)value / (float)durationTicks + num, 1f);
                        Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
                        thought_Memory.moodPowerFactor = moodPowerFactor;
                        ownedPawns[i].needs.mood.thoughts.memories.TryGainMemory(thought_Memory);
                    }
                    TaleRecorder.RecordTale(flag ? OrganizerTale : AttendeeTale, ownedPawns[i], _organizer);
                }
            }
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_Party party = new LordToil_Party(_spot, _gathering);
            stateGraph.AddToil(party);
            LordToil_End lordToilEnd = new LordToil_End();
            stateGraph.AddToil(lordToilEnd);
            Transition transition = new Transition(party, lordToilEnd);
            transition.AddTrigger(new Trigger_TickCondition(ShouldBeCalledOff));
            //transition.AddTrigger(new Trigger_PawnLostViolently());
            transition.AddTrigger(new Trigger_PawnKilled());
            transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.LeftVoluntarily, _organizer));
            transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.ChangedFaction, _organizer));
            transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.Drafted, _organizer));
            transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.ExitedMap, _organizer));
            transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.ForcedByQuest, _organizer));
            transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.ForcedToJoinOtherLord, _organizer));
            transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.Incapped, _organizer));
            transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.Killed, _organizer));
            transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.MadePrisoner, _organizer));
            transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.Vanished, _organizer));
            transition.AddPreAction(new TransitionAction_Custom((Action)delegate
            {
                ApplyOutcome(party);
            }));
            transition.AddPreAction(new TransitionAction_Message(_gathering.calledOffMessage/*"MessagePartyCalledOff".Translate()*/, MessageTypeDefOf.NegativeEvent, new TargetInfo(_spot, Map)));
            stateGraph.AddTransition(transition);
            _timeoutTrigger = new Trigger_TicksPassed(durationTicks);
            Transition transition2 = new Transition(party, lordToilEnd);
            transition2.AddTrigger(_timeoutTrigger);
            transition2.AddPreAction(new TransitionAction_Custom((Action)delegate
            {
                ApplyOutcome(party);
            }));
            transition2.AddPreAction(new TransitionAction_Message(_gathering.finishedMessage/*"MessagePartyFinished".Translate()*/, MessageTypeDefOf.NegativeEvent, new TargetInfo(_spot, Map)));
            stateGraph.AddTransition(transition2);
            return stateGraph;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _spot, "RWDM_spot");
            Scribe_References.Look(ref _organizer, "RWDM_organizer");
            Scribe_Values.Look(ref durationTicks, "RWDM_durationTicks", 0);
            Scribe_Defs.Look(ref _gathering, "RWDM_gatheringDef");
            //Scribe_References.Look(ref _timeoutTrigger, "RWDM_timeoutTrigger");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && _gathering == null)
            {
                _gathering = GatheringDefOf.Party;
            }
            
        }

        private bool IsGatheringAboutToEnd()
        {
            if (_timeoutTrigger == null)
                return true;
            return _timeoutTrigger.TicksLeft < 1200;
        }

        public override float VoluntaryJoinPriorityFor(Pawn p)
        {
            if (p == null)
                return 0;

            if (!IsInvited(p))
                return 0f;
            if (!GatheringsUtility.ShouldPawnKeepGathering(p, _gathering))
                return 0f;

            if (_spot == null)
                return 0f;
            if (_spot.IsForbidden(p))
                return 0f;

            if (lord == null)
                return 0f;
            if (!lord.ownedPawns.Contains(p) && IsGatheringAboutToEnd())
                return 0f;

            return VoluntarilyJoinableLordJobJoinPriorities.SocialGathering;
        }

        private bool IsInvited(Pawn p)
        {
            if (p == null)
                return false;
            if (!p.IsColonist || !p.RaceProps.Humanlike || p.Drafted || p.Downed || !p.Spawned)
                return false;
            if (_invited != null)
                return _invited.Contains(p);
            if (lord != null && lord.faction != null)
                return p.Faction == lord.faction;
            return false;
        }
    }
}
