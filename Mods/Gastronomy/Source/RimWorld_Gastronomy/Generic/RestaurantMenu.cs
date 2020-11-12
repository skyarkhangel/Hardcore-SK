using System.Linq;
using RimWorld;
using Verse;

namespace Gastronomy
{
    public class RestaurantMenu : IExposable
    {
        private ThingFilter menuFilter;
        private ThingFilter menuGlobalFilter;

        public void ExposeData()
        {
            Scribe_Deep.Look(ref menuFilter, "menuFilter");
        }

        public bool IsOnMenu(ThingDef def)
        {
            if (menuFilter == null) InitMenuFilter(menuGlobalFilter);
            return menuFilter.Allows(def);
        }

        public bool IsOnMenu(Thing thing)
        {
            if (menuFilter == null) InitMenuFilter(menuGlobalFilter);
            return menuFilter.Allows(thing);
        }

        public void GetMenuFilters(out ThingFilter filter, out ThingFilter globalFilter)
        {
            if (menuGlobalFilter == null) InitMenuGlobalFilter();
            globalFilter = menuGlobalFilter;
            if (menuFilter == null) InitMenuFilter(globalFilter);
            filter = menuFilter;
        }

        private void InitMenuGlobalFilter()
        {
            menuGlobalFilter = new ThingFilter();
            menuGlobalFilter.SetAllow(ThingCategoryDefOf.Foods, true);
            menuGlobalFilter.SetAllow(ThingCategoryDefOf.Drugs, true);
            menuGlobalFilter.allowedQualitiesConfigurable = true;
        }

        private void InitMenuFilter(ThingFilter globalFilter)
        {
            menuFilter = new ThingFilter();
            menuFilter.SetAllowAll(globalFilter);
            foreach (var def in menuFilter.AllowedThingDefs.ToArray())
            {
                if(def.ingestible?.joy <= 0)
                {
                    var preferability = def.ingestible.preferability;
                    if (preferability == FoodPreferability.DesperateOnly 
                        || preferability == FoodPreferability.DesperateOnlyForHumanlikes 
                        || preferability == FoodPreferability.RawBad)
                        menuFilter.SetAllow(def, false);

                    if (preferability == FoodPreferability.Undefined) Log.Message($"{def.LabelCap} has preferability undefined");
                }
            }
        }
    }
}
