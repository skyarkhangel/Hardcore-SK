using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace SK_RareMiner
{
    public class Building_RareExtractor : Building
    {
        private static readonly Texture2D chooseIcon = ContentFinder<Texture2D>.Get("UI/Commands/ChooseResourceUI");
        public CompPowerTrader power;
        private List<RareMinerResourceDef> availableList;
        private int resourceIndex = 0;
        private int tickCount;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.power = base.GetComp<CompPowerTrader>();
            FirstSetup();
        }

        private void FirstSetup()
        {
            availableList = this.ResourceList().ToList();
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
            if (!this.HasPower)
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

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (base.GetGizmos() != null)
            {
                foreach (var c in base.GetGizmos())
                {
                    yield return c;
                }
            }
            Command_Action com = new Command_Action()
{
    action = () =>
        {
            CycleThroughAvailbleResources();
        },
    activateSound = SoundDefOf.Click,
    defaultDesc = "ChangeResourceDesc".Translate(),
    defaultLabel = "ChangeResourceLabel".Translate(),
    icon = chooseIcon,
    groupKey = 313740004
};
            yield return com;
        }

        private void CycleThroughAvailbleResources()
        {
            if (availableList.Count == 1)
            {
                return;
            }
            else
            {
                resourceIndex++;
                if (resourceIndex >= availableList.Count)
                {
                    resourceIndex = 0;
                }
                tickCount = availableList[resourceIndex].ticksToProduce;
            }
        }

        public override string GetInspectString()
        {

            StringBuilder str = new StringBuilder();
            str.AppendLine(base.GetInspectString());
            str.AppendLine(String.Format("SelectedResourceLabel".Translate(), availableList[resourceIndex].label));
            return str.ToString();
        }

        public void Dig()
        {
            if (HasPower)
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
                        return 1f;
            }
        }

        public IEnumerable<RareMinerResourceDef> ResourceList()
        {
            return DefDatabase<RareMinerResourceDef>.AllDefs;
        }


        public bool HasPower
        {
            get
            {
                return this.power != null && power.PowerOn;
            }
        }
    }
}
