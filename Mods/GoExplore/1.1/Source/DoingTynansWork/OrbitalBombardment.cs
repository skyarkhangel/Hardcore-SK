using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Verse.AI;

namespace LetsGoExplore
{
    [DefOf]
    public static class ThingDefsLGE
    {
        public static ThingDef OribitalBombardmentBeamLGE;
    }

    public class MapComponent_OrbitalBombardmentLGE : MapComponent
    {
        public bool active = false;

        public int tickAtNextVolley = -1;

        public IntRange volleyTimeout = new IntRange(340, 420);

        public IntVec3 volleyTarget;

        public MapComponent_OrbitalBombardmentLGE(Map map) : base(map)
        {
        }

        public override void MapGenerated()
        {
            base.MapGenerated();
        }

        public override void MapRemoved()
        {
            base.MapRemoved();
            this.active = false;
        }

        public void StartBombing()
        {
            this.active = true;
            this.volleyTarget = PickNewSpotToBomb();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            //Check if bombardment is active, otherwise return
            if (!this.active)
            {
                return;
            }
            if(Find.TickManager.TicksGame >= tickAtNextVolley)
            {
                //mostly stolen from Verb_PowerBeam
                BombardmentBeamLGE powerBeam = (BombardmentBeamLGE)GenSpawn.Spawn(ThingDefsLGE.OribitalBombardmentBeamLGE, this.volleyTarget, this.map, WipeMode.Vanish);
                powerBeam.duration = 600;
                powerBeam.StartStrike();

                CalculateNextBombardmentTick();
                this.volleyTarget = PickNewSpotToBomb();
                DrawTargeterBeam(this.volleyTarget);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.tickAtNextVolley, "tickAtNextVolley", -1, false);
            Scribe_Values.Look<bool>(ref this.active, "active", false, false);
            Scribe_Values.Look<IntVec3>(ref this.volleyTarget, "volleyTarget");
        }

        public IntVec3 PickNewSpotToBomb()
        {
            //just selects random cell. Maybe add some kind of predicate using Verse.CellFinderLoose.TryFindRandomNotEdgeCellWith to make sure only visible/unfogged cells are selected
            return CellFinder.RandomNotEdgeCell(10, this.map);
        }

        public void CalculateNextBombardmentTick()
        {
            this.tickAtNextVolley = Find.TickManager.TicksGame + volleyTimeout.RandomInRange;
        }

        public void DrawTargeterBeam(IntVec3 position)
        {
            if (!position.IsValid || !position.InBounds(this.map))
            {
                position = PickNewSpotToBomb();
            }
            MoteMaker.MakeBombardmentMote(position, this.map);
        }

    }


    public class BombardmentBeamLGE : OrbitalStrike
    {
        //Custom class simply copied and adjusted from vanilla PowerBeam

        //reduced from 15
        public const float Radius = 7f;

        //reduced from 4
        private const int FiresStartedPerTick = 2;

        private static readonly IntRange FlameDamageAmountRange = new IntRange(40, 80);

        private static readonly IntRange CorpseFlameDamageAmountRange = new IntRange(5, 10);

        private static List<Thing> tmpThings = new List<Thing>();

        public override void StartStrike()
        {
            base.StartStrike();
            this.MakePowerBeamMote(base.Position, base.Map);
        }

        //copied from vanilla motemaker
        public void MakePowerBeamMote(IntVec3 cell, Map map)
        {
            Mote mote = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_PowerBeam, null);
            mote.exactPosition = cell.ToVector3Shifted();
            mote.Scale = 45f;
            mote.rotationRate = 1.28f;
            GenSpawn.Spawn(mote, cell, map, WipeMode.Vanish);
        }

        public override void Tick()
        {
            base.Tick();
            if (!base.Destroyed)
            {
                for (int i = 0; i < FiresStartedPerTick; i++)
                {
                    this.StartRandomFireAndDoFlameDamage();
                }
            }
        }

        private void StartRandomFireAndDoFlameDamage()
        {
            IntVec3 c = (from x in GenRadial.RadialCellsAround(base.Position, Radius, true)
                         where x.InBounds(base.Map)
                         select x).RandomElementByWeight((IntVec3 x) => 1f - Mathf.Min(x.DistanceTo(base.Position) / 15f, 1f) + 0.05f);
            FireUtility.TryStartFireIn(c, base.Map, Rand.Range(0.1f, 0.925f));
            BombardmentBeamLGE.tmpThings.Clear();
            BombardmentBeamLGE.tmpThings.AddRange(c.GetThingList(base.Map));
            for (int i = 0; i < BombardmentBeamLGE.tmpThings.Count; i++)
            {
                int num = (!(BombardmentBeamLGE.tmpThings[i] is Corpse)) ? BombardmentBeamLGE.FlameDamageAmountRange.RandomInRange : BombardmentBeamLGE.CorpseFlameDamageAmountRange.RandomInRange;
                Pawn pawn = BombardmentBeamLGE.tmpThings[i] as Pawn;
                //No log entries because there is no instigator
                Thing thing = BombardmentBeamLGE.tmpThings[i];
                DamageDef flame = DamageDefOf.Flame;
                float amount = (float)num;
                Thing instigator = this.instigator;
                ThingDef weaponDef = this.weaponDef;
                thing.TakeDamage(new DamageInfo(flame, amount, 0f, -1f, instigator, null, weaponDef, DamageInfo.SourceCategory.ThingOrUnknown, null));
            }
            BombardmentBeamLGE.tmpThings.Clear();
        }

    }

    public class SitePartWorker_OrbitalBombardmentLGE : SitePartWorker
    {
        public override void PostMapGenerate(Map map)
        {
            //every map component gets added to every map so I just need to activate it
            map.GetComponent<MapComponent_OrbitalBombardmentLGE>().StartBombing();
        }
    }
}
