using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace RocketMan
{
    public static class FunctionsUtility
    {
        public static Type[] GetLoadableTypes(this Assembly a)
        {
            List<Type> types;
            try
            {
                types = a.GetTypes().Where(i => i != null).ToList();
            }
            catch (ReflectionTypeLoadException e)
            {
                if (RocketDebugPrefs.Debug)
                    Log.Warning($"<color=blue>[ROCKETMAN]</color>:[{a.FullName}] Gettypes fallback mod activated!");
                types = new List<Type>();
                foreach (Type type in e.Types)
                {
                    try
                    {
                        if (type != null && type.Assembly == a)
                            types.Add(type);
                    }
                    catch (BadImageFormatException badImageFormatException)
                    {
                        Log.Error($"ROCKETMAN:[{a.FullName}] {a.FullName} is a bad file! (corrupted):{badImageFormatException}");
                    }
                }
            }
            return types?.ToArray() ?? null;
        }

        public static IEnumerable<Action> GetActions<T>() where T : Attribute
        {
            foreach (var method in RocketAssembliesInfo.Assemblies
                .SelectMany(a => AccessTools.GetTypesFromAssembly(a))
                .SelectMany(t => AccessTools.GetDeclaredMethods(t))
                .Where(m => m.HasAttribute<T>())
                .ToArray())
            {
                Logger.Debug(string.Format("ROCKETMAN: Found action with attribute {0}, {1}:{2}", typeof(T).Name,
                     method.DeclaringType.Name, method.Name));
                yield return () => { method.Invoke(null, null); };
            }
        }

        public static IEnumerable<Action<P>> GetActions<T, P>() where T : Attribute
        {
            foreach (var method in RocketAssembliesInfo.Assemblies
                .SelectMany(a => AccessTools.GetTypesFromAssembly(a))
                .SelectMany(t => AccessTools.GetDeclaredMethods(t))
                .Where(m => m.HasAttribute<T>())
                .ToArray())
            {
                Logger.Debug(string.Format("ROCKETMAN: Found action with attribute {0}, {1}:{2}", typeof(T).Name,
                     method.DeclaringType.Name, method.Name));
                yield return (param) => { method.Invoke(null, new object[] { param }); };
            }
        }

        public static IEnumerable<Action<P, K>> GetActions<T, P, K>() where T : Attribute
        {
            foreach (var method in RocketAssembliesInfo.Assemblies
                .SelectMany(a => AccessTools.GetTypesFromAssembly(a))
                .SelectMany(t => AccessTools.GetDeclaredMethods(t))
                .Where(m => m.HasAttribute<T>())
                .ToArray())
            {
                Logger.Debug(string.Format("ROCKETMAN: Found action with attribute {0}, {1}:{2}", typeof(T).Name,
                     method.DeclaringType.Name, method.Name));
                yield return (param1, param2) => { method.Invoke(null, new object[] { param1, param2 }); };
            }
        }

        public static IEnumerable<Func<P>> GetFunctions<T, P>() where T : Attribute
        {
            foreach (var method in RocketAssembliesInfo.Assemblies
                .SelectMany(a => AccessTools.GetTypesFromAssembly(a))
                .SelectMany(t => AccessTools.GetDeclaredMethods(t))
                .Where(m => m.HasAttribute<T>())
                .ToArray())
            {
                Logger.Debug(string.Format("ROCKETMAN: Found function with attribute {0}, {1}:{2}", typeof(T).Name,
                    method.DeclaringType.Name, method.Name));
                yield return () => { return (P)method.Invoke(null, null); };
            }
        }

        public static IEnumerable<Func<P, K>> GetFunctions<T, P, K>() where T : Attribute
        {
            foreach (var method in RocketAssembliesInfo.Assemblies
                .SelectMany(a => AccessTools.GetTypesFromAssembly(a))
                .SelectMany(t => AccessTools.GetDeclaredMethods(t))
                .Where(m => m.HasAttribute<T>())
                .ToArray())
            {
                Logger.Debug(string.Format("ROCKETMAN: Found function with attribute {0}, {1}:{2}", typeof(T).Name,
                    method.DeclaringType.Name, method.Name));
                yield return (input) => (K)method.Invoke(null, new object[] { input });
            }
        }

        public static IEnumerable<Func<P, K, U>> GetFunctions<T, P, K, U>() where T : Attribute
        {
            foreach (var method in RocketAssembliesInfo.Assemblies
                .SelectMany(a => AccessTools.GetTypesFromAssembly(a))
                .SelectMany(t => AccessTools.GetDeclaredMethods(t))
                .Where(m => m.HasAttribute<T>())
                .ToArray())
            {
                Logger.Debug(string.Format("ROCKETMAN: Found function with attribute {0}, {1}:{2}", typeof(T).Name,
                    method.DeclaringType.Name, method.Name));
                yield return (input1, input2) => (U)method.Invoke(null, new object[] { input1, input2 });
            }
        }
    }
}