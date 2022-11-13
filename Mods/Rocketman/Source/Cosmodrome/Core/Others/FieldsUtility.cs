using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace RocketMan
{
    public static class FieldsUtility
    {
        public static IEnumerable<FieldInfo> GetFields<T>() where T : Attribute
        {
            foreach (var field in RocketAssembliesInfo.Assemblies
                .Where(ass => !ass.FullName.Contains("System") && !ass.FullName.Contains("VideoTool"))
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetFields())
                .Where(f => f.HasAttribute<T>() && f.IsStatic)
                .ToArray())
            {
                if (Prefs.DevMode && RocketDebugPrefs.Debug) Log.Message(string.Format("ROCKETMAN: Found <color=yellow>settings fields</color> with {0}, {1}:{2}", typeof(T).Name,
                     field.DeclaringType.Name, field.Name));
                yield return field;
            }
        }
    }
}
