using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace LetsGoExplore
{
    public class IncidentWorker_LostCityLGE : IncidentWorker
    {
        private static readonly IntRange TimeoutDaysRange = new IntRange(8, 14);

        private static readonly int minDist = 7;
        private static readonly int maxDist = 11;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            int tile;
            return base.CanFireNowSub(parms) && TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1);
        }

        public static Site CreateBombardmentCitySite(int tile, int days)
        {
            List<SitePartDef> siteParts = new List<SitePartDef>();
            siteParts.Add(SiteDefOf.StandartLostCityLGE);
            siteParts.Add(SiteDefOf.OrbitalBombardmentLGE);
            Site site = SiteMaker.MakeSite(siteParts, tile, Faction.OfAncients);
            site.GetComponent<TimeoutComp>().StartTimeout(days * 60000);
            Find.WorldObjects.Add(site);
            return site;
        }

        public static Site CreateInfestedCitySite(int tile, int days)
        {
            Site site = SiteMaker.MakeSite(SiteDefOf.InfestedLostCityLGE, tile: tile, faction: Faction.OfAncients);
            site.GetComponent<TimeoutComp>().StartTimeout(days * 60000);
            Find.WorldObjects.Add(site);
            return site;
        }

        public static Site CreateToxicCitySite(int tile, int days)
        {
            Site site = SiteMaker.MakeSite(SiteDefOf.ToxicLostCityLGE, tile: tile, faction: Faction.OfAncients);
            site.GetComponent<TimeoutComp>().StartTimeout(days * 60000);
            Find.WorldObjects.Add(site);
            return site;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            int tile;
            if (!TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1))
            {
                return false;
            }
            if (Rand.Chance(0.33f))
            {
                Site site = CreateBombardmentCitySite(tile, TimeoutDaysRange.RandomInRange);
                Find.LetterStack.ReceiveLetter("LetterLabelStandartLostCityLGE".Translate(), "LetterStandartLostCityLGE".Translate(), LetterDefOf.PositiveEvent, site, null);
            }
            else
            {
                if (Rand.Chance(0.5f))
                {
                    Site site = CreateInfestedCitySite(tile, TimeoutDaysRange.RandomInRange);
                    Find.LetterStack.ReceiveLetter("LetterLabelInfestedLostCityLGE".Translate(), "LetterInfestedLostCityLGE".Translate(), LetterDefOf.PositiveEvent, site, null);
                }
                else
                {
                    Site site = CreateToxicCitySite(tile, TimeoutDaysRange.RandomInRange);
                    Find.LetterStack.ReceiveLetter("LetterLabelToxicLostCityLGE".Translate(), "LetterToxicLostCityLGE".Translate(), LetterDefOf.PositiveEvent, site, null);
                }
            }
            return true;
        }
    }

    public class IncidentWorker_AmbrosiaAnimalsLGE : IncidentWorker
    {
        private static readonly IntRange TimeoutDaysRange = new IntRange(8, 12);
        private static readonly int minDist = 3;
        private static readonly int maxDist = 7;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            int maxTries = 20;
            bool acceptableTemp = false;
            int tile;

            for(int i = 0; i < maxTries; i++)
            {
                TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1);
                if(Find.WorldGrid[tile].temperature > 0f && Find.WorldGrid[tile].hilliness != Hilliness.Mountainous)
                {
                    acceptableTemp = true;
                    break;
                }
            }

            return base.CanFireNowSub(parms) && acceptableTemp;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            int contingency = 0;
            bool tileFound = false;
            int tile = -1;
            while(contingency < 99 && !tileFound)
            {
                TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1);
                if (Find.WorldGrid[tile].temperature > 0f && Find.WorldGrid[tile].hilliness != Hilliness.Mountainous)
                {
                    tileFound = true;
                }
                contingency++;
            }
            if (!tileFound || tile == -1)
            {
                return false;
            }
            Site site = SiteMaker.MakeSite(SiteDefOf.AmbrosiaAnimalsLGE, tile: tile, faction: Faction.OfAncients);
            site.GetComponent<TimeoutComp>().StartTimeout(TimeoutDaysRange.RandomInRange * 60000);
            Find.WorldObjects.Add(site);
            Find.LetterStack.ReceiveLetter("LetterLabelAmbrosiaAnimalsLGE".Translate(), "LetterAmbrosiaAnimalsLGE".Translate(), LetterDefOf.PositiveEvent, site, null);
            return true;
        }
    }

    public class IncidentWorker_ShipCoreStartupLGE : IncidentWorker
    {
        private static readonly IntRange TimeoutDaysRange = new IntRange(9, 14);

        private static readonly int minDist = 8;
        private static readonly int maxDist = 14;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            int tile;
            return base.CanFireNowSub(parms) && TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1) && CommsConsoleUtility.PlayerHasPoweredCommsConsole();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            int tile;
            if (!TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1))
            {
                return false;
            }
            Site site = (Site)WorldObjectMaker.MakeWorldObject(SiteDefOf.ShipCoreStartupSiteLGE);
            site.Tile = tile;
            site.AddPart(new SitePart(site, SiteDefOf.ShipCoreStartupLGE, SiteDefOf.ShipCoreStartupLGE.Worker.GenerateDefaultParams(StorytellerUtility.DefaultSiteThreatPointsNow(), tile, Faction.OfAncientsHostile)));
            site.SetFaction(Faction.OfAncientsHostile);
            site.GetComponent<TimeoutComp>().StartTimeout(TimeoutDaysRange.RandomInRange * 60000);
            Find.WorldObjects.Add(site);
            Find.LetterStack.ReceiveLetter("LetterLabelShipCoreStartupLGE".Translate(), "LetterShipCoreStartupLGE".Translate(), LetterDefOf.PositiveEvent, site, null);

            return true;
        }
    }

    public class IncidentWorker_PrisonCampLGE : IncidentWorker
    {
        private static readonly IntRange TimeoutDaysRange = new IntRange(8, 12);

        private static FloatRange RewardtotalMarketValueRange = new FloatRange(900f, 2000f);

        private static readonly int minDist = 7;
        private static readonly int maxDist = 14;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            int tile;
            return base.CanFireNowSub(parms) && TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1) && TryFindFaction(out Faction faction);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            int tile;
            if (!TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1))
            {
                return false;
            }
            Faction faction;
            if(!TryFindFaction(out faction))
            {
                return false;
            }
            Site site = (Site)WorldObjectMaker.MakeWorldObject(SiteDefOf.PrisonSiteLGE);
            site.Tile = tile;
            site.AddPart(new SitePart(site, SiteDefOf.PrisonCampLGE, SiteDefOf.PrisonCampLGE.Worker.GenerateDefaultParams(StorytellerUtility.DefaultSiteThreatPointsNow(), tile, faction)));
            site.SetFaction(faction);
            ThingSetMakerParams thingMakerparms = default(ThingSetMakerParams);
            thingMakerparms.totalMarketValueRange = new FloatRange?(RewardtotalMarketValueRange);
            List<Thing> list = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(thingMakerparms);
            site.GetComponent<PrisonerRescueQuestComp>().StartQuest(faction, 18, list);

            site.GetComponent<TimeoutComp>().StartTimeout(TimeoutDaysRange.RandomInRange * 60000);
            Find.WorldObjects.Add(site);
            Find.LetterStack.ReceiveLetter("LetterLabelPrisonCampLGE".Translate(), "LetterPrisonCampLGE".Translate(faction.Name, GenThing.GetMarketValue(list).ToStringMoney(null), GenLabel.ThingsLabel(list, string.Empty)), LetterDefOf.PositiveEvent, site, null);

            return true;
        }

        protected bool TryFindFaction(out Faction faction)
        {
            return (from x in Find.FactionManager.AllFactionsVisible
                    where !x.IsPlayer && (!x.def.hidden) && (!x.defeated) && (x.def.humanlikeFaction) && !x.HostileTo(Faction.OfPlayer)
             select x).TryRandomElement(out faction);
        }
    }

    public class IncidentWorker_NewSettlementLGE : IncidentWorker
    {
        private static readonly int minDist = 12;
        private static readonly int maxDist = 40;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            int tile;
            return base.CanFireNowSub(parms) && TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            int tile;
            if (!TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1))
            {
                return false;
            }
            Faction faction = (from x in Find.World.factionManager.AllFactionsListForReading
                                where !x.def.isPlayer && !x.def.hidden
                                select x).RandomElementByWeight((Faction x) => x.def.settlementGenerationWeight);

            Settlement settlement = (Settlement) WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            settlement.SetFaction(faction);
            settlement.Tile = tile;
            settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, null);
            Find.WorldObjects.Add(settlement);
            Find.LetterStack.ReceiveLetter("LetterLabelNewSettlementLGE".Translate(), "LetterNewSettlementLGE".Translate(faction.Name), LetterDefOf.NeutralEvent, settlement, null);

            return true;
        }
    }

    public class IncidentWorker_ResearchRequestLGE : IncidentWorker
    {
        private static readonly IntRange TimeoutDaysRange = new IntRange(16, 20);

        private static readonly int minDist = 8;
        private static readonly int maxDist = 14;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            int tile;
            Faction faction;
            return base.CanFireNowSub(parms) && this.TryFindFaction(out faction) && TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Faction faction;
            if (!this.TryFindFaction(out faction))
            {
                return false;
            }
            int tile;
            if (!TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1))
            {
                return false;
            }
            WorldObject_ResearchRequestLGE researchRequest = (WorldObject_ResearchRequestLGE)WorldObjectMaker.MakeWorldObject(SiteDefOf.ResearchRequestLGE);
            researchRequest.Tile = tile;
            researchRequest.SetFaction(faction);
            researchRequest.GetComponent<TimeoutComp>().StartTimeout(TimeoutDaysRange.RandomInRange * 60000);
            Find.WorldObjects.Add(researchRequest);
            string text = string.Format(this.def.letterText.AdjustedFor(faction.leader, "PAWN"), faction.def.leaderTitle, faction.Name, TimeoutDaysRange.RandomInRange).CapitalizeFirst();
            Find.LetterStack.ReceiveLetter(this.def.letterLabel, text, this.def.letterDef, researchRequest, null);
            return true;
        }

        private bool TryFindFaction(out Faction faction)
        {
            return (from x in Find.FactionManager.AllFactions
                    where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated && !SettlementUtility.IsPlayerAttackingAnySettlementOf(x)
                    select x).TryRandomElement(out faction);
        }
    }

    public class IncidentWorker_InterceptedMessageLGE : IncidentWorker
    {
        private static readonly IntRange TimeoutDaysRange = new IntRange(16, 20);

        private static readonly int minDist = 7;
        private static readonly int maxDist = 14;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            int tile;
            return base.CanFireNowSub(parms) && TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1) && CommsConsoleUtility.PlayerHasPoweredCommsConsole();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            int tile;
            if (!TileFinder.TryFindNewSiteTile(out tile, minDist, maxDist, false, true, -1))
            {
                return false;
            }
            WorldObject_InterceptedMessageLGE interceptedMessage = (WorldObject_InterceptedMessageLGE)WorldObjectMaker.MakeWorldObject(SiteDefOf.InterceptedMessageLGE);
            interceptedMessage.Tile = tile;
            interceptedMessage.GetComponent<TimeoutComp>().StartTimeout(TimeoutDaysRange.RandomInRange * 60000);
            Find.WorldObjects.Add(interceptedMessage);
            Find.LetterStack.ReceiveLetter("LetterLabelInterceptedMessageLGE".Translate(), "LetterInterceptedMessageLGE".Translate(), LetterDefOf.NeutralEvent, interceptedMessage, null);
            return true;
        }

    }
}
