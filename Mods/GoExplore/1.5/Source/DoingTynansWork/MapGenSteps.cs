using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.BaseGen;
using Verse.Noise;
using UnityEngine;

namespace LetsGoExplore
{
    public class GenStep_BasicGenLGE : GenStep
    {
        protected CellRect adventureRegion;

        protected ResolveParams baseResolveParams;

        public override int SeedPart => 54849541;

        public override void Generate(Map map, GenStepParams parms)
        {
            int num = map.Size.x / 10;
            int num2 = 8 * map.Size.x / 10;
            int num3 = map.Size.z / 10;
            int num4 = 8 * map.Size.z / 10;
            this.adventureRegion = new CellRect(num, num3, num2, num4);
            this.adventureRegion.ClipInsideMap(map);
            BaseGen.globalSettings.map = map;
            IntVec3 playerStartSpot;
            CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => GenGrid.Standable(v, map), map, 0f, out playerStartSpot);
            MapGenerator.PlayerStartSpot = playerStartSpot;
            this.baseResolveParams = default(ResolveParams);
            this.baseResolveParams.rect = this.adventureRegion;
        }
    }

    public class GenStep_ToxicFalloutLostCity : GenStep_BasicGenLGE
    {
        public override void Generate(Map map, GenStepParams parms)
        {
            base.Generate(map, parms);
            this.baseResolveParams.rect = new CellRect(0, 0, map.Size.x, map.Size.z);
            BaseGen.symbolStack.Push("toxicFalloutCityBaseLGE", this.baseResolveParams);
            BaseGen.Generate();

            //after generation spawn permanent toxic fallout
            GameCondition falloutCondition = GameConditionMaker.MakeCondition(GameConditionDefOf.ToxicFallout);
            falloutCondition.Permanent = true;
            map.GameConditionManager.RegisterCondition(falloutCondition);
        }
    }

    public class GenStep_InfestedLostCity : GenStep_BasicGenLGE
    {
        public override void Generate(Map map, GenStepParams parms)
        {
            base.Generate(map, parms);
            this.baseResolveParams.rect = new CellRect(0, 0, map.Size.x, map.Size.z);
            BaseGen.symbolStack.Push("infestedCityBaseLGE", this.baseResolveParams);
            BaseGen.Generate();
        }
    }

    public class GenStep_StandartLostCity : GenStep_BasicGenLGE
    {
        public override void Generate(Map map, GenStepParams parms)
        {
            base.Generate(map, parms);
            this.baseResolveParams.rect = new CellRect(0, 0, map.Size.x, map.Size.z);
            BaseGen.symbolStack.Push("exampleBaselineLostCityLGE", this.baseResolveParams);
            BaseGen.Generate();
        }
    }

    public class GenStep_AmbrosiaAnimalsGenLGE : GenStep_BasicGenLGE
    {
        public override int SeedPart
        {
            get
            {
                return 812342045;
            }
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            base.Generate(map, parms);
            int randomInRange = GenStep_AmbrosiaAnimalsGenLGE.AmbrosiaSproutSizeWidth.RandomInRange;
            int randomInRange2 = GenStep_AmbrosiaAnimalsGenLGE.AmbrosiaSproutSizeHeight.RandomInRange;
            CellRect rect = new CellRect(Rand.RangeInclusive(this.adventureRegion.minX + 10, this.adventureRegion.maxX - AmbrosiaSproutSizeWidth.max -10), Rand.RangeInclusive(this.adventureRegion.minZ + 10, this.adventureRegion.maxZ - AmbrosiaSproutSizeHeight.max -10), randomInRange, randomInRange2);
            rect.ClipInsideMap(map);
            foreach (IntVec3 c2 in rect)
            {
                CompTerrainPumpDry.AffectCell(map, c2);
                for (int i = 0; i < 8; i++)
                {
                    Vector3 b = Rand.InsideUnitCircleVec3 * 3f;
                    IntVec3 c3 = IntVec3.FromVector3(c2.ToVector3Shifted() + b);
                    if (c3.InBounds(map))
                    {
                        CompTerrainPumpDry.AffectCell(map, c3);
                    }
                }
            }
            ResolveParams baseResolveParams = default(ResolveParams);
            baseResolveParams.rect = rect;
            BaseGen.globalSettings.map = map;
            BaseGen.symbolStack.Push("ambrosiaAreaPrepareLGE", baseResolveParams);
            BaseGen.Generate();
        }

        private static readonly IntRange AmbrosiaSproutSizeWidth = new IntRange(16, 24);

        private static readonly IntRange AmbrosiaSproutSizeHeight = new IntRange(16, 24);
    }

    public class GenStep_ShipCoreStartupLGE : GenStep_BasicGenLGE
    {
        private static readonly IntRange ShipCoreStartupSizeWidth = new IntRange(52, 54);

        private static readonly IntRange ShipCoreStartupSizeHeight = new IntRange(54, 56);

        public override void Generate(Map map, GenStepParams parms)
        {
            base.Generate(map, parms);
            int shipCoreWidth = GenStep_ShipCoreStartupLGE.ShipCoreStartupSizeWidth.RandomInRange;
            int shipCoreHeight = GenStep_ShipCoreStartupLGE.ShipCoreStartupSizeHeight.RandomInRange;
            CellRect rect = new CellRect(this.adventureRegion.minX + (this.adventureRegion.Width / 2) - (shipCoreWidth / 2), this.adventureRegion.minZ + (this.adventureRegion.Height / 2) - (shipCoreHeight /2), shipCoreWidth, shipCoreHeight);
            rect.ClipInsideMap(map);
            foreach (IntVec3 c2 in rect)
            {
                CompTerrainPumpDry.AffectCell(map, c2);
                for (int i = 0; i < 8; i++)
                {
                    Vector3 b = Rand.InsideUnitCircleVec3 * 3f;
                    IntVec3 c3 = IntVec3.FromVector3(c2.ToVector3Shifted() + b);
                    if (c3.InBounds(map))
                    {
                        CompTerrainPumpDry.AffectCell(map, c3);
                    }
                }
            }
            ResolveParams baseResolveParams = default(ResolveParams);
            baseResolveParams.rect = rect;
            BaseGen.globalSettings.map = map;
            BaseGen.symbolStack.Push("shipCoreStartupLGE", baseResolveParams);
            BaseGen.Generate();
        }
    }

    public class GenStep_PrisonCampLGE : GenStep_BasicGenLGE
    {
        private static readonly IntRange PrisonCampSizeWidth = new IntRange(24, 26);

        private static readonly IntRange PrisonCampSizeHeight = new IntRange(28, 32);

        public override void Generate(Map map, GenStepParams parms)
        {
            base.Generate(map, parms);
            int prisonCampWidth = GenStep_PrisonCampLGE.PrisonCampSizeWidth.RandomInRange;
            int prisonCampHeight = GenStep_PrisonCampLGE.PrisonCampSizeHeight.RandomInRange;
            CellRect rect = new CellRect(this.adventureRegion.minX + (this.adventureRegion.Width / 2) - (prisonCampWidth / 2), this.adventureRegion.minZ + (this.adventureRegion.Height / 2) - (prisonCampHeight / 2), prisonCampWidth, prisonCampHeight);
            rect.ClipInsideMap(map);
            foreach (IntVec3 c2 in rect)
            {
                CompTerrainPumpDry.AffectCell(map, c2);
                for (int i = 0; i < 8; i++)
                {
                    Vector3 b = Rand.InsideUnitCircleVec3 * 3f;
                    IntVec3 c3 = IntVec3.FromVector3(c2.ToVector3Shifted() + b);
                    if (c3.InBounds(map))
                    {
                        CompTerrainPumpDry.AffectCell(map, c3);
                    }
                }
            }
            ResolveParams baseResolveParams = default(ResolveParams);
            baseResolveParams.rect = rect;
            BaseGen.globalSettings.map = map;
            BaseGen.symbolStack.Push("prisonCampLGE", baseResolveParams);
            BaseGen.Generate();
        }
    }

}
