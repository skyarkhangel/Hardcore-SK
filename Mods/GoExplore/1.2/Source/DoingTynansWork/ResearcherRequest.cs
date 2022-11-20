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
    public class WorldObject_ResearchRequestLGE : WorldObject
    {

        private static readonly SimpleCurve BadOutcomeFactorAtResearchSpeed = new SimpleCurve
        {
            {
                new CurvePoint(0.1f, 4f),
                true
            },
            {
                new CurvePoint(1.6f, 0.85f),
                true
            },
            {
                new CurvePoint(2.1f, 0.18f),
                true
            }
        };

        private const float BaseWeight_Disaster = 0.08f;

        private const float BaseWeight_Backfire = 0.15f;

        private const float BaseWeight_TalksFlounder = 0.3f;

        private const float BaseWeight_Success = 0.50f;

        private const float BaseWeight_Triumph = 0.15f;

        private static readonly IntRange DisasterFactionRelationOffset = new IntRange(-30, -15);

        private static readonly IntRange BackfireFactionRelationOffset = new IntRange(-15, -5);

        private static readonly IntRange SuccessFactionRelationOffset = new IntRange(5, 15);

        private static readonly IntRange TriumphFactionRelationOffset = new IntRange(15, 25);

        private static readonly FloatRange SuccessResearchAmount = new FloatRange(80f, 120f);

        private static readonly FloatRange TriumphResearchAmount = new FloatRange(150f, 200f);

        private const float IntellectualXPGainAmount = 5000f;

        private static List<Pair<Action, float>> tmpPossibleOutcomes = new List<Pair<Action, float>>();

        public void Notify_CaravanArrived(Caravan caravan)
        {
            Pawn pawn = BestCaravanPawnUtility.FindPawnWithBestStat(caravan, StatDefOf.ResearchSpeed);
            if (pawn == null)
            {
                Messages.Message("MessagePeaceTalksNoDiplomat".Translate(), caravan, MessageTypeDefOf.NegativeEvent);
                return;
            }
            float badOutcomeWeightFactor = WorldObject_ResearchRequestLGE.GetBadOutcomeWeightFactor(pawn);
            float num = 1f / badOutcomeWeightFactor;
            WorldObject_ResearchRequestLGE.tmpPossibleOutcomes.Clear();
            WorldObject_ResearchRequestLGE.tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
            {
                this.Outcome_Disaster(caravan);
            }, BaseWeight_Disaster * badOutcomeWeightFactor));
            WorldObject_ResearchRequestLGE.tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
            {
                this.Outcome_Backfire(caravan);
            }, BaseWeight_Backfire * badOutcomeWeightFactor));
            WorldObject_ResearchRequestLGE.tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
            {
                this.Outcome_Neutral(caravan);
            }, BaseWeight_TalksFlounder));
            WorldObject_ResearchRequestLGE.tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
            {
                this.Outcome_Success(caravan);
            }, BaseWeight_Success * num));
            WorldObject_ResearchRequestLGE.tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
            {
                this.Outcome_Triumph(caravan);
            }, BaseWeight_Triumph * num));
            Action first = WorldObject_ResearchRequestLGE.tmpPossibleOutcomes.RandomElementByWeight((Pair<Action, float> x) => x.Second).First;
            first();
            pawn.skills.Learn(SkillDefOf.Intellectual, IntellectualXPGainAmount, true);
            Find.WorldObjects.Remove(this);
        }

        protected ResearchProjectDef ApplyPointsToResearch(float points)
        {
            ResearchManager researchManager = Find.ResearchManager;
            if (!researchManager.AnyProjectIsAvailable)
            {
                return null;
            }
            ResearchProjectDef research;
            IEnumerable<ResearchProjectDef> researchList = (from x in DefDatabase<ResearchProjectDef>.AllDefsListForReading
                                                            where x.CanStartNow
                                                            select x);

            ResearchProjectDef currentResearch = researchManager.currentProj;
            researchList.TryRandomElementByWeight((ResearchProjectDef x) => 1 / x.baseCost, out research);

            //points need to multiplied 
            points *= 121f;

            researchManager.currentProj = research;
            researchManager.ResearchPerformed(points, null);
            researchManager.currentProj = currentResearch;

            return research;
        }

        private void Outcome_Disaster(Caravan caravan)
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                int randomInRange = WorldObject_ResearchRequestLGE.DisasterFactionRelationOffset.RandomInRange;
                this.Faction.TryAffectGoodwillWith(Faction.OfPlayer, randomInRange, false, false, null, null);
                if (!this.Faction.HostileTo(Faction.OfPlayer))
                {
                    this.Faction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, false, null, null);
                }
                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, caravan);
                incidentParms.faction = this.Faction;
                PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, true);
                defaultPawnGroupMakerParms.generateFightersOnly = true;
                List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, true).ToList<Pawn>();
                Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, list, false);
                if (list.Any<Pawn>())
                {
                    LordMaker.MakeNewLord(incidentParms.faction, new LordJob_AssaultColony(this.Faction, true, true, false, false, true), map, list);
                }
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                GlobalTargetInfo lookTarget = (!list.Any<Pawn>()) ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(list[0].Position, map, false);
                Find.LetterStack.ReceiveLetter("LetterLabelResearchRequest_DisasterLGE".Translate(), this.GetLetterText("LetterResearchRequest_DisasterLGE".Translate(this.Faction.def.pawnsPlural.CapitalizeFirst(),
                    this.Faction.Name,
                    Mathf.RoundToInt(randomInRange)), caravan), LetterDefOf.ThreatBig, lookTarget, null);
            }, "GeneratingMapForNewEncounter", false, null);
        }

        private void Outcome_Backfire(Caravan caravan)
        {
            int randomInRange = WorldObject_ResearchRequestLGE.BackfireFactionRelationOffset.RandomInRange;
            base.Faction.TryAffectGoodwillWith(Faction.OfPlayer, randomInRange, false, false, null, null);
            Find.LetterStack.ReceiveLetter("LetterLabelResearchRequest_BackfireLGE".Translate(), this.GetLetterText("LetterResearchRequest_BackfireLGE".Translate(base.Faction.Name,
                Mathf.RoundToInt(randomInRange)), caravan), LetterDefOf.NegativeEvent, caravan, null);
        }

        private void Outcome_Neutral(Caravan caravan)
        {
            Find.LetterStack.ReceiveLetter("LetterLabelResearchRequest_NeutralLGE".Translate(), this.GetLetterText("LetterResearchRequest_NeutralLGE".Translate(base.Faction.Name), caravan), LetterDefOf.NeutralEvent, caravan, null);
        }

        private void Outcome_Success(Caravan caravan)
        {
            int randomInRange = WorldObject_ResearchRequestLGE.SuccessFactionRelationOffset.RandomInRange;
            base.Faction.TryAffectGoodwillWith(Faction.OfPlayer, randomInRange, false, false, null, null);
            ResearchProjectDef researchDef = ApplyPointsToResearch(SuccessResearchAmount.RandomInRange);
            string researchDefLabel = "No available research :(";
            if (researchDef != null)
            {
                researchDefLabel = researchDef.LabelCap;
            }
            Find.LetterStack.ReceiveLetter("LetterLabelResearchRequest_SuccessLGE".Translate(), this.GetLetterText("LetterResearchRequest_SuccessLGE".Translate(base.Faction.Name,
                Mathf.RoundToInt(randomInRange), researchDefLabel), caravan), LetterDefOf.PositiveEvent, caravan, null);
        }

        private void Outcome_Triumph(Caravan caravan)
        {
            int randomInRange = WorldObject_ResearchRequestLGE.TriumphFactionRelationOffset.RandomInRange;
            base.Faction.TryAffectGoodwillWith(Faction.OfPlayer, randomInRange);
            ResearchProjectDef researchDef = ApplyPointsToResearch(TriumphResearchAmount.RandomInRange);
            string researchDefLabel = "No available research :(";
            if(researchDef != null)
            {
                researchDefLabel = researchDef.LabelCap;
            }
            Find.LetterStack.ReceiveLetter("LetterLabelResearchRequest_TriumphLGE".Translate(), this.GetLetterText("LetterResearchRequest_TriumphLGE".Translate(base.Faction.Name,
                Mathf.RoundToInt(randomInRange), researchDefLabel), caravan), LetterDefOf.PositiveEvent, caravan, null);
        }

        private string GetLetterText(string baseText, Caravan caravan)
        {
            string text = baseText;
            Pawn pawn = BestCaravanPawnUtility.FindPawnWithBestStat(caravan, StatDefOf.ResearchSpeed);
            if (pawn != null)
            {
                text = text + "\n\n" + "ResearchRequestXPGainLGE".Translate(pawn.LabelShort, IntellectualXPGainAmount);
            }
            return text;
        }

        private static float GetBadOutcomeWeightFactor(Pawn diplomat)
        {
            float statValue = diplomat.GetStatValue(StatDefOf.ResearchSpeed, true);
            return WorldObject_ResearchRequestLGE.GetBadOutcomeWeightFactor(statValue);
        }

        private static float GetBadOutcomeWeightFactor(float ResearchSpeed)
        {
            return WorldObject_ResearchRequestLGE.BadOutcomeFactorAtResearchSpeed.Evaluate(ResearchSpeed);
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
            foreach (FloatMenuOption f in CaravanArrivalAction_VisitResearchRequest.GetFloatMenuOptions(caravan, this))
            {
                yield return f;
            }
            yield break;
        }

    }

    public class CaravanArrivalAction_VisitResearchRequest : CaravanArrivalAction
    {
        private WorldObject_ResearchRequestLGE researchRequest;

        //I can leave Peacetalks here because it just says Visit {0} and I can piggyback on already existing translations this way
        public override string Label => "VisitPeaceTalks".Translate(researchRequest.Label);

        public override string ReportString => "CaravanVisiting".Translate(researchRequest.Label);

        public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, WorldObject_ResearchRequestLGE researchRequest)
        {
            return researchRequest != null && researchRequest.Spawned;
        }

        public CaravanArrivalAction_VisitResearchRequest()
        {
        }

        public CaravanArrivalAction_VisitResearchRequest(WorldObject_ResearchRequestLGE researchRequest)
        {
            this.researchRequest = researchRequest;
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
        {
            FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
            if (!(bool)floatMenuAcceptanceReport)
            {
                return floatMenuAcceptanceReport;
            }
            if (researchRequest != null && researchRequest.Tile != destinationTile)
            {
                return false;
            }
            return CanVisit(caravan, researchRequest);
        }

        public override void Arrived(Caravan caravan)
        {
            this.researchRequest.Notify_CaravanArrived(caravan);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<WorldObject_ResearchRequestLGE>(ref this.researchRequest, "doctorRequest", false);
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_ResearchRequestLGE researchRequest)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions<CaravanArrivalAction_VisitResearchRequest>(() => CanVisit(caravan, researchRequest), () => new CaravanArrivalAction_VisitResearchRequest(researchRequest), "VisitPeaceTalks".Translate(researchRequest.Label), caravan, researchRequest.Tile, researchRequest);
        }
    }
}
