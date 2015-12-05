using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace Combat_Realism_Compatibility.Shields
{
    public class Building_Shield : Enhanced_Development.Shields.Building_Shield
    {

        public override void SpawnSetup_ShieldField()
        {
            shieldField = new Combat_Realism_Compatibility.Shields.ShieldField(this, base.Position, shieldMaxShieldStrength, shieldInitialShieldStrength, shieldShieldRadius, shieldRechargeTickDelay, shieldRecoverWarmup, shieldBlockIndirect, shieldBlockDirect, shieldFireSupression, shieldInterceptDropPod, shieldStructuralIntegrityMode, colourRed, colourGreen, colourBlue);
        }
    }
}