using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;
using Verse.Sound;
using RimWorld;


namespace SK_TLP
{
    public class TeslaLigthningPower : Building
    {
        private int Stage = 0;
      //  private int ChargesString = 0;
        private int Work = 0;
        private int Cool = 0;
        private CompGlower glowerComp;
        private CompPowerTrader powerComp;
        private static readonly SoundDef SoundHiss = SoundDef.Named("PowerOn");
        public CompTempControl compTempControl;
        private string FramePath = "Things/Building/TeslaLigthningPowerFrames/TeslaLigthningPowerF";
        //   private string FramePath2 = "Things/Building/PlasmaGeothermalPlantFrames/PlasmaBigGenVF";
        private static int FrameCount = 12;
        private int timer = 0;
        private static Graphic[] TexResFrames;
     //   private static Graphic[] TexResFrames2;
        private Graphic TexMain;

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            this.powerComp = base.GetComp<CompPowerTrader>();
            this.powerComp.PowerOn = true;
            this.glowerComp = base.GetComp<CompGlower>();
            this.glowerComp.Lit = false;

     //       TexResFrames2 = new Graphic_Single[FrameCount];
     //       for (int i = 0; i < FrameCount; i++)
      //      {
      //          TexResFrames2[i] = GraphicDatabase.Get<Graphic_Single>(FramePath2 + (i + 1), this.def.graphicData.Graphic.Shader);
       //         TexResFrames2[i].drawSize = this.def.graphicData.drawSize;
       //     }

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
       //     Scribe_Values.LookValue<int>(ref timer, "timer");
            Scribe_Values.LookValue<int>(ref Work, "Work");
            Scribe_Values.LookValue<int>(ref Cool, "Cool");
        //    Scribe_Values.LookValue<int>(ref ChargesString, "ChargesString");
            Scribe_Values.LookValue<int>(ref Stage, "Stage");

        }

        public override void Tick()
        {
            base.Tick();
            if (this.powerComp.PowerOn)
          {

            if (Stage == 0)
              {
            Work += 1;
            this.powerComp.PowerOutput = 6000;
            this.glowerComp.Lit = true;
            timer += 1;
            if (timer >= (TexResFrames.Count() * 3))
            {
                timer = 0;
            }

                 if (Work >= 30000)
                 {
                Stage = 1;
                Cool = 0;
                 }
              }

             if (Stage == 1)
              {
            Cool += 1;
            this.powerComp.PowerOutput = 0;
            this.glowerComp.Lit = false;
            timer = 0;
                  if (Rand.Value < 0.1f)
                    {
                    MoteThrower.ThrowAirPuffUp(this.TrueCenter());
                     }
                 if (Cool >= 30000)
                 {
                Stage = 0;
                Work = 0;
                 }
              }
             handleAnimation();
          }      
            
                
         //       if(Charges>=90000)
         //       {
          //          Stage=1;
          //      }
          //      if (Charges < 90000)
           //     {
           //         Charges += 1;
           //         this.powerComp.PowerOutput = -1200;
           //     }
           //     if(Stage<=0)
           //     {
           //      if (this.Position.GetTemperature() > -100)
            //      {
                GenTemperature.PushHeat(this, 40f);
            //      }
             //     if (Math.IEEERemainder(Charges, 4500f) == 1)
             //      {
              //        ChargesString += 1;
             //     }
             //   }
          //      else if(Stage >0)
           //     {
            //        if (Find.MapConditionManager.ConditionIsActive(MapConditionDefOf.SolarFlare))
             //       {
              //          this.powerComp.PowerOutput = 2400;
              //      }
              //      else
              //      {
              //          this.powerComp.PowerOutput = 1200;
               //     }
               //     if (this.Position.GetTemperature() < 100)
               //     {
               //         GenTemperature.PushHeat(this, 0.5f);
               //     }
            //    }
           // }
          //  else if (!powerComp.PowerOn && Stage <=0)
           // {
           //     Charges = 0;
           //     ChargesString = 0;
           // }
          
           

        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.Append("Power output: " + powerComp.PowerOutput + " W Status: ");
            if (Stage == 0)
            {
                stringBuilder.Append("Working");
            }
            if (Stage == 1)
            {
                stringBuilder.Append("Cooling");
            }

           //     stringBuilder.AppendLine();
            //    stringBuilder.Append("Charging Progress: " + (int)(((float)Charges / 90000f) * 100f) + "%");
            //    stringBuilder.AppendLine();
            //    stringBuilder.Append("[");
            //    for (int i = 0; i < ChargesString; i++)
            //    {
            //        stringBuilder.Append("■");
            //    }
            //    for (int j = 20; j > ChargesString; j--)
            //    {
            //        stringBuilder.Append("□");
            //    }
            //    stringBuilder.Append("]");
          //  }

            return stringBuilder.ToString();
        }

        private void handleAnimation()
        {
            if (timer < (TexResFrames.Count() * 3))
            {
                int i = timer / 3;
              //  if (Stage <= 0)
               // {
               //     TexMain = TexResFrames2[i];
              //  }
              //  else if(Stage >0)
               // {
                 TexMain = TexResFrames[i];
              //  }
                TexMain.color = base.Graphic.color;

            }
        }
        
        public override void Draw()
        {
            base.Draw();
            if (powerComp.PowerOn && this.TexMain != null)
            {
                Matrix4x4 matrix = default(Matrix4x4);
                Vector3 s = new Vector3(8f, 1f, 8f);
                matrix.SetTRS(DrawPos + Altitudes.AltIncVect, this.Rotation.AsQuat, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, this.TexMain.MatAt(this.Rotation, null), 0);
            }
        }
    }
}
