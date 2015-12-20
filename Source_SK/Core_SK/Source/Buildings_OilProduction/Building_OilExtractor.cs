using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace SK_Oilfield
{
    public class Building_OilExtractor : Building
    {
        private static readonly Texture2D chooseIcon = ContentFinder<Texture2D>.Get("UI/Commands/ChooseResourceUI");
        public CompPowerTrader power;
        private List<OilResourceDef> availableList;
        private int resourceIndex = 0;
        private int tickCount;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.power = base.GetComp<CompPowerTrader>();
            if (HasFissure)
            {
                FirstSetup();
            }
        }

        private void FirstSetup()
        {
            switch (CurFissureSize)
            {
                case FissureSize.Small:
                    {
                        availableList = (
                            from t in this.ResourceList()
                            where t.fissureSizeRequired == FissureSize.Small
                            select t).ToList();
                        break;
                    }
            }
            tickCount = availableList[0].ticksToProduce;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.resourceIndex, "count");
        }

        public override void Tick()
        {
            base.Tick();
            if (!this.HasPower || !this.HasFissure)
            {
                return;
            }
            if (Find.TickManager.TicksGame % 200 == 0)
            {
                GenTemperature.PushHeat(this, 1f);
            }
            if (Find.TickManager.TicksGame % 90 == 0)
            {
                MoteThrower.ThrowDustPuff(this.TrueCenter(), 0.3f);
            }
            if (tickCount-- <= 0)
            {
                tickCount = availableList[resourceIndex].ticksToProduce;
                Dig();
            }
        }

        private void CycleThroughAvailbleResources()
        {
                tickCount = availableList[resourceIndex].ticksToProduce;
        }

        public override string GetInspectString()
        {

            StringBuilder str = new StringBuilder();
            str.AppendLine(base.GetInspectString());
            return str.ToString();
        }

        public void Dig()
        {
            if (HasPower && HasFissure)
            {
                //Get the number to spawn
                int num = (int)(Rand.Range(availableList[resourceIndex].spawnRangeMin, availableList[resourceIndex].spawnRangeMax) * SizeMultiplier);
                //Set the spawn position to the interaction cell
                IntVec3 pos = this.InteractionCell;
                //Try to find the def for the conveyor belt loader
                ThingDef loaderDef = DefDatabase<ThingDef>.GetNamed("A2BLoader", false);
                //If the def exists, then search for any that are connected to the extractor
                if (loaderDef != null)
                {
                    //Look at each cell around the extractor
                    foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(this))
                    {
                        //Search for a loader in that cell
                        Thing thing3 = Find.ThingGrid.ThingAt(current, loaderDef);
                        //If one is found, set the spawn position to the loader position and exit the loop
                        if (thing3 != null)
                        {
                            pos = thing3.Position;
                            break;
                        }
                    }
                }

                //Look for some of the resource at the spawn position
                Thing thing2 = Find.ThingGrid.ThingAt(pos, availableList[resourceIndex].resourceDefName);
                //If some is found, the resource to be spawned will be placed in this stack
                if (thing2 != null)
                    thing2.stackCount += num;
                else
                {
                    //If there is no resource already there, then make a new stack at the position
                    Thing thing = ThingMaker.MakeThing(availableList[resourceIndex].resourceDefName);
                    thing.stackCount = num;
                    //Spawn the resource
                    GenSpawn.Spawn(thing, pos);
                }

            }
        }

        public float SizeMultiplier
        {
            get
            {
                switch (CurFissureSize)
                {
                    case FissureSize.Small:
                        return 1f;
                    default:
                        return 1f;
                }
            }
        }

        public IEnumerable<OilResourceDef> ResourceList()
        {
            return DefDatabase<OilResourceDef>.AllDefs;
        }

        public bool HasPower
        {
            get
            {
                return this.power != null && power.PowerOn;
            }
        }
        public FissureSize CurFissureSize
        {
            get
            {
                Fissure thing = (Fissure)Find.ThingGrid.ThingAt(this.Position, ThingDef.Named("Fissure"));
                if (thing != null)
                    return thing.size;
                else
                    return FissureSize.None;
            }
        }

        public bool HasFissure
        {
            get
            {
                return this.CurFissureSize != FissureSize.None;
            }
        }
    }
}
