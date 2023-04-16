using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI.Group;
using System.Diagnostics;

namespace LetsGoExplore
{
    public class WorldObject_InterceptedMessageLGE : WorldObject
    {
        protected const float treasureChance = 0.66f;

        protected const float treasureAmbushChance = 0.7f;

        protected const float humanAmbushChance = 0.6f;

        protected const float ManhunterAmbushPointsFactor = 1.0f;

        protected const float MinimumPointThreshold = 400.0f;

        public void Notify_CaravanArrived(Caravan caravan)
        {
            if (!Rand.Chance(treasureChance))
            {
                if (Rand.Chance(humanAmbushChance))
                {
                    CaravanAmbushHumansNoTreasure(caravan);
                }
                else
                {
                    CaravanAmbushManhuntersNoTreasure(caravan);
                }
            }
            else
            {
                if (Rand.Chance(treasureAmbushChance))
                {
                    if (Rand.Chance(humanAmbushChance))
                    {
                        TreasureWithHumanAmbush(caravan);
                    }
                    else
                    {
                        TreasureWithManhunters(caravan);
                    }
                }
                else
                {
                    FoundTreasure(caravan);
                }
            }
            Find.WorldObjects.Remove(this);
        }

        protected void CaravanAmbushHumansNoTreasure(Caravan caravan)
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, caravan);
                //make sure a minimum point threshold is hit
                if(incidentParms.points < MinimumPointThreshold)
                {
                    incidentParms.points = MinimumPointThreshold;
                }
                incidentParms.faction = Find.FactionManager.RandomEnemyFaction();
                PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, true);
                defaultPawnGroupMakerParms.generateFightersOnly = true;
                List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, true).ToList<Pawn>();
                Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, list, false);
                if (list.Any<Pawn>())
                {
                    LordMaker.MakeNewLord(incidentParms.faction, new LordJob_AssaultColony(incidentParms.faction, true, true, false, false, true), map, list);
                }
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                GlobalTargetInfo lookTarget = (!list.Any<Pawn>()) ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(list[0].Position, map, false);
                Find.LetterStack.ReceiveLetter("LetterLabelIMAmbushLGE".Translate(), "LetterIMAmbushLGE".Translate(), LetterDefOf.ThreatBig, lookTarget, null);
            }, "GeneratingMapForNewEncounter", false, null);
        }

        protected void CaravanAmbushManhuntersNoTreasure(Caravan caravan)
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, caravan);
                //make sure a minimum point threshold is hit
                if (incidentParms.points < MinimumPointThreshold)
                {
                    incidentParms.points = MinimumPointThreshold;
                }
                PawnKindDef animalKind;
                if (!ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(this.AdjustedPoints(incidentParms.points), incidentParms.target.Tile, out animalKind) && !ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(this.AdjustedPoints(incidentParms.points), -1, out animalKind))
                {
                    Log.Warning("Could not find any valid animal kind for " + this.def + " incident. Going with Wargs", false);
                    animalKind = ThingDefOfVanilla.Warg;
                }
                List<Pawn> list = ManhunterPackIncidentUtility.GenerateAnimals(animalKind, incidentParms.target.Tile, this.AdjustedPoints(incidentParms.points));
                Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, list, false);
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, null, false);
                }
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                GlobalTargetInfo lookTarget = (!list.Any<Pawn>()) ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(list[0].Position, map, false);
                Find.LetterStack.ReceiveLetter("LetterLabelIMAmbushLGE".Translate(), "LetterIMAmbushLGE".Translate(), LetterDefOf.ThreatBig, lookTarget, null);
            }, "GeneratingMapForNewEncounter", false, null);
        }

        protected void FoundTreasure(Caravan caravan)
        {
            List<Thing> list = GenerateReward();
            for (int i = 0; i < list.Count; i++)
            {
                caravan.AddPawnOrItem(list[i], true);
            }
            Find.LetterStack.ReceiveLetter("LetterLabelFoundTreasureLGE".Translate(), "LetterFoundTreasureLGE".Translate(GenThing.GetMarketValue(list).ToStringMoney(null), GenLabel.ThingsLabel(list, string.Empty)), LetterDefOf.PositiveEvent, caravan, null);
        }

        protected void TreasureWithHumanAmbush(Caravan caravan)
        {
            List<Thing> rewardList = GenerateReward();
            for (int i = 0; i < rewardList.Count; i++)
            {
                caravan.AddPawnOrItem(rewardList[i], true);
            }
            LongEventHandler.QueueLongEvent(delegate
            {
                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, caravan);
                //make sure a minimum point threshold is hit
                if (incidentParms.points < MinimumPointThreshold)
                {
                    incidentParms.points = MinimumPointThreshold + 100f;
                }
                incidentParms.faction = Find.FactionManager.RandomEnemyFaction();
                PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, true);
                defaultPawnGroupMakerParms.generateFightersOnly = true;
                List<Pawn> pawnList = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, true).ToList<Pawn>();
                Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, pawnList, false);
                if (pawnList.Any<Pawn>())
                {
                    LordMaker.MakeNewLord(incidentParms.faction, new LordJob_AssaultColony(incidentParms.faction, true, true, false, false, true), map, pawnList);
                }
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                GlobalTargetInfo lookTarget = (!pawnList.Any<Pawn>()) ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(pawnList[0].Position, map, false);
                Find.LetterStack.ReceiveLetter("LetterLabelTreasureAmbushLGE".Translate(), "LetterTreasureAmbushLGE".Translate(GenThing.GetMarketValue(rewardList).ToStringMoney(null), GenLabel.ThingsLabel(rewardList, string.Empty)), LetterDefOf.ThreatBig, lookTarget, null);
            }, "GeneratingMapForNewEncounter", false, null);
        }

        protected void TreasureWithManhunters(Caravan caravan)
        {
            List<Thing> rewardList = GenerateReward();
            for (int i = 0; i < rewardList.Count; i++)
            {
                caravan.AddPawnOrItem(rewardList[i], true);
            }
            LongEventHandler.QueueLongEvent(delegate
            {
                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, caravan);
                //make sure a minimum point threshold is hit
                if (incidentParms.points < MinimumPointThreshold)
                {
                    incidentParms.points = MinimumPointThreshold + 100f;
                }
                PawnKindDef animalKind;
                if (!ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(this.AdjustedPoints(incidentParms.points), incidentParms.target.Tile, out animalKind) && !ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(this.AdjustedPoints(incidentParms.points), -1, out animalKind))
                {
                    Log.Warning("Could not find any valid animal kind for " + this.def + " incident. Going with Wargs", false);
                    animalKind = ThingDefOfVanilla.Warg;
                }
                List<Pawn> pawnList = ManhunterPackIncidentUtility.GenerateAnimals(animalKind, incidentParms.target.Tile, this.AdjustedPoints(incidentParms.points));
                Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, pawnList, false);
                for (int i = 0; i < pawnList.Count; i++)
                {
                    pawnList[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, null, false);
                }
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                GlobalTargetInfo lookTarget = (!pawnList.Any<Pawn>()) ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(pawnList[0].Position, map, false);
                Find.LetterStack.ReceiveLetter("LetterLabelTreasureAmbushLGE".Translate(), "LetterTreasureAmbushLGE".Translate(GenThing.GetMarketValue(rewardList).ToStringMoney(null), GenLabel.ThingsLabel(rewardList, string.Empty)), LetterDefOf.ThreatBig, lookTarget, null);
            }, "GeneratingMapForNewEncounter", false, null);
        }

        private float AdjustedPoints(float basePoints)
        {
            return basePoints * ManhunterAmbushPointsFactor;
        }

        protected List<Thing> GenerateReward()
        {
            return RewardGeneratorUtilityLGE.GenerateInterceptedMessageReward();
        }

        [DebuggerHidden]
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            //Old Stuff that gets removed the apparently produced errors
            /*
            yield return new FloatMenuOption("VisitPeaceTalks".Translate(new object[]
            {
                this.Label
            }), delegate
            {
                caravan.pather.StartPath(this.Tile, new CaravanArrivalAction_VisitDoctorRequest(this), true);
            }, MenuOptionPriority.Default, null, null, 0f, null, this);
            if (Prefs.DevMode)
            {
                yield return new FloatMenuOption("VisitPeaceTalks".Translate(new object[]
                {
                    this.Label
                }) + " (Dev: instantly)", delegate
                {
                    caravan.Tile = this.Tile;
                    caravan.pather.StopDead();
                    new CaravanArrivalAction_VisitDoctorRequest(this).Arrived(caravan);
                }, MenuOptionPriority.Default, null, null, 0f, null, this);
            }
            */

            foreach (FloatMenuOption o in base.GetFloatMenuOptions(caravan))
            {
                yield return o;
            }
            foreach (FloatMenuOption f in CaravanArrivalAction_VisitInterceptedMessageLGE.GetFloatMenuOptions(caravan, this))
            {
                yield return f;
            }
            yield break;
        }

    }

    public class CaravanArrivalAction_VisitInterceptedMessageLGE : CaravanArrivalAction
    {
        private WorldObject_InterceptedMessageLGE interceptedMessage;

        //I can leave Peacetalks here because it just says Visit {0} and I can piggyback on already existing translations this way
        public override string Label => "VisitPeaceTalks".Translate(interceptedMessage.Label);

        public override string ReportString => "CaravanVisiting".Translate(interceptedMessage.Label);

        public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, WorldObject_InterceptedMessageLGE interceptedMessage)
        {
            return interceptedMessage != null && interceptedMessage.Spawned;
        }

        public CaravanArrivalAction_VisitInterceptedMessageLGE()
        {
        }

        public CaravanArrivalAction_VisitInterceptedMessageLGE(WorldObject_InterceptedMessageLGE interceptedMessage)
        {
            this.interceptedMessage = interceptedMessage;
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
        {
            FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
            if (!(bool)floatMenuAcceptanceReport)
            {
                return floatMenuAcceptanceReport;
            }
            if (interceptedMessage != null && interceptedMessage.Tile != destinationTile)
            {
                return false;
            }
            return CanVisit(caravan, interceptedMessage);
        }

        public override void Arrived(Caravan caravan)
        {
            this.interceptedMessage.Notify_CaravanArrived(caravan);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<WorldObject_InterceptedMessageLGE>(ref this.interceptedMessage, "interceptedMessage", false);
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_InterceptedMessageLGE interceptedMessage)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions<CaravanArrivalAction_VisitInterceptedMessageLGE>(() => CanVisit(caravan, interceptedMessage), () => new CaravanArrivalAction_VisitInterceptedMessageLGE(interceptedMessage), "VisitPeaceTalks".Translate(interceptedMessage.Label), caravan, interceptedMessage.Tile, interceptedMessage);
        }
    }
}
