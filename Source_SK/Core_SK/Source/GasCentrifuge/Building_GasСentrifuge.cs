using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace SK_GC
{
    public class Building_GC : Building
    {
        private static readonly Texture2D chooseIcon = ContentFinder<Texture2D>.Get("UI/Commands/ChooseResourceUI");
        public CompPowerTrader power;
        private List<NuclearResourceDef> availableList;
        private int resourceIndex = 0;
        private int tickCount;
        private int burnDelay;
        private int pufferSmoke;
        public CompGlower glowerComp;

        public bool IsFlareActive
        {
            get
            {
                return !Find.MapConditionManager.ConditionIsActive(MapConditionDefOf.SolarFlare);
            }
        }

        public bool CanBurnNow
        {
            get
            {
                return this.power.PowerOn && this.HasFuelInHopper && IsFlareActive;
            }
        }

        public bool HasFuelInHopper
        {
            get
            {
                return this.FuelInHopper != null;
            }
        }

        public Building AnyAdjacentHopper
        {
            get
            {
                ThingDef thingDef = ThingDef.Named("CentrifugeFeeder");
                foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(this))
                {
                    Building building = current.GetEdifice();
                    if (building != null && building.def == thingDef)
                    {
                        return (Building_Storage)building;
                    }
                }
                return null;
            }
        }
        private Thing FuelInHopper
        {
            get
            {
                ThingDef thingdef = ThingDef.Named("Uranium");
                ThingDef thingdef2 = ThingDef.Named("CentrifugeFeeder");
                foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(this))
                {
                    Thing thing = null;
                    Thing thing2 = null;
                    foreach (Thing current2 in Find.ThingGrid.ThingsAt(current))
                    {
                        if (current2.def == thingdef)
                        {
                            thing = current2;
                        }
                        if (current2.def == thingdef2)
                        {
                            thing2 = current2;
                        }
                    }
                    if (thing2 != null && thing != null)
                    {
                        return thing;
                    }
                }
                return null;
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.power = base.GetComp<CompPowerTrader>();
            glowerComp = base.GetComp<CompGlower>();
            glowerComp.Lit = false;
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

            this.burnDelay--;
            this.pufferSmoke--;
            if (!this.CanBurnNow)
            {
                glowerComp.Lit = false;
                this.power.PowerOutput = 0f;
                burnDelay = 0;
            }
            else
                if (pufferSmoke <= 0)
                {
                    MoteThrower.ThrowSmoke(this.TrueCenter(), 2f);
                    pufferSmoke = 120;
                    if (this.burnDelay <= 0)
                    {
                        glowerComp.Lit = true;
                        int num = 0;
                        List<ThingDef> list = new List<ThingDef>();
                        Thing FuelInHopper = this.FuelInHopper;
                        int num2 = Mathf.Min(FuelInHopper.stackCount, 1);
                        num += num2;
                        list.Add(FuelInHopper.def);
                        FuelInHopper.SplitOff(num2);
                        FuelInHopper = this.FuelInHopper;
                        this.power.PowerOutput = -5000f;
                        this.burnDelay = 500;
                    }
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
            if (!this.HasPower || !this.HasFuelInHopper) return;
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

                return 1.2f;
            }
        }

        public IEnumerable<NuclearResourceDef> ResourceList()
        {
            return DefDatabase<NuclearResourceDef>.AllDefs;
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