using UnityEngine;
using Verse;

namespace WorldGenRules
{
    class Settings : ModSettings
    {
        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            
            listing_Standard.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
