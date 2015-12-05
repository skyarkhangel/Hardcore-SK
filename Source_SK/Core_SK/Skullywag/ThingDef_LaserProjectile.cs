using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine; 
using Verse;     
using Verse.Sound;
using RimWorld;

namespace SK_LaserWeapons
{
    public class ThingDef_LaserProjectile : ThingDef
    {
        public float preFiringInitialIntensity = 0f;
        public float preFiringFinalIntensity = 0f;
        public float postFiringInitialIntensity = 0f;
        public float postFiringFinalIntensity = 0f;
        public string warmupGraphicPathSingle = null;
        public int preFiringDuration = 0;
        public int postFiringDuration = 0;
    }
}
