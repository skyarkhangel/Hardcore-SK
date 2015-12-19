using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace SK
{
    class ThingDef_ProjectileFrag : ThingDef
    {
        public int fragAmountSmall = 0;  //Amount of fragments to scatter
        public int fragAmountMedium = 0;
        public int fragAmountLarge = 0;

        public float fragRange = 0;    //How far the fragments fly

        public ThingDef fragProjectileSmall = null;    //What projectiles to use for fragments
        public ThingDef fragProjectileMedium = null;
        public ThingDef fragProjectileLarge = null;
    }
}
