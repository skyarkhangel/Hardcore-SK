using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.BaseGen;

namespace LetsGoExplore
{
    public class SymbolResolver_AmbrosiaAnimalsLGE : SymbolResolver
    {
        private static readonly IntRange CountRange = new IntRange(24, 30);

        private const int SpawnRadius = 9;

        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp);
        }

        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            IntVec3 root = IntVec3Utility.ToIntVec3(rp.rect.CenterVector3);
            IntVec3 motherVec;
            CellFinder.TryFindRandomCellNear(root, map, 3, (IntVec3 x) => x.Standable(map) && map.fertilityGrid.FertilityAt(x) > ThingDefOf.Plant_Ambrosia.plant.fertilityMin, out motherVec);
            float eventPoints = StorytellerUtility.DefaultThreatPointsNow(Find.World) * 0.6f;

            if(eventPoints > 4000f)
            {
                eventPoints *= 0.9f;
                if(eventPoints > 7000f)
                {
                    eventPoints = 7000f;
                }
            }

            foreach (IntVec3 vec in rp.rect)
            {
                if (map.terrainGrid.TerrainAt(vec).fertility < 0.7f)
                {
                    if (Rand.Chance(0.5f))
                    {
                        map.terrainGrid.SetTerrain(vec, TerrainDefOf.Gravel);
                    }
                }
            }

            //spawn mother plant
            MotherAmbrosiaLGE motherAmbrosia = (MotherAmbrosiaLGE)GenSpawn.Spawn(DefsOfLGE.Plant_MotherAmbrosiaLGE, motherVec, map, WipeMode.Vanish);
            motherAmbrosia.Growth = Rand.Range(0.9f, 0.98f);

            PawnKindDef animalKind;
            (from k in map.Biome.AllWildAnimals
             where Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(map.Tile, k.race)
             select k).TryRandomElement(out animalKind);
            if (animalKind == null)
            {
                animalKind = ThingDefOfVanilla.Warg;
            }
            motherAmbrosia.SpawnAnimals(animalKind, eventPoints);


            int randomInRange = (int)(eventPoints / 80);//SymbolResolver_AmbrosiaAnimalsLGE.CountRange.RandomInRange;
            for (int i = 0; i < randomInRange; i++)
            {
                IntVec3 intVec;
                if (!CellFinder.TryRandomClosewalkCellNear(motherVec, map, SpawnRadius, out intVec, (IntVec3 x) => this.CanSpawnAt(x, map)))
                {
                    break;
                }
                Plant plant = intVec.GetPlant(map);
                if (plant != null)
                {
                    plant.Destroy(DestroyMode.Vanish);
                }
                Plant ambrosia = (Plant)GenSpawn.Spawn(ThingDefOf.Plant_Ambrosia, intVec, map, WipeMode.Vanish);
                ambrosia.Growth = Rand.Range(0.6f, 0.95f);
            }

            //Spawn ambrosia reward
            ResolveParams resolveParamsReward = rp;
            resolveParamsReward.rect = CellRect.CenteredOn(motherVec, SpawnRadius - 2);
            resolveParamsReward.stockpileConcreteContents = RewardGeneratorUtilityLGE.GenerateAmbrosia((int) (eventPoints / 150f));
            BaseGen.symbolStack.Push("spawnStockpileLGE", resolveParamsReward);
        }

        private bool CanSpawnAt(IntVec3 c, Map map)
        {
            if (!c.Standable(map) || map.fertilityGrid.FertilityAt(c) < ThingDefOf.Plant_Ambrosia.plant.fertilityMin || c.GetEdifice(map) != null)
            {
                return false;
            }
            List<Thing> thingList = c.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (thingList[i].def == ThingDefOf.Plant_Ambrosia)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class SymbolResolver_AmbrosiaAreaPrepareLGE : SymbolResolver
    {
        private static readonly IntRange CountRange = new IntRange(24, 30);

        private const int SpawnRadius = 9;

        private static List<Thing> tmpThingsToDestroy = new List<Thing>();

        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp);
        }

        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            foreach (IntVec3 vec in rp.rect)
            {
                if (!vec.Standable(map))
                {
                    SymbolResolver_AmbrosiaAreaPrepareLGE.tmpThingsToDestroy.Clear();
                    SymbolResolver_AmbrosiaAreaPrepareLGE.tmpThingsToDestroy.AddRange(vec.GetThingList(BaseGen.globalSettings.map));
                    for (int j = 0; j < SymbolResolver_AmbrosiaAreaPrepareLGE.tmpThingsToDestroy.Count; j++)
                    {
                        if (SymbolResolver_AmbrosiaAreaPrepareLGE.tmpThingsToDestroy[j].def.destroyable)
                        {
                            SymbolResolver_AmbrosiaAreaPrepareLGE.tmpThingsToDestroy[j].Destroy(DestroyMode.Vanish);
                        }
                    }
                    BaseGen.globalSettings.map.roofGrid.SetRoof(vec, null);

                }
                if (map.terrainGrid.TerrainAt(vec).fertility < 0.7f)
                {
                    if (Rand.Chance(0.3f))
                    {
                        map.terrainGrid.SetTerrain(vec, TerrainDefOf.Gravel);
                    }
                }
            }

            ResolveParams resolveParamsAmbrosia = rp;
            BaseGen.symbolStack.Push("ambrosiaAnimalsLGE", resolveParamsAmbrosia);

            BaseGen.symbolStack.Push("ensureCanReachMapEdge", rp);
        }
    }
}
