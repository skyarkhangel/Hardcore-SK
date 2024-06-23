using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace NOQ_CoordReadout
{
    public class Settings : ModSettings
    {
        public static bool ShowCoords = true;
        public static float xPosUI = 125f;
        public static float yPosUI = 65f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref ShowCoords, "ShowCoords", true);
            Scribe_Values.Look(ref xPosUI, "xPosUI", 125f);
            Scribe_Values.Look(ref yPosUI, "yPosUI", 65f);
            base.ExposeData();            
        }

        public static void DoSettingsWindowContents( Rect rect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(rect);
            list.CheckboxLabeled("CoordRead.ShowCoords".Translate(), ref ShowCoords,
                                  "CoordRead.ShowCoordsTip".Translate());
            list.Label("CoordRead.xPosUI".Translate(xPosUI.ToString() ) );
            xPosUI = Mathf.Round(list.Slider( xPosUI, 15f, (float)UI.screenWidth-125f));
            list.Label("CoordRead.yPosUI".Translate(yPosUI.ToString() ) );
            yPosUI = Mathf.Round(list.Slider( yPosUI, 65f, (float)UI.screenHeight-15f));
            list.Label("Defaults are (125, 65)");
            list.Label("May exceed screen if it is resized but saves location as to be able to restore screen size");
            list.End();
        }
    }

}
