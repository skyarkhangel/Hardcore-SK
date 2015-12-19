using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using System.Xml;

namespace  Enhanced_Development.Stargate.Saving
{
    class SaveThings
    {
        public static void save(List<Thing> thingsToSave, string fileLocation, Thing source)
        {
            Scribe.InitWriting(fileLocation,"Stargate");

            //Log.Message("Starting Save");
            //Save Pawn

            //Scribe_Collections.LookList<Thing>(ref thingsToSave, "things", LookMode.Deep, (object)null);

            //Scribe.EnterNode("map");
            //Scribe.EnterNode("things");
            //source.ExposeData();
            Scribe_Collections.LookList<Thing>(ref thingsToSave, "things", LookMode.Deep, (object)null);
            //Scribe.ExitNode();

            //Scribe.ExitNode();

            /*
            for (int i = 0; i < thingsToSave.Count; i++)
            {
                Scribe_Deep.LookDeep<Thing>(ref thingsToSave[i], thingsToSave[i].ThingID);
            }*/

            Scribe.FinalizeWriting();
            Scribe.mode = LoadSaveMode.Inactive;
            //Log.Message("End Save");
        }

        public static void load(ref List<Thing> thingsToLoad, string fileLocation, Thing currentSource)
        {
            Log.Message("ScribeINIT, loding from:" + fileLocation);
            Scribe.InitLoading(fileLocation);

            //Scribe.EnterNode("Stargate");

            Log.Message("DeepProfiler.Start()");
            DeepProfiler.Start("Load non-compressed things");

            List<Thing> list2 = (List<Thing>)null;
            Log.Message("Scribe_Collections.LookList");
            Scribe_Collections.LookList<Thing>(ref thingsToLoad, "things", LookMode.Deep);
            Log.Message("List1Count:" + thingsToLoad.Count);

            Log.Message("DeepProfiler.End()");
            DeepProfiler.End();

            //Scribe.ExitNode();
            Scribe.mode = LoadSaveMode.Inactive;

            //Log.Message("list: " + thingsToLoad.Count.ToString());


            Log.Message("Exit Node");
            //Scribe.ExitNode();


            Log.Message("ResolveAllCrossReferences");
            CrossRefResolver.ResolveAllCrossReferences();


            Log.Message("DoAllPostLoadInits");
            PostLoadInitter.DoAllPostLoadInits();

            Log.Message("Return");

        }

    }
}
