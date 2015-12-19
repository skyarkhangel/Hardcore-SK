using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace SK_Mending
{
    public class MenderBuildingComp : ThingComp
    {
        public ThingFilter allowances = null;
        public ThingFilter possibleAllowances = null;
        public float searchRadius = 500f;
        public bool outsideItems = true;
        public ThingFilter GetAllowances()
        {
            if (this.allowances == null)
            {
                this.BuildAllowancesLists();
            }
            return this.allowances;
        }
        public ThingFilter GetPossibleAllowances()
        {
            if (this.possibleAllowances == null)
            {
                this.BuildAllowancesLists();
            }
            return this.possibleAllowances;
        }
        public override void PostExposeData()
        {
            Scribe_Deep.LookDeep<ThingFilter>(ref this.allowances, "allowances");
            Scribe_Deep.LookDeep<ThingFilter>(ref this.possibleAllowances, "possibleAllowances");
            Scribe_Values.LookValue<float>(ref this.searchRadius, "searchRadius", 500f, true);
            Scribe_Values.LookValue<bool>(ref this.outsideItems, "outsideItems", true, true);
        }
        private void BuildAllowancesLists()
        {
            this.possibleAllowances = new ThingFilter();
            this.possibleAllowances.SetDisallowAll();
            this.possibleAllowances.SetAllow(SpecialThingFilterDef.Named("AllowRotten"), true);
            this.possibleAllowances.SetAllow(ThingCategoryDef.Named("Weapons"), true);
            this.possibleAllowances.SetAllow(ThingCategoryDef.Named("Apparel"), true);
            this.possibleAllowances.SetAllow(ThingCategoryDef.Named("Items"), true);
            this.allowances = new ThingFilter();
            this.allowances.CopyFrom(this.possibleAllowances);
            this.allowances.SetAllow(ThingCategoryDef.Named("Unfinished"), false);
            this.possibleAllowances.ResolveReferences();
            this.allowances.ResolveReferences();
        }
        public override void PostDraw()
        {
            if (Find.Selector.SingleSelectedThing == this.parent && this.searchRadius < 1000f)
            {
                try
                {
                    GenDraw.DrawRadiusRing(this.parent.Position, this.searchRadius - 0.1f);
                }
                catch (Exception var_0_4E)
                {
                }
            }
        }
        public void ClearAll()
        {
            this.allowances.SetDisallowAll();
        }
    }
}
