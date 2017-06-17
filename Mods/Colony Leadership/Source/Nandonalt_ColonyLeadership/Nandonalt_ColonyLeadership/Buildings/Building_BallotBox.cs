using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI.Group;

namespace Nandonalt_ColonyLeadership
{
    public class Building_BallotBox : Building
    {
        public List<int> allowedDays = new List<int>(new int[] { 0, 6, 14 }); //Means day 1, 7 and 15 on a season
        public bool allowElection = true;
        public int lastElectionTick = -99999;


        protected virtual List<Pawn> getLeaders()
        {
            List<Pawn> pawns = new List<Pawn>();
            pawns.AddRange(IncidentWorker_LeaderElection.getAllColonists());
            List<Pawn> tpawns = new List<Pawn>();
            foreach (Pawn current in pawns)
            {
                Hediff h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
                Hediff h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
                Hediff h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
                Hediff h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));

                if (h1 != null || h2 != null || h3 != null || h4 != null) { }
                else
                {
                    tpawns.Add(current);
                }
            }
            foreach (Pawn current in tpawns)
            {
                pawns.Remove(current);
            }
            return pawns;
        }

        public override void TickRare()
        {
            base.TickRare();

            if (Utility.isDemocracy)
            {
                if (allowedDays.Contains(GenLocalDate.DayOfSeason(Map)))
                {
                    if (allowElection && (Find.TickManager.TicksGame > lastElectionTick + GenDate.TicksPerDay) && Rand.MTBEventOccurs(0.03f, 60000f, 150f) && AcceptableMapConditionsToStartElection(Map))
                    {
                        List<Pawn> leaders = getLeaders();
                        List<Pawn> pawnCount = new List<Pawn>();
                        bool flag = false;
                        pawnCount.AddRange(IncidentWorker_LeaderElection.getAllColonists());
                        if (leaders.NullOrEmpty())
                        {
                            flag = true;
                        }
                        if (leaders.Count <= 1 && pawnCount.Count >= 5)
                        {
                            flag = true;
                        }


                        List<Pawn> canBeVoted = new List<Pawn>();
                        canBeVoted.AddRange(IncidentWorker_LeaderElection.getAllColonists());
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
                            Messages.Message("ElectionFail_NoAbleLeader".Translate(), MessageSound.Negative);
                        }
                        else
                        {

                            if (flag) TryStartGathering(Map);
                        }
                    }
                }
            }
         }

    

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.allowElection, "allowElection", true);
            Scribe_Values.Look<int>(ref this.lastElectionTick, "lastElectionTick", -99999);
        }
        public bool TryStartGathering(Map map)
        {
            Pawn pawn = PartyUtility.FindRandomPartyOrganizer(Faction.OfPlayer, map);
            if (pawn == null)
            {
                Messages.Message("ElectionFail_ColonistsNotFound".Translate(), MessageSound.RejectInput);
                return false;
            }

            lastElectionTick = Find.TickManager.TicksGame;
            allowElection = false;
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_Joinable_LeaderElection(Position), map, null);
            Find.LetterStack.ReceiveLetter("Election".Translate(), "ElectionGathering".Translate(), LetterDefOf.Good, new TargetInfo(Position, map, false), null);
            return true;
        }

        public override string GetInspectString()
        {
            string inspectString = base.GetInspectString();
            string str = "-/-";
  
            if(this.lastElectionTick > 0)
            {
                str = (Find.TickManager.TicksGame - lastElectionTick).ToStringTicksToPeriod(true) + " ago.";
            }
            if(!Utility.isDemocracy) return inspectString + "ElectionFail_NoDemocracy".Translate();
            if(!allowElection) return inspectString + "BallotDescriptionDisabled".Translate() + str;
            return inspectString + "BallotDescription".Translate() + str;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }
            if (this.Faction == Faction.OfPlayer)
            {
                yield return new Command_Toggle
                {
                    hotKey = KeyBindingDefOf.CommandTogglePower,
                    icon = TexCommand.Forbidden,
                    defaultLabel = "EnableElections".Translate(),
                    defaultDesc = "EnableElectionsDesc".Translate(),
                    isActive = (() => this.allowElection),
                    toggleAction = delegate
                    {
                        this.allowElection = !this.allowElection;
                      
                    }
                };
            }
        }

 
  public bool AcceptableMapConditionsToStartElection(Map map)
        {
            if (!PartyUtility.AcceptableGameConditionsToContinueParty(map) || (!Position.Roofed(map) && !JoyUtility.EnjoyableOutsideNow(map, null)))
            {
                return false;
            }
            if (GenLocalDate.HourInteger(map) < 8 || GenLocalDate.HourInteger(map) > 21)
            {
                return false;
            }
            List<Lord> lords = map.lordManager.lords;
            for (int i = 0; i < lords.Count; i++)
            {
                if (lords[i].LordJob is LordJob_Joinable_Party || lords[i].LordJob is LordJob_Joinable_MarriageCeremony || lords[i].LordJob is LordJob_Joinable_LeaderElection)
                {
                    return false;
                }
            }
            if (map.dangerWatcher.DangerRating != StoryDanger.None)
            {
                return false;
            }
            int num2 = Mathf.RoundToInt((float)map.mapPawns.FreeColonistsSpawnedCount * 0.65f);
            num2 = Mathf.Clamp(num2, 2, 10);
            int num3 = 0;
            foreach (Pawn current2 in map.mapPawns.FreeColonistsSpawned)
            {
                if (PartyUtility.ShouldPawnKeepPartying(current2))
                {
                    num3++;
                }
            }
            return num3 >= num2;
        }



    }
}
