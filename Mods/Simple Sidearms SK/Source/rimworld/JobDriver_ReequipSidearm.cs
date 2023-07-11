using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace SimpleSidearms.rimworld
{
    public class JobDriver_ReequipSidearm : JobDriver_EquipSidearm
    {
        public override bool MemorizeOnPickup { get { return false; } }
    }
}
