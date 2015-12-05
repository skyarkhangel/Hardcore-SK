using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
using RimWorld;


namespace SK_Plasmagenerator
    {
    public static class Util_PG
    {
        public static ThingDef PGDef
        {
            get
            {
                return (ThingDef.Named("PlasmaGen"));
            }
        }
    }
    public class Building_PlasmaGenerator : Building
    {
        private int Stage = 0;
        private int ChargesString = 0;
        private int Charges = 0;
        private CompGlower glowerComp;
        private CompPowerTrader powerComp;
        private static readonly SoundDef SoundHiss = SoundDef.Named("PowerOn");
        public CompTempControl compTempControl;
        private string FramePath = "Things/Building/PlasmaGenFrames/PlasmaGenF";
        private string FramePath2 = "Things/Building/PlasmaGenFrames/PlasmaGenVF";
        private static int FrameCount = 12;
        private int timer = 0;
        private static Graphic[] TexResFrames;
        private static Graphic[] TexResFrames2;
        private Graphic TexMain;

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            this.powerComp = base.GetComp<CompPowerTrader>();
            this.glowerComp = base.GetComp<CompGlower>();

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
            Scribe_Values.LookValue<int>(ref timer, "timer");
            Scribe_Values.LookValue<int>(ref Charges, "Charges");
            Scribe_Values.LookValue<int>(ref ChargesString, "ChargesString");
            Scribe_Values.LookValue<int>(ref Stage, "Stage");

        }

        public override void Tick()
        {
            base.Tick();
            if (this.powerComp.PowerOn)
            {
                timer += 1;
                if (timer >= (TexResFrames.Count() * 3))
                {
                    timer = 0;
                }
                handleAnimation();

                if (Charges >= 90000)
                {
                    Stage = 1;
                }
                if (Charges < 90000)
                {
                    Charges += 1;
                    this.powerComp.PowerOutput = -1200;
                }
                if (Stage <= 0)
                {
                    if (this.Position.GetTemperature() > -100)
                    {
                        GenTemperature.PushHeat(this, -0.5f);
                    }
                    if (Math.IEEERemainder(Charges, 4500f) == 1)
                    {
                        ChargesString += 1;
                    }
                }
                else if (Stage > 0)
                {
                    if (Find.MapConditionManager.ConditionIsActive(MapConditionDefOf.SolarFlare))
                    {
                        this.powerComp.PowerOutput = 2400;
                    }
                    else
                    {
                        this.powerComp.PowerOutput = 1200;
                    }
                    if (this.Position.GetTemperature() < 100)
                    {
                        GenTemperature.PushHeat(this, 0.5f);
                    }
                }
            }
            else if (!powerComp.PowerOn && Stage <= 0)
            {
                Charges = 0;
                ChargesString = 0;
            }



        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.Append("Power output: " + powerComp.PowerOutput + " W");
            if (Stage <= 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Charging Progress: " + (int)(((float)Charges / 90000f) * 100f) + "%");
                stringBuilder.AppendLine();
                stringBuilder.Append("[");
                for (int i = 0; i < ChargesString; i++)
                {
                    stringBuilder.Append("■");
                }
                for (int j = 20; j > ChargesString; j--)
                {
                    stringBuilder.Append("□");
                }
                stringBuilder.Append("]");
            }

            return stringBuilder.ToString();
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
                else if (Stage > 0)
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
