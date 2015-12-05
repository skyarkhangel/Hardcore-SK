using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace SK_Mending
{
    public class ITab_Mending_Building : ITab
    {
        private Vector2 scrollPosition;

        public ITab_Mending_Building()
        {
            this.size = new Vector2(300f, 480f);
            this.labelKey = "mending.buildingTab";
        }

        protected override void FillTab()
        {
            MenderBuildingComp menderBuildingComp = ThingCompUtility.TryGetComp<MenderBuildingComp>(base.SelThing);
            if (menderBuildingComp == null)
            {
                Log.Error("Mending mod: could not find MenderBuildingComp, error");
            }
            else
            {
                GUI.Label(new Rect(10f, 20f, 150f, 20f), "Search radius: " + (int)menderBuildingComp.searchRadius);
                menderBuildingComp.searchRadius = GUI.HorizontalSlider(new Rect(10f, 50f, 150f, 20f), menderBuildingComp.searchRadius, 1f, 100f);
                if (menderBuildingComp.searchRadius == 100f)
                {
                    menderBuildingComp.searchRadius = 9999f;
                }
                if (Widgets.TextButton(new Rect(190f, 30f, 100f, 50f), "Clear all"))
                {
                    menderBuildingComp.ClearAll();
                }
                Widgets.Checkbox(new Vector2(10f, 70f), ref menderBuildingComp.outsideItems, 24f, false);
                GUI.Label(new Rect(35f, 70f, 100f, 30f), "Outside items");
                ThingFilterUI.DoThingFilterConfigWindow(new Rect(10f, 110f, this.size.x - 20f, this.size.y - 120f), ref this.scrollPosition, menderBuildingComp.GetAllowances(), menderBuildingComp.GetPossibleAllowances());
                GUI.EndGroup();
            }
        }
    }
}
