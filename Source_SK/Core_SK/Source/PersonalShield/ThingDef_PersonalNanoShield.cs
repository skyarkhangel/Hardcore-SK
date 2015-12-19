using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace Core_SK.PersonalShields
{
    public class ThingDef_PersonalNanoShield : ThingDef
    {
        //public float energyLossPerDamage = 0.027f;
        public float minDrawSize = 1.2f;
        public float maxDrawSize = 1.55f;
        public int maxEnergy = 100;
        public bool isRotating = true;
        public String bubbleGraphicPath = "Other/ShieldBubble";
        public String soundAbsorb = null;
        public String soundBreak = null;
        public String soundReset = null;
    }
}
