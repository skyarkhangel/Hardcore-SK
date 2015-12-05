using System;
using System.Collections.Generic;
using Verse;

namespace kNumbers
{
    class MapComponent_Numbers : MapComponent
    {

        public static Dictionary<MainTabWindow_Numbers.pawnType, List<KListObject>> savedKLists;
        public static MainTabWindow_Numbers.pawnType chosenPawnType;
        public static bool hasData = false;

        private List<KListObject> tmpKList;

        public override void ExposeData()
        {
            
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                SetDefaultValues();
            }

            if(Scribe.mode == LoadSaveMode.Saving) { 
                savedKLists = MainTabWindow_Numbers.savedKLists;
                chosenPawnType = MainTabWindow_Numbers.chosenPawnType;
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.Saving)
            {
                Scribe_Values.LookValue<MainTabWindow_Numbers.pawnType>(ref chosenPawnType, "chosenPawnType", MainTabWindow_Numbers.pawnType.Colonists);
                foreach (MainTabWindow_Numbers.pawnType type in Enum.GetValues(typeof(MainTabWindow_Numbers.pawnType)))
                {
                    tmpKList = savedKLists[type];
                    Scribe_Collections.LookList<KListObject>(ref tmpKList, "klist-" + type, LookMode.Deep);
                    savedKLists[type] = tmpKList;
                }
                hasData = true;
            }

        }

        private static void SetDefaultValues()
        {
            chosenPawnType = MainTabWindow_Numbers.pawnType.Colonists;
            savedKLists = new Dictionary<MainTabWindow_Numbers.pawnType, List<KListObject>>(5);
            foreach(MainTabWindow_Numbers.pawnType pType in Enum.GetValues(typeof(MainTabWindow_Numbers.pawnType)))
            {
                savedKLists.Add(pType, new List<KListObject>(10));
            }
        }


        public static void InitMapComponent()
        {
            if(Find.Map.GetComponent<MapComponent_Numbers>() == null)
            {
                SetDefaultValues();
                Find.Map.components.Add(new MapComponent_Numbers());
            }
        }
    }
}
