using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PeteTimesSix.SimpleSidearms
{
    [StaticConstructorOnStartup]
    public static class TextureResources
    {

        public static readonly Texture2D drawPocket = ContentFinder<Texture2D>.Get("drawPocket", true);
        public static readonly Texture2D drawPocketPrimary = ContentFinder<Texture2D>.Get("drawPocketPrimary", true);
        public static readonly Texture2D drawPocketMemory = ContentFinder<Texture2D>.Get("drawPocketMemory", true);
        public static readonly Texture2D drawPocketMemoryPrimary = ContentFinder<Texture2D>.Get("drawPocketMemoryPrimary", true);
        public static readonly Texture2D drawPocketTemp = ContentFinder<Texture2D>.Get("drawPocketTemp", true);

        public static readonly Texture2D preferRanged = ContentFinder<Texture2D>.Get("preferRanged", true);
        public static readonly Texture2D preferSkilled = ContentFinder<Texture2D>.Get("preferSkilled", true);
        public static readonly Texture2D preferMelee = ContentFinder<Texture2D>.Get("preferMelee", true);

        public static readonly Texture2D defaultRanged = ContentFinder<Texture2D>.Get("sidearm_default_ranged", true);
        public static readonly Texture2D preferredMelee = ContentFinder<Texture2D>.Get("sidearm_preferred_melee", true);
        public static readonly Texture2D forcedAlways = ContentFinder<Texture2D>.Get("sidearm_forced_always", true);
        public static readonly Texture2D forcedDrafted = ContentFinder<Texture2D>.Get("sidearm_forced_drafted", true);

        public static readonly Texture2D blockedWeapon = ContentFinder<Texture2D>.Get("blocked_weapon", true);

        public static readonly Texture2D weaponTypeManual = ContentFinder<Texture2D>.Get("weaponType_manual", true);
        public static readonly Texture2D weaponTypeDangerous = ContentFinder<Texture2D>.Get("weaponType_dangerous", true);
        public static readonly Texture2D weaponTypeEMP = ContentFinder<Texture2D>.Get("weaponType_emp", true);

        public static readonly Texture2D SpeedBiasSliderPositive = ContentFinder<Texture2D>.Get("speedBiasSliderPositive", true);
        public static readonly Texture2D SpeedBiasSliderNegative = ContentFinder<Texture2D>.Get("speedBiasSliderNegative", true);

        public static readonly Texture2D FirstTimeSettingsWarningIcon = ContentFinder<Texture2D>.Get("settingsWarningIcon", true);
        
        public static readonly Texture2D MissingDefIcon = ContentFinder<Texture2D>.Get("missingIcon", true);
    }
}
