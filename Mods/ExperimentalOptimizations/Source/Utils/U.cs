using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ExperimentalOptimizations
{
    public class U
    {
        public static IEnumerable<object> DefDatabaseAllDefs(string className)
        {
            Type genericType = typeof(DefDatabase<>).MakeGenericType( AccessTools.TypeByName(className) );
            IEnumerable collection = (IEnumerable)genericType.InvokeMember("get_AllDefsListForReading", BindingFlags.InvokeMethod, null, null, new object[0]);

            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
}