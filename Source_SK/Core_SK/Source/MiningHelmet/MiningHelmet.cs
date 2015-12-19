using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace MiningHelmet
{
    /// <summary>
    /// MiningHelmet class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class MiningHelmet : Apparel
    {
        public Building_MiningHelmetLight headLight;
        public bool lightIsOn = false;
        public bool refreshIsNecessary = false;

        /// <summary>
        /// Perform the MiningHelmet main treatment:
        /// - switch on the headlight if the pawn is awake and under a natural roof or in the open dark and mining,
        /// - switch off the headlight otherwise.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            // Only tick once a second when light is off.
            if ((this.lightIsOn == false)
                && (Find.TickManager.TicksGame % 60 != 0))
            {
                return;
            }

            // Helmet on ground or wearer is sleeping.
            if ((this.wearer == null)
                || this.wearer.InBed())
            {
                SwitchOffHeadLight();
                return;
            }

            // Colonist is mining.
            bool colonistIsMining = (this.wearer.CurJob != null)
                && (this.wearer.CurJob.def == JobDefOf.Mine);
            if (colonistIsMining)
            {
                SwitchOnHeadLight(this.wearer.Position);
                return;
            }
            
            // Colonist is under a natural roof.
            bool colonistIsUnderARoof = Find.RoofGrid.Roofed(this.wearer.Position);
            if (colonistIsUnderARoof)
            {
                RoofDef roofType = Find.RoofGrid.RoofAt(this.wearer.Position);
                if (roofType != RoofDefOf.RoofConstructed)
                {
                    SwitchOnHeadLight(this.wearer.Position);
                    return;
                }
            }

            // Other cases.
            SwitchOffHeadLight();
        }

        public void SwitchOnHeadLight(IntVec3 newPosition)
        {
            this.lightIsOn = true;
            if (this.headLight == null)
            {
                this.headLight = GenSpawn.Spawn(Util_MiningHelmet.miningHelmetGlowerDef, newPosition) as Building_MiningHelmetLight;
            }
            if ((newPosition != this.headLight.Position)
                || this.refreshIsNecessary)
            {
                this.headLight.Position = newPosition;
                // We need to toggle the light on and off to force the glower position update.
                this.headLight.glowerComp.Lit = false;
                this.headLight.glowerComp.Lit = true;
                this.refreshIsNecessary = false;
            }
        }

        public void SwitchOffHeadLight()
        {
            this.lightIsOn = false;
            if (this.headLight != null)
            {
                this.headLight.Destroy();
                this.headLight = null;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.LookReference<Building_MiningHelmetLight>(ref this.headLight, "headLight");
            Scribe_Values.LookValue<bool>(ref this.lightIsOn, "lightIsOn");
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                this.refreshIsNecessary = true;
            }
        }
    }
}
