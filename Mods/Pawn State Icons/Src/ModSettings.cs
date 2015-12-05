using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Verse;

namespace RimWorldIconMod
{
    /*
     * Config file (serializes to XML)
     */
    public class ModSettings
    {
        public float iconSize = 1.0f;
        public float iconSizeMult = 1.0f;

        public int iconsInColumn = 3;
        public float iconDistanceX = 1.0f;
        public float iconDistanceY = 1.0f;

        public float iconOffsetX = 1.0f;
        public float iconOffsetY = 1.0f;

        public bool iconsHorizontal = false;
        public bool iconsScreenScale = true;

        public string iconSet = "default";

        /* Visibiliy */

        public bool showTargetPoint = true;

        public bool showAggressive = true;
        public bool showDazed = true;
        public bool showLeave = true;

        public bool showDraft = true;
        public bool showIdle = true;
        public bool showUnarmed = true;

        public bool showHungry = true;
        public bool showSad = true;
        public bool showTired = true;

        public bool showDisease = true;
        public bool showEffectiveness = true;
        public bool showBloodloss = true;

        public bool showHot = true;
        public bool showCold = true;
        public bool showNaked = true;

        public bool showDrunk = true;

        public bool showApparelHealth = true;
        public bool showPacific = true;

        /* Limits */
        
        public float limit_MoodLess = 0.25f;
        public float limit_FoodLess = 0.25f;
        public float limit_RestLess = 0.25f;
        public float limit_EfficiencyLess = 0.33f;
        public float limit_diseaseLess = 1.0f;
        public float limit_tempComfortOffset = 0.0f;
        public float limit_bleedMult = 3.0f;
        public float limit_apparelHealthLess = 0.33f;

    }
}
