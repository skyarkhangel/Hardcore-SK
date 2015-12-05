using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enhanced_Development.Shields
{
    public class ShieldBuildingThingDef : Verse.ThingDef
    {
        public int shieldMaxShieldStrength;
        public int shieldInitialShieldStrength;
        public int shieldShieldRadius;

        public int shieldPowerRequiredCharging;
        public int shieldPowerRequiredSustaining;

        public int shieldRechargeTickDelay;
        public int shieldRecoverWarmup;

        public bool shieldBlockIndirect;
        public bool shieldBlockDirect;
        public bool shieldFireSupression;
        public bool shieldInterceptDropPod;

        public bool shieldStructuralIntegrityMode;

        public float colourRed;
        public float colourGreen;
        public float colourBlue;

        public List<string> SIFBuildings;
    }
}
