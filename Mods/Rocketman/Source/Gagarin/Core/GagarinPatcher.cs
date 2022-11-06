using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Gagarin
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class GagarinPatch : IPatch
    {
        public GagarinPatch()
        {
        }

        public GagarinPatch(Type targetType, string targetMethod = null, MethodType methodType = MethodType.Normal, Type[] parameters = null, Type[] generics = null) : base(targetType, targetMethod, methodType, parameters, generics)
        {
        }
    }

    public class GagarinPatchInfo : IPatchInfo<GagarinPatch>
    {
        public override string PluginName => "Gagarin";
        public override string PatchTypeUniqueIdentifier => nameof(GagarinPatch);

        public GagarinPatchInfo(Type type) : base(type)
        {
        }

        public override void OnPatchingSuccessful(MethodBase replacement)
        {
            base.OnPatchingSuccessful(replacement);
            if (RocketDebugPrefs.Debug) Log.Message($"GAGARIN: Patched {replacement}");
        }

        public override void OnPatchingFailed(Exception er)
        {
            base.OnPatchingFailed(er);
            Log.Error($"GAGARIN: Patching failed! {DeclaringType}");
        }
    }

    public class GagarinPatcher
    {
        public static GagarinPatchInfo[] patches = null;

        public readonly static Harmony harmony = new Harmony(Finder.HarmonyID + ".Gagarin");

        public static void PatchAll()
        {
            IEnumerable<Type> types = GetPatchTypes();
            LogTypesToFile(types);
            foreach (var type in types)
            {
                new GagarinPatchInfo(type).Patch(harmony);
            }
            Log.Message($"GAGARIN: Patching finished");
            RocketEnvironmentInfo.GagarinLoaded = true;
        }

        private static IEnumerable<Type> GetPatchTypes()
        {
            List<Type> types = new List<Type>();
            types.AddRange(AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()));
            types.AddRange(AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()).SelectMany(t => t.GetNestedTypes()));
            return types
                .Where(t => t.HasAttribute<GagarinPatch>())
                .Distinct();
        }

        private static void LogTypesToFile(IEnumerable<Type> types)
        {
            string report = string.Empty;
            foreach (Type t in types)
            {
                report += t.FullName + "\n";
            }
            Logger.Debug(Assembly.GetExecutingAssembly().FullName, file: "Types.Gagarin.log");
            Logger.Debug(report, file: "Types.Gagarin.log");
        }
    }
}
