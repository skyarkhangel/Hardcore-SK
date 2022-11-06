using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI;

namespace LetsGoExplore
{

    public class SymbolResolver_LostCityBaseLGE : SymbolResolver
    {

        public override void Resolve(ResolveParams rp)
        {
            //Add rubble
            ResolveParams resolveParamsDebris = rp;
            BaseGen.symbolStack.Push("scatterRubbleLGE", resolveParamsDebris);

            //set up variables for the rooms to skip floors and walls to give the ruined look
            int roomCount = rp.rect.Width * rp.rect.Height / 30;
            ResolveParams resolveParams = rp;
            resolveParams.chanceToSkipWallBlock = 0.38f;
            resolveParams.chanceToSkipFloor = 0.38f;
            resolveParams.noRoof = true;

            for (int i = 0; i < roomCount; i++)
            {
                int width = Rand.RangeInclusive(3, 12);
                int height = Rand.RangeInclusive(3, 12);

                resolveParams.wallStuff = GenStuff.RandomStuffInexpensiveFor(ThingDefOf.Wall, TechLevel.Industrial);
                resolveParams.floorDef = BaseGenUtility.CorrespondingTerrainDef(resolveParams.wallStuff, true);
                resolveParams.rect = new CellRect(Rand.RangeInclusive(rp.rect.minX, rp.rect.maxX - width), Rand.RangeInclusive(rp.rect.minZ, rp.rect.maxZ - height), width, height);
                BaseGen.symbolStack.Push("emptyRoom", resolveParams);
            }

            //Clear out the entire map (including roofs)
            rp.clearRoof = true;
            BaseGen.symbolStack.Push("clear", rp);

        }
    }

    public class SymbolResolver_ScatterSkeletonsLGE : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            //spawn dead dudes
            ResolveParams resolveParamsCorpses = rp;
            int corpseCount = rp.rect.Width * rp.rect.Height / 180; //Adjust as needed. Maybe run symbol multiple times in parent function
            for (int i = 0; i < corpseCount; i++)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(DefsOfLGE.CityDwellerLGE, Faction.OfAncients, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 20f, false, true, true, false, false, false, false, null, null, null, null, null, null, null);
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                HealthUtility.DamageUntilDead(pawn);
                pawn.Corpse.GetComp<CompRottable>().RotProgress = 5000000;
                //This corpse is now 10 years old with 36mio ticks
                pawn.Corpse.Age = 36000000;
                resolveParamsCorpses.singleThingToSpawn = pawn.Corpse;
                BaseGen.symbolStack.Push("thing", resolveParamsCorpses);
            }
        }
    }

    public class SymbolResolver_ScatterRubbleLGE : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            ResolveParams resolveParamsDebris = rp;
            resolveParamsDebris.singleThingDef = ThingDefOf.Filth_RubbleBuilding;
            int buildingRubbleCount = rp.rect.Width * rp.rect.Height / 3;
            for (int i = 0; i < buildingRubbleCount; i++)
            {
                BaseGen.symbolStack.Push("thing", resolveParamsDebris);
            }
            resolveParamsDebris.singleThingDef = ThingDefOf.Filth_RubbleRock;
            int slagRubbleCount = rp.rect.Width * rp.rect.Height / 3;
            for (int i = 0; i < slagRubbleCount; i++)
            {
                BaseGen.symbolStack.Push("thing", resolveParamsDebris);
            }
        }
    }

    public class SymbolResolver_ExampleBaselineLostCityLGE : SymbolResolver
    {

        public override void Resolve(ResolveParams rp)
        {
            //spawn rewards
            int rewardCount = Rand.RangeInclusive(5, 6);
            ResolveParams resolveParamsReward = rp;
            for (int i = 0; i < rewardCount; i++)
            {
                int width = Rand.RangeInclusive(6, 9);
                int height = Rand.RangeInclusive(6, 9);

                resolveParamsReward.rect = new CellRect(Rand.RangeInclusive(rp.rect.minX, rp.rect.maxX - width), Rand.RangeInclusive(rp.rect.minZ, rp.rect.maxZ - height), width, height);
                if (Rand.Chance(0.7f))
                {
                    BaseGen.symbolStack.Push("guardedStockpileLGE", resolveParamsReward);
                }
                else
                {
                    BaseGen.symbolStack.Push("unguardedStockpileLGE", resolveParamsReward);
                }
                
            }

            //spawn dead dudes
            ResolveParams resolveParamsCorpses = rp;
            BaseGen.symbolStack.Push("scatteredSkeletonsLGE", resolveParamsCorpses);

            //City baseline
            ResolveParams resolveParamsCityLandscape = rp;
            BaseGen.symbolStack.Push("lostCityBaseLGE", resolveParamsCityLandscape);

        }
    }

    public class SymbolResolver_FalloutShelterLGE : SymbolResolver
    {

        public override void Resolve(ResolveParams rp)
        {
            //Random chance for Danger
            if (Rand.Chance(0.9f))
            {
                if (rp.disableHives == false) //Meaning they should be active
                {
                    ResolveParams resolveParamsDanger = rp;
                    resolveParamsDanger.hivesCount = Rand.RangeInclusive(1, 2);
                    resolveParamsDanger.rect = rp.rect.ContractedBy(1);
                    if(StorytellerUtility.DefaultThreatPointsNow(Find.World) > 4000f)
                    {
                        resolveParamsDanger.hivesCount = Rand.RangeInclusive(2, 3);
                    }
                    BaseGen.symbolStack.Push("hives", resolveParamsDanger);
                }
                else
                {
                    //MechanoidsSpawn
                    ResolveParams resolveParamsDanger = rp;
                    resolveParamsDanger.mechanoidsCount = Rand.RangeInclusive(3, 5);
                    resolveParamsDanger.rect = rp.rect.ContractedBy(1);
                    if (StorytellerUtility.DefaultThreatPointsNow(Find.World) > 4000f)
                    {
                        resolveParamsDanger.mechanoidsCount = Rand.RangeInclusive(5, 7);
                    }
                    BaseGen.symbolStack.Push("randomMechanoidGroup", resolveParamsDanger);
                }
            }

            //Put in a storage container
            ResolveParams resolveParamsAncientStorage = rp;
            resolveParamsAncientStorage.rect = new CellRect(rp.rect.minX + 3, rp.rect.minZ + 3, rp.rect.Width - 6, rp.rect.Height - 5);
            //Reward Loot
            resolveParamsAncientStorage.stockpileConcreteContents = RewardGeneratorUtilityLGE.GenerateStorageBoxReward();
            BaseGen.symbolStack.Push("storageBoxLGE", resolveParamsAncientStorage);

            ResolveParams resolveParamsCryoCaskets = rp;
            resolveParamsCryoCaskets.rect = rp.rect.ContractedBy(1);
            //We could steal from the SymbolResolver_Interior_AncientTemple here and include rewards and/or dangers
            BaseGen.symbolStack.Push("ancientShrinesGroup", resolveParamsCryoCaskets);

            //spawn empty room
            BaseGen.symbolStack.Push("emptyRoom", rp);

            //Clear out the entire rect (including roofs)
            rp.clearRoof = true;
            BaseGen.symbolStack.Push("clear", rp);
        }
    }

    public class SymbolResolver_ToxicFalloutCityBaseLGE : SymbolResolver
    {

        public override void Resolve(ResolveParams rp)
        {
            //Decide wether danger should be Hives or Mechs
            if (Rand.Chance(0.5f)) //is 0.5 usually
            {
                rp.disableHives = true;
            }
            else
            {
                rp.disableHives = false;
            }

            //Spawn shelters
            int shelterCount = Rand.RangeInclusive(5, 6); //4-6 default value
            ResolveParams resolveParamsShelter = rp;
            for (int i = 0; i < shelterCount; i++)
            {
                int width = Rand.RangeInclusive(11, 13);
                int height = Rand.RangeInclusive(9, 13);

                resolveParamsShelter.rect = new CellRect(Rand.RangeInclusive(rp.rect.minX, rp.rect.maxX - width), Rand.RangeInclusive(rp.rect.minZ, rp.rect.maxZ - height), width, height);
                BaseGen.symbolStack.Push("falloutShelterLGE", resolveParamsShelter);
            }

            //spawn fallout victims
            ResolveParams resolveParamsCorpses = rp;
            BaseGen.symbolStack.Push("scatteredFalloutVictimsLGE", resolveParamsCorpses);

            //City baseline
            ResolveParams resolveParamsCityLandscape = rp;
            BaseGen.symbolStack.Push("lostCityBaseLGE", resolveParamsCityLandscape);
        }
    }

    public class SymbolResolver_ScatterFalloutVictimsLGE : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            //spawn dead dudes
            ResolveParams resolveParamsCorpses = rp;
            int corpseCount = rp.rect.Width * rp.rect.Height / 180; //Adjust as needed. Maybe run symbol multiple times in parent function
            for (int i = 0; i < corpseCount; i++)
            {
                //TODO: Add a custom pawnkinddef here to take control of the label. Maybe just civilian or something
                PawnGenerationRequest request = new PawnGenerationRequest(DefsOfLGE.CityDwellerLGE, Faction.OfAncients, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 20f, false, true, true, false, false, false, false, null, null, null, null, null, null, null);
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, 1);
                //progress just 200k ticks so they are just rotted not sceletons
                pawn.Corpse.GetComp<CompRottable>().RotProgress = 200000;
                pawn.Corpse.Age = 1880000;
                resolveParamsCorpses.singleThingToSpawn = pawn.Corpse;
                BaseGen.symbolStack.Push("thing", resolveParamsCorpses);
            }
        }
    }

    public class SymbolResolver_InfestedCityBaseLGE : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            //spawn insect hives
            ResolveParams resolveParamsHives = rp;
            resolveParamsHives.rect = resolveParamsHives.rect.ContractedBy(15);
            BaseGen.symbolStack.Push("scatterInsectHivesLGE", resolveParamsHives);

            //spawn rewards
            int rewardCount = Rand.RangeInclusive(4, 5);
            ResolveParams resolveParamsReward = rp;
            resolveParamsReward.disableHives = false;
            for (int i = 0; i < rewardCount; i++)
            {
                int width = Rand.RangeInclusive(6, 9);
                int height = Rand.RangeInclusive(6, 9);

                resolveParamsReward.rect = new CellRect(Rand.RangeInclusive(rp.rect.minX, rp.rect.maxX - width), Rand.RangeInclusive(rp.rect.minZ, rp.rect.maxZ - height), width, height);
                BaseGen.symbolStack.Push("guardedStockpileLGE", resolveParamsReward);
            }

            //spawn skeletons
            ResolveParams resolveParamsCorpses = rp;
            BaseGen.symbolStack.Push("scatteredSkeletonsLGE", resolveParamsCorpses);

            //City baseline
            ResolveParams resolveParamsCityLandscape = rp;
            BaseGen.symbolStack.Push("lostCityBaseLGE", resolveParamsCityLandscape);
        }
    }

    public class SymbolResolver_ScatterInsectHivesLGE : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            //spawn Hives
            ResolveParams resolveParamsHives = rp;
            int hiveCount = rp.rect.Width * rp.rect.Height / 1200; //Adjust as needed. Maybe run symbol multiple times in parent function. It was 800 at first
            IntVec3 loc;
            hiveCount = Math.Min(30, hiveCount);
            for (int i = 0; i < hiveCount; i++)
            {
                if (this.TryFindHivePos(rp.rect, out loc))
                {
                    Thing thingHive = ThingMaker.MakeThing(ThingDefOf.Hive, null);
                    thingHive.SetFaction(Faction.OfInsects, null);
                    Hive hive = (Hive)GenSpawn.Spawn(thingHive, loc, BaseGen.globalSettings.map, WipeMode.Vanish);
                    hive.SpawnPawnsUntilPoints(Hive.InitialPawnsPoints);
                }
            }
        }

        private bool TryFindHivePos(CellRect rect, out IntVec3 pos)
        {
            Map map = BaseGen.globalSettings.map;
            return (from mc in rect.Cells
                    where mc.Standable(map)
                    select mc).TryRandomElement(out pos);
        }
    }

    public class SymbolResolver_GuardedStockpileLGE : SymbolResolver
    {

        public override void Resolve(ResolveParams rp)
        {
            //Danger Spawn
            if (rp.disableHives == false) //Meaning they should be active
            {
                ResolveParams resolveParamsDanger = rp;
                resolveParamsDanger.hivesCount = Rand.RangeInclusive(1, 2);
                resolveParamsDanger.rect = rp.rect.ContractedBy(1);
                if (StorytellerUtility.DefaultThreatPointsNow(Find.World) > 2500f)
                {
                    resolveParamsDanger.hivesCount = Rand.RangeInclusive(1, 3);
                }
                if (StorytellerUtility.DefaultThreatPointsNow(Find.World) > 4500f)
                {
                    resolveParamsDanger.hivesCount = Rand.RangeInclusive(2, 3);
                }
                if (StorytellerUtility.DefaultThreatPointsNow(Find.World) > 7000f)
                {
                    resolveParamsDanger.hivesCount = Rand.RangeInclusive(3, 4);
                }
                BaseGen.symbolStack.Push("hives", resolveParamsDanger);
            }
            else
            {
                //MechanoidsSpawn
                ResolveParams resolveParamsDanger = rp;
                resolveParamsDanger.mechanoidsCount = Rand.RangeInclusive(2, 4);
                resolveParamsDanger.rect = rp.rect.ContractedBy(1);
                if (StorytellerUtility.DefaultThreatPointsNow(Find.World) > 2500f)
                {
                    resolveParamsDanger.mechanoidsCount = Rand.RangeInclusive(3, 5);
                }
                if (StorytellerUtility.DefaultThreatPointsNow(Find.World) > 4500f)
                {
                    resolveParamsDanger.mechanoidsCount = Rand.RangeInclusive(4, 7);
                }
                if (StorytellerUtility.DefaultThreatPointsNow(Find.World) > 7000f)
                {
                    resolveParamsDanger.mechanoidsCount = Rand.RangeInclusive(6, 10);
                }
                BaseGen.symbolStack.Push("randomMechanoidGroup", resolveParamsDanger);
            }

            //Generate Loot
            ResolveParams resolveParamsReward = rp;
            //Chance for a storage container
            if (Rand.Chance(0.9f))
            {
                resolveParamsReward.stockpileConcreteContents = RewardGeneratorUtilityLGE.GenerateStorageBoxReward();
                resolveParamsReward.rect = new CellRect(rp.rect.minX + 3, rp.rect.minZ + 3, rp.rect.Width - 5, rp.rect.Height - 5);
                BaseGen.symbolStack.Push("storageBoxLGE", resolveParamsReward);
            }
            else
            {
                //Scattered Loot
                if (Rand.Chance(0.85f))
                {
                    resolveParamsReward.stockpileConcreteContents = RewardGeneratorUtilityLGE.GenerateStockpileReward(0.28f);
                }
                else
                {
                    resolveParamsReward.stockpileConcreteContents = RewardGeneratorUtilityLGE.GenerateWeaponsCacheReward(Rand.RangeInclusive(4, 5));
                }
                resolveParamsReward.rect.ContractedBy(1);
                BaseGen.symbolStack.Push("spawnStockpileLGE", resolveParamsReward);
            }

            //spawn empty room
            BaseGen.symbolStack.Push("emptyRoom", rp);

            //Clear out the entire rect (including roofs)
            rp.clearRoof = true;
            BaseGen.symbolStack.Push("clear", rp);
        }
    }

    public class SymbolResolver_UnguardedStockpileLGE : SymbolResolver
    {

        public override void Resolve(ResolveParams rp)
        {
            //Generate Loot
            ResolveParams resolveParamsReward = rp;
            //Chance for a storage container
            if (Rand.Chance(0.3f))
            {
                resolveParamsReward.stockpileConcreteContents = RewardGeneratorUtilityLGE.GenerateStorageBoxReward();
                resolveParamsReward.rect = new CellRect(rp.rect.minX + 3, rp.rect.minZ + 3, rp.rect.Width - 5, rp.rect.Height - 5);
                BaseGen.symbolStack.Push("storageBoxLGE", resolveParamsReward);
            }
            else
            {
                //Scattered Loot
                if (Rand.Chance(0.85f))
                {
                    resolveParamsReward.stockpileConcreteContents = RewardGeneratorUtilityLGE.GenerateStockpileReward(0.28f);
                }
                else
                {
                    resolveParamsReward.stockpileConcreteContents = RewardGeneratorUtilityLGE.GenerateWeaponsCacheReward(Rand.RangeInclusive(4, 5));
                }
                resolveParamsReward.rect.ContractedBy(1);
                BaseGen.symbolStack.Push("spawnStockpileLGE", resolveParamsReward);
            }

            //spawn empty room
            BaseGen.symbolStack.Push("emptyRoom", rp);

            //Clear out the entire rect (including roofs)
            rp.clearRoof = true;
            BaseGen.symbolStack.Push("clear", rp);
        }
    }

    public class SymbolResolver_StorageBoxLGE : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp);
        }

        public override void Resolve(ResolveParams rp)
        {
            if (Rand.Chance(0.77f))
            {
                Rot4 rot = (Rand.Chance(0.5f)) ? Rot4.East : Rot4.South;
                Building_AncientStorageUnitLGE storageUnit = (Building_AncientStorageUnitLGE) ThingMaker.MakeThing(DefsOfLGE.AncientStorageUnitLGE, null);
                if (rp.stockpileConcreteContents != null && (rp.stockpileConcreteContents.Count > 0))
                {
                    for (int i = 0; i < rp.stockpileConcreteContents.Count; i++)
                    {
                        storageUnit.TryAcceptThing(rp.stockpileConcreteContents[i], false);
                    }
                }

                GenSpawn.Spawn(storageUnit, rp.rect.RandomCell, BaseGen.globalSettings.map, rot);
            }
            else
            {
                Building brokenStorage = (Building) ThingMaker.MakeThing(DefsOfLGE.BrokenStorageUnitLGE, null);
                GenSpawn.Spawn(brokenStorage, rp.rect.RandomCell, BaseGen.globalSettings.map);
            }
        }
    }
}

