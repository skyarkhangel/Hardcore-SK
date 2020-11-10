using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;
using Verse.AI;

namespace CommonSense
{
    public static class testpatch_Patch
    {
        /*
        [HarmonyPatch(typeof(GenHostility), "HostileTo", new Type[] { typeof(ThingDef), typeof(ThingDef) })]
        public static class test1_CommonSensePatch
        {
            static void Prefix(Thing a, Thing b)
            {
                Log.Message($"{a},{b}");
            }
        }
        
        [HarmonyPatch(typeof(ScribeMetaHeaderUtility), "TryCreateDialogsForVersionMismatchWarnings")]
        public static class ScribeMetaHeaderUtility_TryCreateDialogsForVersionMismatchWarnings_CommonSensePatch
        {
            static bool Prefix(ref bool __result, Action confirmedAction)
            {
                //return false;
                string value2;
                string value3;

                if (ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods(out value2, out value3))
                {
                    return true;
                };

                IEnumerable<string> items = from id in Enumerable.Range(0, ScribeMetaHeaderUtility.loadedModIdsList.Count)
                                            where ModLister.GetModWithIdentifier(ScribeMetaHeaderUtility.loadedModIdsList[id]) == null
                                            select ScribeMetaHeaderUtility.loadedModNamesList[id];
                foreach (var item in items)
                {
                    Log.Message(item);
                }
                Messages.Message(string.Format("{0}: {1}", "MissingMods".Translate(), items.ToCommaList(false)), MessageTypeDefOf.RejectInput, false);

                __result = true;
                return false;
            }
        }
        */
    }
}
