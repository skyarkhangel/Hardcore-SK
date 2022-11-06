using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace LetsGoExplore
{
    public class GoExploreSettings : ModSettings
    {
        public bool lostCity = true;
        public bool ambrosiaAnimals = true;
        public bool shipCoreStartup = true;
        public bool prisonCamp = true;
        public bool newSettlement = true;
        public bool researchRequest = true;
        public bool interceptedMessage = true;
        //public float exampleFloat = 200f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref lostCity, "lostCity");
            Scribe_Values.Look(ref ambrosiaAnimals, "ambrosiaAnimals");
            Scribe_Values.Look(ref shipCoreStartup, "shipCoreStartup");
            Scribe_Values.Look(ref prisonCamp, "prisonCamp");
            Scribe_Values.Look(ref newSettlement, "newSettlement");
            Scribe_Values.Look(ref researchRequest, "researchRequest");
            Scribe_Values.Look(ref interceptedMessage, "interceptedMessage");
            //Scribe_Values.Look(ref exampleFloat, "exampleFloat", 200f);
            base.ExposeData();
        }
    }

    public class GoExplore : Mod
    {
        /// <summary>
        /// A reference to our settings.
        /// </summary>
        GoExploreSettings settings;

        /// <summary>
        /// A mandatory constructor which resolves the reference to our settings.
        /// </summary>
        /// <param name="content"></param>
        public GoExplore(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<GoExploreSettings>();
        }

        /// <summary>
        /// The (optional) GUI part to set your settings.
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("LostCityName".Translate(), ref settings.lostCity, "LGEBoolToolTip".Translate());
            listingStandard.CheckboxLabeled("LetterLabelAmbrosiaAnimalsLGE".Translate(), ref settings.ambrosiaAnimals, "LGEBoolToolTip".Translate());
            listingStandard.CheckboxLabeled("LetterLabelShipCoreStartupLGE".Translate(), ref settings.shipCoreStartup, "LGEBoolToolTip".Translate());
            listingStandard.CheckboxLabeled("LetterLabelPrisonCampLGE".Translate(), ref settings.prisonCamp, "LGEBoolToolTip".Translate());
            listingStandard.CheckboxLabeled("LetterLabelNewSettlementLGE".Translate(), ref settings.newSettlement, "LGEBoolToolTip".Translate());
            listingStandard.CheckboxLabeled("LetterLabelInterceptedMessageLGE".Translate(), ref settings.researchRequest, "LGEBoolToolTip".Translate());
            listingStandard.CheckboxLabeled("InterceptedMessageName".Translate(), ref settings.interceptedMessage, "LGEBoolToolTip".Translate());

            //listingStandard.Label("exampleFloatExplanation");
            //settings.exampleFloat = listingStandard.Slider(settings.exampleFloat, 100f, 300f);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings.
        /// Using .Translate() is optional, but does allow for localisation.
        /// </summary>
        /// <returns>The (translated) mod name.</returns>
        public override string SettingsCategory()
        {
            return "GoExploreName".Translate();
        }
    }
}
