using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Verse;

namespace RocketMan
{
    public static class PatchingUtility
    {
        private static readonly Dictionary<MethodBase, string> summaries = new Dictionary<MethodBase, string>();

        public static bool IsValidTarget(this MethodBase method)
        {
            return method != null && !method.IsAbstract && method.DeclaringType == method.ReflectedType && method.HasMethodBody() && method.GetMethodBody()?.GetILAsByteArray()?.Length > 1;
        }

        public static string GetMethodPath(this MethodBase method)
        {
            return string.Format("{0}.{1}:{2}", method.DeclaringType.Namespace, method.ReflectedType.Name, method.Name);
        }

        public static string GetMethodSummary(this MethodBase method)
        {
            return method != null ? (summaries.TryGetValue(method, out string summary) ? summary : summaries[method] = string.Format("([REFLECTED] {0}.{1}:{3}, [DECLARING] {0}.{2}:{3}, [ISSTATIC] {7}, [ISPUBLIC] {8}, [ISVIRTUAL] {5}, [ABSTRACT] {6})", method.ReflectedType.Namespace, method.ReflectedType.Name, method.ReflectedType.Name, method.Name, method.DeclaringType.Namespace, method.DeclaringType.Name, method.IsVirtual, method.IsAbstract, method.IsStatic, method.IsPublic)) : null;
        }

        public static IEnumerable<T> GetPatches<T, P>(Assembly assembly) where P : IPatch where T : IPatchInfo<P>
        {
            IEnumerable<Type> types = assembly.GetLoadableTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.HasAttribute<P>());
            foreach (var type in types)
            {
                T patchInfo = (T)Activator.CreateInstance(typeof(T), type);
                if (!patchInfo.IsValid)
                {
                    Log.Message($"{type} is not a valid patch!");
                    continue;
                }
                yield return patchInfo;
            }
        }
    }
}
