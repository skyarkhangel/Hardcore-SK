using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QOLTweaksPack.hugsLibSettings
{
    class MealSetHandler : StringHashSetHandler
    {

        protected override void SetToDefault()
        {
            strings.Add(ThingDefOf.MealSimple.defName);
            strings.Add(ThingDefOf.MealFine.defName);
        }
    }
}
