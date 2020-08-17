using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Hospitality
{
    internal static class SpawnGroupUtility
    {
        public static void CheckVisitorsValid(List<Pawn> visitors)
        {
            foreach (var visitor in visitors)
            {
                if (visitor.TryGetComp<CompGuest>() != null && !visitor.Discarded && !visitor.Dead && !visitor.NonHumanlikeOrWildMan()) continue;

                try
                {
                    var humanlike = (visitor.def.race.Humanlike ? "humanlike" : "not humanlike");
                    var modName = visitor.def.modContentPack == null ? "vanilla (?)" : visitor.def.modContentPack.Name;

                    Log.Error($"Spawned visitor without GuestComp: {visitor.def.defName} - {humanlike} - {modName}");
                }
                catch
                {
                    Log.Error($"Failed to get more information about {visitor.Label}.");
                }
                finally
                {
                    visitors.Remove(visitor);
                    visitor.Destroy();
                }
            }
        }

        public static void GenerateNewGearFor(Pawn pawn, int mapTile)
        {
            // All default, except kindDef, faction and forceAddFreeWarmLayerIfNeeded
            var request = new PawnGenerationRequest(pawn.kindDef, pawn.Faction, PawnGenerationContext.NonPlayer,
                mapTile, false, false, false, false, true, 
                false, 1, true, true, true, false, 
                false, false, false, false, 0, null, 0.7f, null, null, null);

            PawnGenerator.RedressPawn(pawn, request);
        }

        public static Thing CreateRandomItem(Pawn visitor, ThingDef thingDef)
        {
            ThingDef stuff = GenStuff.RandomStuffFor(thingDef);
            var item = ThingMaker.MakeThing(thingDef, stuff);
            item.stackCount = 1;

            CompQuality compQuality = item.TryGetComp<CompQuality>();
            compQuality?.SetQuality(QualityUtility.GenerateQualityTraderItem(), ArtGenerationContext.Outsider);
            if (item.def.Minifiable)
            {
                item = item.MakeMinified();
                if (item.GetInnerIfMinified() == null)
                {
                    Log.ErrorOnce($"Hospitality: Tried to create minified {item.def.defName}, but InnerThing ended up being null. It is from {item.def.modContentPack.Name}.", 42896749 + item.def.debugRandomId);
                    item.Destroy();
                    return null;
                }
            }
            if (item.def.useHitPoints)
            {
                // Make sure health is at least 60%. Otherwise too expensive items can become gifts.
                const float minHealthPct = 0.6f;
                var healthRange = visitor.kindDef.gearHealthRange;
                healthRange.min = minHealthPct;
                healthRange.max = Mathf.Max(minHealthPct, healthRange.max);

                var healthPct = healthRange.RandomInRange;
                if (healthPct < 1)
                {
                    int num = Mathf.RoundToInt(healthPct * item.MaxHitPoints);
                    num = Mathf.Max(1, num);
                    item.HitPoints = num;
                }
            }
            return item;
        }

        public static ThingDef GetRandomItem(TechLevel techLevel, ref ThingDef[] itemsCache)
        {
            if (itemsCache == null)
            {
                bool Qualifies(ThingDef d) => d.category == ThingCategory.Item 
                                              && d.EverStorable(true) 
                                              && d.alwaysHaulable 
                                              && d.thingClass != typeof(MinifiedThing) 
                                              && d.tradeability != Tradeability.None 
                                              && d.GetCompProperties<CompProperties_Hatcher>() == null
                                              && !d.WillRotSoon()
                                              && (d.thingSetMakerTags == null || !d.thingSetMakerTags.Contains("NotForGuests"));

                itemsCache = DefDatabase<ThingDef>.AllDefs.Where(Qualifies).ToArray();
            }

            return itemsCache.Where(thingDef => thingDef.techLevel <= Increase(techLevel)).TryRandomElementByWeight(thingDef => TechLevelDiff(thingDef.techLevel, techLevel), out var def)
                ? def
                : null;
        }

        private static bool WillRotSoon(this ThingDef d)
        {
            return d.GetCompProperties<CompProperties_Rottable>()?.daysToRotStart < 6;
        }

        private static TechLevel Increase(TechLevel techLevel)
        {
            return techLevel+1;
        }

        private static float TechLevelDiff(TechLevel def, TechLevel target)
        {
            return (float) TechLevel.Ultra - Mathf.Abs((float) target - (float) def);
        }

        public static void SetupAsVisitor(this Pawn visitor)
        {
            GuestUtility.AddNeedJoy(visitor);
            GuestUtility.AddNeedComfort(visitor);
            visitor.FixTimetable();
            visitor.FixDrugPolicy();
        }

        public static Pawn SpawnVisitor(List<Pawn> spawned, Pawn pawn, Map map, IntVec3 location)
        {
            GenerateNewGearFor(pawn, map.Tile);
            var spawnedPawn = (Pawn)GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(location, map, 5), map);

            spawnedPawn.SetupAsVisitor();
            spawnedPawn.needs.SetInitialLevels();

            spawned.Add(spawnedPawn);
            if (pawn.IsWorldPawn()) Find.WorldPawns.RemovePawn(pawn);
            return spawnedPawn;
        }

        public static IEnumerable<Pawn> GetKnownPawns(IncidentParms parms)
        {
            return Find.WorldPawns.AllPawnsAlive.Where(pawn => ValidGuest(pawn, parms.faction));
        }

        private static bool ValidGuest(Pawn pawn, Faction faction)
        {
            var validGuest = !pawn.Discarded && !pawn.Dead && !pawn.Spawned && !pawn.NonHumanlikeOrWildMan() && !pawn.Downed && pawn.Faction == faction;
            // Leader only comes when relations are good
            if (faction.leader == pawn && faction.PlayerGoodwill < 80) return false;

            return validGuest;
        }

        public static IEnumerable<Pawn> RandomlyUsingTitleAsChance(this IEnumerable<Pawn> pawns)
        {
            foreach (var pawn in pawns)
            {
                if (pawn == null) continue;
                var title = pawn.royalty?.MostSeniorTitle?.def;
                if (title == null) yield return pawn;
                else
                {
                    var chance = 25 * title.commonality / (title.seniority+100); // 0-1; seniority can be 0!
                    Log.Message($"{pawn.NameShortColored} has a chance of {chance:P2} of showing up.");
                    if (Rand.Chance(chance)) yield return pawn;
                }
            }
        }
    }
}
