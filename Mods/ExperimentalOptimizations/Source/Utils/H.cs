using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ExperimentalOptimizations
{
    public static class H
    {
        public const string HarmonyName = "Experimental.Optimizations";
        public static readonly Harmony Harmony = new Harmony(HarmonyName);

        public class PatchInfo
        {
            private MethodInfo _method;
            public HarmonyMethod Prefix { get; }
            public HarmonyMethod Postfix { get; }
            public HarmonyMethod Transpiler { get; }

            public PatchInfo(MethodInfo method, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null, bool autoPatch = true)
            {
                _method = method;
                Prefix = prefix;
                Postfix = postfix;
                Transpiler = transpiler;

                if (autoPatch) Enable();
            }

            public bool Enable()
            {
                if (_method == null || (Prefix == null && Postfix == null && Transpiler == null)) return false;
                Harmony.Patch(_method, Prefix, Postfix, Transpiler);
#if DEBUG
                var activePatches = new[]
                    {
                        new {name = "prefix", patch = Prefix},
                        new {name = "postfix", patch = Postfix},
                        new {name = "transpiler", patch = Transpiler}
                    }.Where(p => p.patch != null)
                    .Select(p => $"{p.name}: {p.patch.method.Name}")
                    .ToArray();
                Log.Warning($"[PATCH] {_method.DeclaringType.FullName}:{_method.Name} => {String.Join(";", activePatches)}");
#endif
                return true;
            }

            public void Disable()
            {
                if (_method == null) return;
                int numPatches = new[] {Prefix, Postfix, Transpiler}.Count(p => p != null);
                if (numPatches == 0) return;
                _method.Unpatch(id: HarmonyName);
            }
        }

        public static void Unpatch(this MethodInfo method, HarmonyPatchType patchType = HarmonyPatchType.All, string id = null)
        {
            Harmony.Unpatch(method, patchType, id);
        }

        public static PatchInfo Patch(this MethodInfo method, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null, bool autoPatch = true)
        {
            if (method == null) return null;
            return new PatchInfo(method, prefix, postfix, transpiler, autoPatch);
        }

        public static void Patch(this MethodInfo method, ref List<PatchInfo> patchesList, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null, bool autoPatch = true)
        {
            if (method == null) return;
            patchesList.Add(new PatchInfo(method, prefix, postfix, transpiler, autoPatch));
        }

        public static MethodInfo Method(this string typeColonMethodname, Type[] parameters = null, Type[] generics = null, bool warn = true)
        {
            var method = AccessTools.Method(typeColonMethodname, parameters, generics);
            if (method == null)
            {
                if (warn) Log.Error($"Method not found: {typeColonMethodname}");
                return null;
            }
            return method;
        }

        public static MethodInfo Method(this Type type, string methodName, Type[] parameters = null, Type[] generics = null, bool warn = true)
        {
            var method = AccessTools.Method(type, methodName, parameters, generics);
            if (method == null)
            {
                if (warn) Log.Error($"Method not found: {type.FullName}:{methodName}");
                return null;
            }
            return method;
        }

        public static HarmonyMethod HarmonyMethod(this Type type, string methodName, Type[] parameters = null, Type[] generics = null, int priority = Priority.Normal, string[] before = null, string[] after = null, bool? debug = null, bool warn = true)
        {
            var method = type.Method(methodName, parameters, generics, warn);
            if (method == null)
                return null;
            return method.ToHarmonyMethod(priority, before, after, debug);
        }

        public static HarmonyMethod HarmonyMethod(this string typeColonMethodname, Type[] parameters = null, Type[] generics = null, int priority = Priority.Normal, string[] before = null, string[] after = null, bool? debug = null, bool warn = true)
        {
            var method = typeColonMethodname.Method(parameters, generics, warn);
            if (method == null)
                return null;
            return method.ToHarmonyMethod(priority, before, after, debug);
        }

        public static HarmonyMethod ToHarmonyMethod(this MethodInfo method, int priority = Priority.Normal, string[] before = null, string[] after = null, bool? debug = null)
        {
            return new HarmonyMethod(method, priority, before, after, debug);
        }

        public static T InvokeStatic<T>(this MethodInfo method, params object[] args)
        {
            return (T) method.Invoke(null, args);
        }

        public static T InvokeStaticMethod<T>(this Type type, string methodName, params object[] args)
        {
            return (T) type.Method(methodName).Invoke(null, args);
        }

        public static void InvokeStatic(this MethodInfo method, params object[] args)
        {
            method.Invoke(null, args);
        }

        public static void InvokeStaticMethod(this Type type, string methodName, params object[] args)
        {
            type.Method(methodName).Invoke(null, args);
        }
    }
}