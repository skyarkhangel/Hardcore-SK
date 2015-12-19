using CommunityCoreLibrary;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace StorageSearch
{
    public class Injector_StorageSearch : SpecialInjector
    {
        ITab_Storage_Enhanced tabStorage;

        public Injector_StorageSearch()
        {
            tabStorage = new ITab_Storage_Enhanced();
        }

        public override void Inject()
        {

            FieldInfo field = typeof(Zone_Stockpile).GetField("StorageTab", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, tabStorage);

            foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
            {
                if (current.inspectorTabsResolved != null)
                {
                    bool flag = false;
                    ITab tabToReplace = null;
                    foreach (ITab tab in current.inspectorTabsResolved)
                    {
                        if (tab.GetType() == typeof(ITab_Storage))
                        {
                            flag = true;
                            tabToReplace = tab;
                            break;
                        }
                    }

                    if (flag)
                    {
                        int index = current.inspectorTabsResolved.IndexOf(tabToReplace);
                        current.inspectorTabsResolved.RemoveAt(index);
                        current.inspectorTabsResolved.Insert(index, tabStorage);
                    }
                }

            }
            Log.Message("Injector_StorageSearch : Injected");

        }
        
    }
}
