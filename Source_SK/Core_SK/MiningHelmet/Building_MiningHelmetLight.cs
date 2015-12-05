using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace MiningHelmet
{
    /// <summary>
    /// Building_MiningHelmetLight class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_MiningHelmetLight : Building
    {
        public CompGlower glowerComp;

        /// <summary>
        /// Initialize the glower and turn it off by default to avoid lightning when unecessary.
        /// </summary>
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            this.glowerComp = base.GetComp<CompGlower>();
            this.glowerComp.Lit = false;
        }
    }
}
