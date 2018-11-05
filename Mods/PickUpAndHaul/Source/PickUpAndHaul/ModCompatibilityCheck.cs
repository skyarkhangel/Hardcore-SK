using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace PickUpAndHaul
{
    [StaticConstructorOnStartup]
    public class ModCompatibilityCheck
    {
        public static bool CombatExtendedIsActive
        {
            get
            {
                return ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Combat Extended");
            }
        }

        public static bool AllowToolIsActive
        {
            get
            {
                return ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Allow Tool");
            }
        }

        public static bool ExtendedStorageIsActive
        {
            get
            {
                return (ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "ExtendedStorageFluffyHarmonised") || ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Core SK"));
            }
        }

        public static bool HCSKIsActive
        {
            get
            {
                return (ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Core SK"));
            }
        }

        public static bool WhileYoureUpIsActive
        {
            get
            {
                return ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "While You're Up");
            }
        }
    }
}
