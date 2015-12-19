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


namespace SK_GWT
{
    public class GauseWindTurbine : Building
    {
     //   private int Stage = 0;
      //  private int ChargesString = 0;
      //  private int Charges = 0;
        private const float MaxUsableWindIntensity = 1.5f;
        public int updateWeatherEveryXTicks = 250;
        private int ticksSinceWeatherUpdate;
        private CompGlower glowerComp;
        private CompPowerTrader powerComp;
        private static readonly SoundDef SoundHiss = SoundDef.Named("PowerOn");
        public CompTempControl compTempControl;
        private string FramePath = "Things/Building/GauseWindTurbineFrames/GauseWindTurbineF";
        //   private string FramePath2 = "Things/Building/GauseWindTurbineFrames/PlasmaBigGenVF";
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
            Scribe_Values.LookValue<int>(ref this.ticksSinceWeatherUpdate, "updateCounter", 0, false);
       //     Scribe_Values.LookValue<int>(ref timer, "timer");
        //    Scribe_Values.LookValue<int>(ref Charges, "Charges");
        //    Scribe_Values.LookValue<int>(ref ChargesString, "ChargesString");
        //    Scribe_Values.LookValue<int>(ref Stage, "Stage");

        }

        public override void Tick()
        {
           
          base.Tick();
			if (this.powerComp == null || !this.powerComp.PowerOn)
			{
				this.powerComp.PowerOutput = 0f;
				return;
			}
			this.ticksSinceWeatherUpdate++;
			if (this.ticksSinceWeatherUpdate >= this.updateWeatherEveryXTicks)
			{
				float num = Mathf.Min(WindManager.WindSpeed, 1.5f);
				this.ticksSinceWeatherUpdate = 0;
				this.powerComp.PowerOutput = -(this.powerComp.props.basePowerConsumption * num);
				
			}


            if ( this.powerComp.PowerOutput > 3000  )
                {
                  timer += 30;
                    if (timer >= (TexResFrames.Count() * 30))
                     {
                       timer = 0;
                      }
                        }
            else if (  this.powerComp.PowerOutput > 1000   || this.powerComp.PowerOutput <= 3000 )
                {
                  timer += 10;
                    if (timer >= (TexResFrames.Count() * 30))
                     {
                       timer = 0;
                      }
                            }
            else if (  this.powerComp.PowerOutput < 1000 )
                {
                  timer += 2;
                    if (timer >= (TexResFrames.Count() * 30))
                     {
                       timer = 0;
                      }
                }


            handleAnimation();
		}
          
           

        
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.Append("Power output: " + powerComp.PowerOutput + " W");
           // if (Stage <= 0)
           // {
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
            if (timer < (TexResFrames.Count() * 30))
            {
                int i = timer / 30;
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
                Vector3 s = new Vector3(5f, 1f, 5f);
                matrix.SetTRS(DrawPos + Altitudes.AltIncVect, this.Rotation.AsQuat, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, this.TexMain.MatAt(this.Rotation, null), 0);
            }
        }
    }
}
