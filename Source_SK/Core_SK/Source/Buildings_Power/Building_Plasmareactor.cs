using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
using RimWorld;


namespace SK_Plasmareactor
{
    public class Building_Plasmareactor : Building
    {
        private int Stage = 0;
        private CompGlower glowerComp;
        private CompPowerTrader powerComp;
        private static readonly SoundDef SoundHiss = SoundDef.Named("PowerOn");
        public CompTempControl compTempControl;
        private string FramePath = "Things/Building/PlasmaGeothermalPlantFrames/GeothermalPlantF";
        private string FramePath2 = "Things/Building/PlasmaGeothermalPlantFrames/GeothermalPlantF";
        private static int FrameCount = 12;
        private int timer = 0;
        private static Graphic[] TexResFrames;
        private static Graphic[] TexResFrames2;
        private Graphic TexMain;
        private int burnDelay;
        private int anyAnimation;


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
                return this.powerComp.PowerOn && this.HasFuelInHopper && IsFlareActive;
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
                ThingDef thingDef = ThingDef.Named("PlasmaGenFeeder");
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
                ThingDef thingdef = ThingDef.Named("Aluminium");
                ThingDef thingdef2 = ThingDef.Named("PlasmaGenFeeder");
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

            this.powerComp = base.GetComp<CompPowerTrader>();
            this.glowerComp = base.GetComp<CompGlower>();
            glowerComp.Lit = false;

            TexResFrames2 = new Graphic_Single[FrameCount];
            for (int i = 0; i < FrameCount; i++)
            {
                TexResFrames2[i] = GraphicDatabase.Get<Graphic_Single>(FramePath2 + (i + 1), this.def.graphicData.Graphic.Shader);
                TexResFrames2[i].drawSize = this.def.graphicData.drawSize;
            }

            TexResFrames = new Graphic_Single[FrameCount];
            for (int i = 0; i < FrameCount; i++)
            {
                TexResFrames[i] = GraphicDatabase.Get<Graphic_Single>(FramePath + (i + 1), this.def.graphicData.Graphic.Shader);
                TexResFrames[i].drawSize = this.def.graphicData.drawSize;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void Tick()
        {
            base.Tick();

            this.burnDelay--;
            this.anyAnimation--;
            if (!this.CanBurnNow)
            {
                glowerComp.Lit = false;
                this.powerComp.PowerOutput = 0f;
                burnDelay = 0;
            }
            else
                if (anyAnimation <= 0)
                {
                    timer += 1;
                    if (timer >= (TexResFrames.Count() * 3))
                    {
                        timer = 0;
                    }
                    handleAnimation();
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
                        this.powerComp.PowerOutput = 5000f;
                        this.burnDelay = 500;
                    }
                }
        }
        
        private void handleAnimation()
        {
            if (timer < (TexResFrames.Count() * 3))
            {
                int i = timer / 3;
                if (Stage <= 0)
                {
                    TexMain = TexResFrames2[i];
                }
                else if(Stage >0)
                {
                    TexMain = TexResFrames[i];
                }
                TexMain.color = base.Graphic.color;

            }
        }
        
        public override void Draw()
        {
            base.Draw();
            if (powerComp.PowerOn && this.TexMain != null)
            {
                Matrix4x4 matrix = default(Matrix4x4);
                Vector3 s = new Vector3(2f, 1f, 2f);
                matrix.SetTRS(DrawPos + Altitudes.AltIncVect, this.Rotation.AsQuat, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, this.TexMain.MatAt(this.Rotation, null), 0);
            }
        }
    }
}
