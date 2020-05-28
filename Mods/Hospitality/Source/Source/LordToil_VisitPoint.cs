using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Hospitality
{
    internal class LordToilData_VisitPoint : LordToilData
    {
        public float radius;
        public Dictionary<int, float> visitorMoods = new Dictionary<int, float>();
        public List<int> soldItemIDs = new List<int>(); // items that may not be bought or gifted back

        public override void ExposeData()
        {
            Scribe_Values.Look(ref radius, "radius", 45f);
            Scribe_Collections.Look(ref visitorMoods, "visitorMoods");
            Scribe_Collections.Look(ref soldItemIDs, "soldItemIDs", LookMode.Value);
        }
    }

    internal class LordToil_VisitPoint : LordToil
    {
        public LordToilData_VisitPoint Data => (LordToilData_VisitPoint) data;

        public LordToil_VisitPoint()
        {
            data = new LordToilData_VisitPoint();
        }

        public override void Init()
        {
            base.Init();
            Arrive();
        }

        public bool BoughtOrSoldByPlayer(Thing thing)
        {
            if (Data.soldItemIDs == null) return false;
            return Data.soldItemIDs.Contains(thing.thingIDNumber);
        }

        public void JoinLate(Pawn pawn)
        {
            pawn.CompGuest().lord = lord;
            if (lord.ownedPawns.Contains(pawn)) return;
            lord.AddPawn(pawn);
            if(!Data.visitorMoods.ContainsKey(pawn.thingIDNumber))
            {
                var startMood = pawn.needs.mood.CurInstantLevel;
                Data.visitorMoods.Add(pawn.thingIDNumber, startMood);
            }
            pawn.Arrive();
        }

        private void Arrive()
        {
            //Log.Message("Init State_VisitPoint "+brain.ownedPawns.Count + " - "+brain.faction.name);
            foreach (var pawn in lord.ownedPawns)
            {
                if (pawn.needs?.mood == null) Data.visitorMoods.Add(pawn.thingIDNumber, 0.5f);
                else Data.visitorMoods.Add(pawn.thingIDNumber, pawn.needs.mood.CurInstantLevel);
                //Log.Message("Added "+pawn.NameStringShort+": "+pawn.needs.mood.CurLevel);

                var tweak = 0; // -0.1f;
                var regularity = Mathf.Lerp(-0.5f, 0.25f, Mathf.InverseLerp(-100, 100, lord.faction.PlayerGoodwill));
                    // negative factions have lower expectations
                float expectations = tweak + regularity;
                Data.visitorMoods[pawn.thingIDNumber] += expectations;

                pawn.Arrive();
            }

            GuestUtility.OnLordArrived(lord);
          
            // Lessons
            LessonAutoActivator.TeachOpportunity(ConceptDef.Named("GuestBeds"), lord.ownedPawns.FirstOrDefault(), OpportunityType.Important);
            if (PlayerHasSkilledNegotiator)
            {
                LessonAutoActivator.TeachOpportunity(ConceptDef.Named("RecruitGuest"), lord.ownedPawns.FirstOrDefault(), OpportunityType.GoodToKnow);
            }
        }

        public bool PlayerHasSkilledNegotiator
        {
            get
            {
                if (Map.mapPawns.FreeColonistsSpawnedCount == 0) return false;
                return Map.mapPawns.FreeColonistsSpawned.Any(
                    p => p?.Dead == false && p.skills.AverageOfRelevantSkillsFor(DefDatabase<WorkTypeDef>.GetNamed("Warden")) >= 9);
            }
        }

        public override void Cleanup()
        {
            Leave();

            base.Cleanup();
        }

        private void Leave()
        {
            GuestUtility.OnLordLeft(lord);
            var pawns = lord.ownedPawns.ToArray(); // Copy, because recruiting changes lord
            bool hostile = lord.faction.RelationWith(Faction.OfPlayer).kind == FactionRelationKind.Hostile;

            bool sentAway = false;

            foreach (var pawn in pawns)
            {
                var compGuest = pawn.CompGuest();
                if(compGuest != null)
                {
                    if (compGuest.sentAway)
                    {
                        sentAway = true;
                    }
                    if(!hostile)
                    {
                        var score = GetVisitScore(pawn);

                        if (score > 0.99f) LeaveVerySatisfied(pawn, score);
                        else if (score > 0.65f) LeaveSatisfied(pawn, score);
                    }
                }
                pawn.Leave();
            }

            // Rescued pawns don't factor in here. If the group is only rescued pawns, we want no message
            var nonRescuedPawns = lord.ownedPawns.Where(p => p.CompGuest()?.rescued != true).ToArray();

            if (nonRescuedPawns.Any() && !hostile)
            {
                var avgScore = nonRescuedPawns.Average(GetVisitScore);

                DisplayLeaveMessage(avgScore, lord.faction, lord.ownedPawns.Count, lord.Map, sentAway);
            }
            else
            {
                DisplayNoMessage(lord.faction, lord.Map);
            }
        }

        private static void DisplayNoMessage(Faction faction, Map currentMap)
        {
            // Not affecting goodwill, no revisit, no message
            // There is a goodwill penalty somewhere else

            // Don't come again soon
            PlanRevisit(faction, -100, currentMap, true);
        }

        public static void DisplayLeaveMessage(float score, Faction faction, int visitorCount, Map currentMap, bool sentAway)
        {
            var targetGoodwill = AffectGoodwill(score, faction, visitorCount);

            var days = PlanRevisit(faction, targetGoodwill, currentMap, sentAway);

            string messageReturn = " ";
            if (days < 7)
                messageReturn += "VisitorsReturnSoon".Translate();
            else if (days < 14)
                messageReturn += "VisitorsReturnWhile".Translate();
            else if (days < 40)
                messageReturn += "VisitorsReturnNotSoon".Translate();
            else
                messageReturn += "VisitorsReturnNot".Translate();

            if(sentAway)
                Messages.Message("VisitorsSentAway".Translate(faction.Name, targetGoodwill) + messageReturn, MessageTypeDefOf.NeutralEvent);
            else if (targetGoodwill >= 90)
                Messages.Message("VisitorsLeavingGreat".Translate(faction.Name, targetGoodwill) + messageReturn, MessageTypeDefOf.PositiveEvent);
            else if (targetGoodwill >= 50)
                Messages.Message("VisitorsLeavingGood".Translate(faction.Name, targetGoodwill) + messageReturn, MessageTypeDefOf.PositiveEvent);
            else if (targetGoodwill <= -25)
                Messages.Message("VisitorsLeavingAwful".Translate(faction.Name, targetGoodwill) + messageReturn, MessageTypeDefOf.NegativeEvent);
            else if (targetGoodwill <= 5)
                Messages.Message("VisitorsLeavingBad".Translate(faction.Name, targetGoodwill) + messageReturn, MessageTypeDefOf.NegativeEvent);
            else
                Messages.Message("VisitorsLeavingNormal".Translate(faction.Name, targetGoodwill) + messageReturn, MessageTypeDefOf.NeutralEvent);
        }

        private static int AffectGoodwill(float score, Faction faction, int visitorCount)
        {
            int targetGoodwill = Mathf.RoundToInt(Mathf.Lerp(-100, 100, score));
            float goodwillChangeMax = Mathf.Lerp(3, 24, Mathf.InverseLerp(1, 8, visitorCount));
            float currentGoodwill = faction.GoodwillWith(Faction.OfPlayer);
            float offset = targetGoodwill - currentGoodwill;
            int goodwillChange = Mathf.RoundToInt(Mathf.Clamp(offset, -goodwillChangeMax, goodwillChangeMax));

            faction.TryAffectGoodwillWith(Faction.OfPlayer, goodwillChange, false);
            return targetGoodwill;
        }

        private static float PlanRevisit(Faction faction, float targetGoodwill, Map currentMap, bool sentAway)
        {
            float days;
            if (faction.defeated) return 100;
            else if (targetGoodwill > 0)
                days = Mathf.Lerp(Rand.Range(6f, 12f), Rand.Range(3f, 6f), targetGoodwill/100f);
            else
                days = Mathf.Lerp(Rand.Range(12f, 24f), Rand.Range(25f, 30f), targetGoodwill/-100f);

            if (targetGoodwill < -25) days += (-targetGoodwill - 25);
            if (sentAway) days += 5;

            Map randomVisitMap = Rand.Value < 0.1f ? Find.Maps.Where(m => m.IsPlayerHome).RandomElement() : currentMap;

            if (Rand.Value < targetGoodwill / 100f && Rand.Value < 0.2f)
            {
                // Send another friendly faction as well (start walking now)
                if (Find.FactionManager.AllFactionsVisible.Where(f => f != faction && !f.defeated && !f.HostileTo(Faction.OfPlayer) && !f.IsPlayer).TryRandomElement(out var newFaction))
                {
                    GenericUtility.TryCreateVisit(randomVisitMap, 0, newFaction);
                }
            }

            //Log.Message(faction.def.LabelCap + " will visit again in " + days + " days (+" + GenericUtility.GetTravelDays(faction, randomVisitMap)*2 + " days for travel).");
            GenericUtility.TryCreateVisit(randomVisitMap, days, faction, 2);
            return days;
        }

        public float GetVisitScore(Pawn pawn)
        {
            if (pawn.needs?.mood == null) return 0;

            const float defaultMood = 0.5f;
            if (!Data.visitorMoods.TryGetValue(pawn.thingIDNumber, out var initialMood)) initialMood = defaultMood;

            var increase = pawn.needs.mood.CurLevel - initialMood;
            var score = Mathf.Lerp(increase * 2.75f, pawn.needs.mood.CurLevel * 1.35f, 0.5f);
            //Log.Message(pawn.NameStringShort + " increase: " + (increase * 2.75f) + " mood: " + (pawn.needs.mood.CurLevel * 1.35f) + " score: " + score);
            return score;
        }

        private List<Thing> GetLoot(Pawn pawn, float desiredValue)
        {
            var totalValue = 0f;
            var items = pawn.inventory.innerContainer.Where(i => WillDrop(pawn, i)).InRandomOrder().ToList();
            var dropped = new List<Thing>();
            while (totalValue < desiredValue && items.Count > 0)
            {
                var item = items.First();
                items.Remove(item);
                if (totalValue + item.MarketValue > desiredValue) continue;
                Map map = pawn.MapHeld;
                if (pawn.inventory.innerContainer.TryDrop(item, pawn.Position, map, ThingPlaceMode.Near, out item))
                {
                    dropped.Add(item);
                    totalValue += item.MarketValue;
                }

                // Handle trade stuff
                if (item is ThingWithComps twc && map.mapPawns.FreeColonistsSpawnedCount > 0) twc.PreTraded(TradeAction.PlayerBuys, map.mapPawns.FreeColonistsSpawned.RandomElement(), pawn);
            }
            return dropped;
        }

        private void LeaveVerySatisfied(Pawn pawn, float score)
        {
            if (pawn.inventory.innerContainer.Count == 0 || Settings.disableGifts) return;

            var dropped = GetLoot(pawn, (score + 10)*1.5f);
            if (dropped.Count == 0) return;
            var itemNames = dropped.Select(GetItemName).ToCommaList(true);
            
            var text = "VisitorVerySatisfied".Translate(pawn.Name.ToStringShort, pawn.Possessive(), pawn.ProSubjCap(), itemNames);
            Messages.Message(text, dropped.First(), MessageTypeDefOf.PositiveEvent);

            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDef.Named("Hospitality"), KnowledgeAmount.Total);
        }

        private void LeaveSatisfied(Pawn pawn, float score)
        {
            if (pawn.inventory.innerContainer.Count == 0 || Settings.disableGifts) return;

            var desiredValue = (score + 10)*2;
            var things = pawn.inventory.innerContainer.Where(i => WillDrop(pawn, i) && i.MarketValue < desiredValue).ToArray();
            if (!things.Any()) return;

            var item = things.MaxBy(i => i.MarketValue); // MaxBy throws exception when list is empty!!!
            if (item == null) return;

            pawn.inventory.innerContainer.TryDrop(item, pawn.Position, pawn.MapHeld, ThingPlaceMode.Near, out item);

            var text = "VisitorSatisfied".Translate(pawn.Name.ToStringShort, pawn.Possessive(), pawn.ProSubjCap(), GetItemName(item));
            Messages.Message(text, item, MessageTypeDefOf.PositiveEvent);
        }

        private bool WillDrop(Pawn pawn, Thing i)
        {
            // To prevent dropping ammo from CE or similar
            var ammoCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail("Ammo");
            if (ammoCategory != null && i.def.IsWithinCategory(ammoCategory)) return false;
            if (EquipmentUtility.IsBiocoded(i)) return false;
            if (i.TryGetComp<CompBladelinkWeapon>()?.bondedPawn != null) return false;

            return i.def != ThingDefOf.Silver && !i.IsMeal() && !pawn.Bought(i) && !BoughtOrSoldByPlayer(i) && !pawn.inventory.NotForSale(i);
        }

        private static string GetItemName(Thing item)
        {
            return Find.ActiveLanguageWorker.WithIndefiniteArticlePostProcessed(item.Label);
        }

        public override void UpdateAllDuties()
        {
            foreach (Pawn pawn in lord.ownedPawns)
            {
                GuestUtility.AddNeedJoy(pawn);
                GuestUtility.AddNeedComfort(pawn);
                pawn.mindState.duty = new PawnDuty(GuestUtility.relaxDef, pawn.Position, Data.radius);
            }
        }

        public void OnPlayerBoughtItem(Thing thing)
        {
            if(Data.soldItemIDs == null) Data.soldItemIDs = new List<int>();
            Data.soldItemIDs.Add(thing.thingIDNumber);
        }

        public void OnPlayerSoldItem(Thing thing)
        {
            OnPlayerBoughtItem(thing); // Same thing
        }
    }
}
