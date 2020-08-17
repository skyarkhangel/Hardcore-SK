using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Hospitality
{
    // Note that this implementation is VERY different from vanilla
    public class IncidentWorker_VisitorGroup : IncidentWorker_NeutralGroup
    {
        private static ThingDef[] _items;

        // Taken from core
        private static readonly SimpleCurve pointsCurve = new SimpleCurve
        {
            new CurvePoint(45f, 0f), new CurvePoint(50f, 1f), new CurvePoint(100f, 1f), new CurvePoint(200f, 0.25f), new CurvePoint(300f, 0.1f), new CurvePoint(500f, 0f)
        };

        public static float MaxPleaseAmount(float current)
        {
            // if current standing is 100, 10 can be gained
            // if current standing is -100, 50 can be gained
            // then clamped.
            return Mathf.Clamp(current + 30 - Offset(current), -100, 100);
        }

        public static float MaxAngerAmount(float current)
        {
            return Mathf.Clamp(current - 30, -100, 100);
        }

        private static float Offset(float current)
        {
            return Mathf.Lerp(-20, 20, Mathf.InverseLerp(-100, 100, current));
        }

        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
        {
            return !f.IsPlayer && !f.defeated && !f.def.hidden && !f.HostileTo(Faction.OfPlayer) 
                   && f.def.pawnGroupMakers != null && f.def.pawnGroupMakers.Any(x => x.kindDef == PawnGroupKindDef);
        }

        private static bool CheckCanCome(Map map, Faction faction, out TaggedString reasons)
        {
            var fallout = map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout);
            var hostileFactions = map.mapPawns.AllPawnsSpawned.Where(p => !p.Dead && !p.IsPrisoner && p.Faction != null && !p.Downed && !IsFogged(p) && !p.InContainerEnclosed).Select(p => p.Faction).Where(p =>
                                                                   p.HostileTo(Faction.OfPlayer) || p.HostileTo(faction)).ToArray();
            var winter = map.GameConditionManager.ConditionIsActive(GameConditionDefOf.VolcanicWinter);
            var temp = faction.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp) && faction.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.SeasonalTemp);
            var beds = map.listerBuildings.AllBuildingsColonistOfClass<Building_GuestBed>().Any();

            reasons = null;

            if (temp && !fallout && !winter && !hostileFactions.Any() && beds) return true; // All clear, don't ask

            var reasonList = new List<string>(); // string, so we can check for Distinct later
            if (!beds) reasonList.Add("- " + "VisitorsArrivedReasonNoBeds".Translate());
            if (fallout) reasonList.Add("- " + GameConditionDefOf.ToxicFallout.LabelCap);
            if (winter) reasonList.Add("- " + GameConditionDefOf.VolcanicWinter.LabelCap);
            if (!temp) reasonList.Add("- " + "Temperature".Translate());

            foreach (var f in hostileFactions)
            {
                reasonList.Add("- " + f.def.pawnsPlural.CapitalizeFirst());
            }

            reasons = reasonList.Distinct().Aggregate((a, b) => a+"\n"+b);
            return false; // Do ask
    
        }

        private static bool IsFogged(Pawn pawn)
        {
            return pawn.MapHeld.fogGrid.IsFogged(pawn.PositionHeld);
        }

        private static void ShowAskMayComeDialog(Faction faction, Map map, TaggedString reasons, Direction8Way spawnDirection, Action allow, Action refuse)
        {
            var text = "VisitorsArrivedDesc".Translate(faction, reasons);

            DiaNode diaNode = new DiaNode(text);
            DiaOption diaOption = new DiaOption("VisitorsArrivedAccept".Translate()) {action = allow, resolveTree = true};
            diaNode.options.Add(diaOption);

            DiaOption diaOption2 = new DiaOption("VisitorsArrivedRefuse".Translate()) {action = refuse, resolveTree = true};
            diaNode.options.Add(diaOption2);

            if (!map.listerBuildings.AllBuildingsColonistOfClass<Building_GuestBed>().Any())
            {
                DiaOption diaOption3 = new DiaOption("VisitorsArrivedRefuseUntilBeds".Translate());
                diaOption3.resolveTree = true;
                diaOption3.action = () => {
                    Hospitality_MapComponent.RefuseGuestsUntilWeHaveBeds(map);
                    refuse();
                };
                diaNode.options.Add(diaOption3);
            }

            string title = "VisitorsArrivedTitle".Translate(new NamedArgument((MapParent)map.ParentHolder, "WORLDOBJECT"), spawnDirection.LabelShort());
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, title));
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!TryResolveParms(parms))
            {
                Log.ErrorOnce($"Trying to spawn visitors, but parms couldn't be resolved.", 84289426);
                return false;
            }

            // Is map not available anymore?
            if (!(parms.target is Map map))
            {
                Log.ErrorOnce("Trying to spawn visitors, but map does not exist anymore.", 43692862);
                return true;
            }

            if (parms.points < 40)
            {
                Log.Error("Trying to spawn visitors, but points are too low.");
                return false;
            }

            if (parms.faction == null)
            {
                Log.ErrorOnce("Trying to spawn visitors, but couldn't find valid faction.", 43638973);
                return true;
            }
            if (!parms.spawnCenter.IsValid)
            {
                Log.ErrorOnce("Trying to spawn visitors, but could not find a valid spawn point.", 94839643);
                return false;
            }

            if (parms.faction == Faction.OfPlayer)
            {
                Log.ErrorOnce("Trying to spawn visitors, but they are of Faction.OfPlayer.", 3464363);
                return true;
            }
            if (parms.faction.RelationWith(Faction.OfPlayer).kind == FactionRelationKind.Hostile)
            {
                Log.Message($"Trying to spawn visitors of faction {parms.faction.Name}, but they are hostile to the player (now).");
                return true;
            }

            if (parms.faction.defeated)
            {
                Log.Message($"Trying to spawn visitors of faction {parms.faction.Name}, but they have been defeated.");
                return true;
            }

            if (Settings.disableGuests || map.mapPawns.ColonistCount == 0 || !map.GetMapComponent().guestsAreWelcome)
            {
                if(Settings.disableGuests)
                    Log.Message($"Guest group arrived, but guests are disabled in the options.");
                else if(map.mapPawns.ColonistCount == 0)
                    Log.Message($"Guest group arrived, but there are no remaining colonists on the map.");
                else if(!map.GetMapComponent().guestsAreWelcome)
                    Log.Message($"Guest group arrived, but the player disabled guests for this map.");

                GenericUtility.PlanNewVisit(map, Rand.Range(5f, 25f), parms.faction);
            }
            else
            {
                // Did the player refuse guests until beds are made and there are no beds yet?
                if (!BedCheck(map))
                {
                    Log.Message("Guest group arrived, but there are no guest beds and the player asked to wait until they are built.");

                    GenericUtility.PlanNewVisit(map, Rand.Range(5f, 10f), parms.faction);
                    return true;
                }

                // We check here instead of CanFireNow, so we can reschedule the visit.
                // Any reasons not to come?
                if (CheckCanCome(map, parms.faction, out var reasons))
                {
                    // No, spawn
                    return SpawnGroup(parms, map);
                }

                // Yes, ask the player for permission
                void Allow() => SpawnGroup(parms, map);
                void Refuse() => GenericUtility.PlanNewVisit(map, Rand.Range(2f, 5f), parms.faction);

                AskForPermission(parms, map, reasons, Allow, Refuse);
            }
            return true;
        }

        protected virtual void AskForPermission(IncidentParms parms, Map map, string reasons, Action allow, Action refuse)
        {
            var spawnDirection = GetSpawnDirection(map, parms.spawnCenter);
            ShowAskMayComeDialog(parms.faction, map, reasons, spawnDirection,
                // Permission, spawn
                allow,
                // No permission, come again later
                refuse);
        }

        private static Direction8Way GetSpawnDirection(Map map, IntVec3 position)
        {
            var offset = map.Center - position;
            var angle = (offset.AngleFlat+180)%360;

            const float step = 360/16f;
            if(angle < 1*step) return Direction8Way.North;
            if(angle < 3*step) return Direction8Way.NorthEast;
            if(angle < 5*step) return Direction8Way.East;
            if(angle < 7*step) return Direction8Way.SouthEast;
            if(angle < 9*step) return Direction8Way.South;
            if(angle < 11*step) return Direction8Way.SouthWest;
            if(angle < 13*step) return Direction8Way.West;
            if(angle < 15*step) return Direction8Way.NorthWest;
            return Direction8Way.North;
        }

        private bool SpawnGroup(IncidentParms parms, Map map)
        {
            var visitors = new List<Pawn>();
            try
            {
                //Log.Message(string.Format("Spawning visitors from {0}, at {1}.", parms.faction, parms.spawnCenter));
                SpawnPawns(parms, visitors);

                SpawnGroupUtility.CheckVisitorsValid(visitors);

                if (visitors == null || visitors.Count == 0) return false;

                var area = visitors.First().GetGuestArea() ?? map.GetMapComponent().defaultAreaRestriction;
                var spot = GetSpot(map, area, visitors.First());

                if (!spot.IsValid)
                {
                    throw new Exception($"Visitors from {parms.faction.Name} failed to find a valid travel target from {visitors.First()?.Position} to guest area {area?.Label}.");
                }

                GiveItems(visitors);

                var stayDuration = (int)(Rand.Range(1f, 2.4f) * GenDate.TicksPerDay);
                CreateLord(parms.faction, spot, visitors, map, true, true, stayDuration);
            }
            catch (Exception e)
            {
                var faction = parms.faction?.Name;
                var factionType = parms.faction?.def.label;
                Log.Error($"Hospitality: Something failed when setting up visitors from faction {faction} ({factionType}):\n{e}");
                foreach (var visitor in visitors)
                {
                    if (visitor?.Spawned == true)
                    {
                        visitor.DeSpawn();
                        visitor.DestroyOrPassToWorld();
                    }
                }
                GenericUtility.PlanNewVisit(map, Rand.Range(1f, 3f), parms.faction);
            }
            return true; // be gone, event
        }

        protected override void ResolveParmsPoints(IncidentParms parms)
        {
            if (parms.points < 0f)
            {
                parms.points = Rand.ByCurve(pointsCurve);
            }
        }

        protected void SpawnPawns(IncidentParms parms,  List<Pawn> spawned)
        {
            var map = (Map)parms.target;

            var selection = GetPawnsToSpawn(parms).Distinct();

            foreach (var pawn in selection)
            {
                try
                {
                    var visitor = SpawnGroupUtility.SpawnVisitor(spawned, pawn, map, parms.spawnCenter);
                    if (visitor.needs?.rest != null) visitor.needs.rest.CurLevel = Rand.Range(0.1f, 0.7f);
                }
                catch (Exception e)
                {
                    Log.Error($"Hospitality: Failed to spawn pawn {pawn?.Label}:\n{e}");
                    if (pawn.Spawned)
                    {
                        pawn.DeSpawn();
                        pawn.DestroyOrPassToWorld();
                    }
                }
            }
        }

        protected IEnumerable<Pawn> GetPawnsToSpawn(IncidentParms parms)
        {
            var totalAmount = GetGroupSize();
            int knownPawnAmount = Mathf.RoundToInt(totalAmount * ChanceToKnowEachPawn);
            
            var options = SpawnGroupUtility.GetKnownPawns(parms).RandomlyUsingTitleAsChance().InRandomOrder().Take(knownPawnAmount).ToList();

            if (options.Count < totalAmount)
            {
                try
                {
                    // Create some new people
                    int missing = totalAmount - options.Count;
                    var newPawns = GenerateNewPawns(parms, missing);
                    options.AddRange(newPawns);
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to create new pawns for faction {parms.faction?.Name}.\n{e}");
                }
            }
            return options;
        }

        protected virtual IEnumerable<Pawn> GenerateNewPawns(IncidentParms parms, int preferredAmount)
        {
            int i = 0;
            while(i < preferredAmount)
            {
                var newPawns = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDef, parms, true), false).ToArray();
                Log.Message($"Created {newPawns.Length} new pawns for {parms.faction.Name}.");
                foreach (var pawn in newPawns)
                {
                    Find.World.worldPawns.PassToWorld(pawn);
                }

                foreach (var pawn in newPawns.RandomlyUsingTitleAsChance())
                {
                    yield return pawn;
                    i++;
                    if (i >= preferredAmount) yield break;
                }

                // To avoid infinite loop
                if (!newPawns.Any()) i++;
            }
        }

        protected virtual int GetGroupSize()
        {
            //Log.Message($"Optimal amount of guests = {OptimalAmount}, max = {OptimalAmount * 16f/6}");
            var random = Rand.GaussianAsymmetric(OptimalAmount, 1.5f, 16f / 6);
            return Mathf.Clamp(Mathf.CeilToInt(random), Settings.minGuestGroupSize, Settings.maxGuestGroupSize);
        }

        /// <summary>
        /// From year 1-6, increase for 0 to 6 as the optimal amount
        /// </summary>
        private static float OptimalAmount => 1 + Mathf.Clamp(GenDate.YearsPassedFloat, 0f, 5f);
        protected virtual float ChanceToKnowEachPawn => 0.75f;

        private static IntVec3 GetSpot(Map map, Area guestArea, Pawn pawn)
        {
            if(map == null) throw new NullReferenceException("map is null!");
            if(map.reachability == null) throw new NullReferenceException("map.reachability is null!");
            // guestArea = null? Everywhere!
            if(guestArea != null && guestArea.Map != map) throw new ArgumentException($"The map of the guest area of {pawn.LabelShort} does not match the current map!");

            var cells = new List<IntVec3>();
            GetSpotAddGuestArea(map, guestArea, cells);

            GetSpotAddDropSpots(map, cells);

            return CheckCanReach(pawn, cells);
        }

        private static IntVec3 CheckCanReach(Pawn pawn, IEnumerable<IntVec3> cells)
        {
            var map = pawn.Map;
            var unroofedOption = IntVec3.Invalid;

            // Prefer roofed
            foreach (var cell in cells.InRandomOrder())
            {
                if (cell.IsValid && pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly))
                {
                    if(cell.Roofed(map)) return cell;
                    if(!unroofedOption.IsValid) unroofedOption = cell;
                }
            }

            // Otherwise not roofed
            return unroofedOption;
        }

        private static void GetSpotAddDropSpots(Map map, List<IntVec3> cells)
        {
            var tradeDropSpot = DropCellFinder.TradeDropSpot(map);
            cells.Add(tradeDropSpot);

            if (tradeDropSpot.IsValid && DropCellFinder.TryFindDropSpotNear(tradeDropSpot, map, out var near, false, false)) cells.Add(near);
            cells.Add(DropCellFinder.RandomDropSpot(map));
        }

        private static void GetSpotAddGuestArea(Map map, Area guestArea, List<IntVec3> cells)
        {
            if(map.areaManager == null) throw new NullReferenceException("map.areaManager is null!");

            if (guestArea?.ActiveCells.Any() != true) guestArea = map.GetMapComponent().defaultAreaRestriction;
            if (guestArea?.ActiveCells.Any() != true) guestArea = map.areaManager.Home;

            cells.AddRange(guestArea.ActiveCells);
        }

        private static void GiveItems(IEnumerable<Pawn> visitors)
        {
            foreach (var visitor in visitors)
            {
                PawnInventoryGenerator.GiveRandomFood(visitor);
                if(Rand.Value < 0.5f) visitor.TryGiveBackpack();


                float totalValue = 0;

                // Money
                //Log.Message("Goodwill: "+visitor.Faction.ColonyGoodwill);
                var wealthBase = visitor.Faction.PlayerGoodwill;
                var title = visitor.royalty?.MostSeniorTitle;
                if (title != null) wealthBase += title.def.seniority/2;
                var amountS = Mathf.Max(0, Mathf.RoundToInt(Rand.Gaussian(wealthBase, wealthBase)*2))+Rand.Range(0, 40);
                if (amountS >= Rand.Range(10, 25))
                {
                    var money = SpawnGroupUtility.CreateRandomItem(visitor, ThingDefOf.Silver);
                    money.stackCount = amountS;

                    var spaceFor = visitor.GetInventorySpaceFor(money);
                    if (spaceFor > 0)
                    {
                        money.stackCount = Mathf.Min(spaceFor, amountS);
                        var success = visitor.inventory.innerContainer.TryAdd(money);
                        if (success) totalValue += money.MarketValue*money.stackCount;
                        else if(!money.Destroyed) money.Destroy();
                    }
                }
                
                // Items
                float maxValue = (wealthBase + 25)*Rand.Range(5, 8);
                if (maxValue - totalValue <= 100) maxValue = totalValue + Rand.Range(25, 60); // At least bring some junk
                float value = maxValue - totalValue;
                int curCount = 0;
                while (value > 100 && curCount < 200)
                {
                    //Log.Message("Total is now " + totalValue + ", space left is " + space);
                    curCount++;

                    bool apparel = Rand.Value < 0.5f;
                    ThingDef thingDef;
                    do thingDef = SpawnGroupUtility.GetRandomItem(visitor.Faction.def.techLevel, ref _items); 
                    while (thingDef != null && apparel && thingDef.IsApparel);
                    if (thingDef == null) break;

                    //var amount = Mathf.Min(Mathf.RoundToInt(Mathf.Abs(Rand.Gaussian(1, thingDef.stackLimit/2f))),
                    //    thingDef.stackLimit);
                    //if (amount <= 0) continue;

                    var item = SpawnGroupUtility.CreateRandomItem(visitor, thingDef);

                    //Log.Message(item.Label + " has this value: " + item.MarketValue);
                    if (item.Destroyed) continue;

                    if (item.MarketValue >= value)
                    {
                        item.Destroy();
                        continue;
                    }
                    
                    if (item.MarketValue < 1)
                    {
                        item.Destroy();
                        continue;
                    }
                    var uniquesAmount = item.TryGetComp<CompQuality>() != null ? 1 : item.def.stackLimit;
                    var maxItems = Mathf.Min(Mathf.FloorToInt(value/item.MarketValue), item.def.stackLimit, uniquesAmount);
                    var minItems = Mathf.Max(1, Mathf.CeilToInt(Rand.Range(50,100)/item.MarketValue));

                    if (maxItems < 1 || minItems > maxItems)
                    {
                        item.Destroy();
                        continue;
                    }

                    //Log.Message("Can fit " + maxItems+" of "+item.Label);
                    item.stackCount = Rand.RangeInclusive(minItems, maxItems);
                    //Log.Message("Added " + item.stackCount + " with a value of " + (item.MarketValue * item.stackCount));

                    var spaceFor = visitor.GetInventorySpaceFor(item);
                    if (spaceFor > 0)
                    {
                        item.stackCount = Mathf.Min(spaceFor, item.stackCount);
                        var success = visitor.inventory.innerContainer.TryAdd(item);
                        if (success) totalValue += item.MarketValue*item.stackCount;
                        else if(!item.Destroyed) item.Destroy();
                    }
                    value = maxValue - totalValue;
                }
            }
        }

        public static void CreateLord(Faction faction, IntVec3 chillSpot, List<Pawn> pawns, Map map, bool showLetter, bool getUpsetWhenLost, int duration)
        {
            // Make sure existing lords don't hold any of the pawns
            RemovePawnsFromMapLords(pawns, map);

            var mapComp = map.GetMapComponent();

            var lordJob = new LordJob_VisitColony(faction, chillSpot, duration, getUpsetWhenLost);
            var lord = LordMaker.MakeNewLord(faction, lordJob, map, pawns);


            // Set default interaction
            pawns.ForEach(delegate(Pawn p) {
                var compGuest = p.CompGuest();
                if (compGuest != null)
                {
                    compGuest.ResetForGuest(lord);
                    compGuest.entertain = mapComp.defaultEntertain;
                    compGuest.makeFriends = mapComp.defaultMakeFriends;
                    compGuest.GuestArea = mapComp.defaultAreaRestriction;
                    compGuest.ShoppingArea = mapComp.defaultAreaShopping;
                }
            });

            foreach (var pawn in pawns)
            {
                pawn.ConvertToTrader(true);
            }

            Pawn leader = pawns.Find(x => faction.leader == x);
            TaggedString label;
            TaggedString description;
            if (pawns.Count == 1)
            {
                var value2 = leader == null ? TaggedString.Empty : "\n\n" + "SingleVisitorArrivesLeaderInfo".Translate(pawns[0].Named("PAWN")).AdjustedFor(pawns[0]);
                label = "LetterLabelSingleVisitorArrives".Translate();
                description = "SingleVisitorArrives".Translate(pawns[0].story.Title, faction.Name, pawns[0].Name.ToStringFull, string.Empty, value2, pawns[0].Named("PAWN")).AdjustedFor(pawns[0]);
            }
            else
            {
                var value4 = leader == null ? TaggedString.Empty : "\n\n" + "GroupVisitorsArriveLeaderInfo".Translate(leader.LabelShort, leader);
                label = "LetterLabelGroupVisitorsArrive".Translate();
                description = "GroupVisitorsArrive".Translate(faction.Name, string.Empty, value4);
            }

            if (showLetter)
            {
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref label, ref description, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), true);
                Find.LetterStack.ReceiveLetter(label, description, LetterDefOf.PositiveEvent, pawns[0], faction);
            }
        }

        private static void RemovePawnsFromMapLords(List<Pawn> pawns, Map map)
        {
            foreach (var pawn in pawns)
            {
                map.lordManager.lords.ForEach(lord => {
                    if (lord.ownedPawns.Contains(pawn))
                    {
                        Log.Warning($"Existing lord still contains pawn {pawn.LabelShort}. Removing.");
                        lord.Notify_PawnLost(pawn, PawnLostCondition.Undefined);
                    }
                });
            }
        }

        public static bool BedCheck(Map map)
        {
            if (map == null) return false;
            var mapComp = map.GetMapComponent();

            if (!mapComp.refuseGuestsUntilWeHaveBeds) return true;
            if (!map.listerBuildings.AllBuildingsColonistOfClass<Building_GuestBed>().Any()) return false;

            // We have beds now!
            mapComp.refuseGuestsUntilWeHaveBeds = false;
            return true;
        }
    }
}
