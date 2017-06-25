using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;


namespace Nandonalt_ColonyLeadership
{
    public class Building_Chalkboard : Building
    {

        public int state = 0;
        public int frame = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.state, "state", 0);
            Scribe_Values.Look<int>(ref this.state, "frame", 0);
        }

        public override Graphic Graphic
        {
            get
            {
          
                if (state == 0 || frame < 0) return ModTextures.CH_Empty.GetColoredVersion(ShaderDatabase.CutoutComplex, this.DrawColor, this.DrawColorTwo); 
                else if (state == 1) 
                {
                    if (frame == 0) return ModTextures.CH_Botanist1.GetColoredVersion(ShaderDatabase.CutoutComplex, this.DrawColor, this.DrawColorTwo);
                    if (frame == 1) return ModTextures.CH_Botanist2.GetColoredVersion(ShaderDatabase.CutoutComplex, this.DrawColor, this.DrawColorTwo);
                    if (frame >= 2) return ModTextures.CH_Botanist.GetColoredVersion(ShaderDatabase.CutoutComplex, this.DrawColor, this.DrawColorTwo);
                }
                else if (state == 2)
                {
                    if (frame == 0) return ModTextures.CH_Warrior1.GetColoredVersion(ShaderDatabase.CutoutComplex, this.DrawColor, this.DrawColorTwo);
                    if (frame == 1) return ModTextures.CH_Warrior2.GetColoredVersion(ShaderDatabase.CutoutComplex, this.DrawColor, this.DrawColorTwo);
                    if (frame >= 2) return ModTextures.CH_Warrior.GetColoredVersion(ShaderDatabase.CutoutComplex, this.DrawColor, this.DrawColorTwo);
                }
                else if (state == 3) 
                {
                    if (frame == 0) return ModTextures.CH_Carpenter1.GetColoredVersion(ShaderDatabase.CutoutComplex, this.DrawColor, this.DrawColorTwo);
                    if (frame == 1) return ModTextures.CH_Carpenter2.GetColoredVersion(ShaderDatabase.CutoutComplex, this.DrawColor, this.DrawColorTwo);
                    if (frame >= 2) return ModTextures.CH_Carpenter.GetColoredVersion(ShaderDatabase.CutoutComplex, this.DrawColor, this.DrawColorTwo);
                }
                else if (state == 4)
                {
                    if (frame == 0) return ModTextures.CH_Scientist1.GetColoredVersion(ShaderDatabase.CutoutComplex, this.DrawColor, this.DrawColorTwo);
                    if (frame == 1) return ModTextures.CH_Scientist2.GetColoredVersion(ShaderDatabase.CutoutComplex, this.DrawColor, this.DrawColorTwo);
                    if (frame >= 2) return ModTextures.CH_Scientist.GetColoredVersion(ShaderDatabase.CutoutComplex, this.DrawColor, this.DrawColorTwo);
                }

                return this.def.graphicData.GraphicColoredFor(this);
            }                                
    }
}
}
