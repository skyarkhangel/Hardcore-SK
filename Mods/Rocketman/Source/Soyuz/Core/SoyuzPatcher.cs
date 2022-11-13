using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Soyuz
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SoyuzPatch : IPatch
    {
        public SoyuzPatch()
        {
        }

        public SoyuzPatch(Type targetType, string targetMethod = null, MethodType methodType = MethodType.Normal, Type[] parameters = null, Type[] generics = null) : base(targetType, targetMethod, methodType, parameters, generics)
        {
        }
    }

    public class SoyuzPatchInfo : IPatchInfo<SoyuzPatch>
    {
        public override string PluginName => "SOYUZ";
        public override string PatchTypeUniqueIdentifier => nameof(SoyuzPatch);

        public SoyuzPatchInfo(Type type) : base(type)
        {
        }
    }

    public class SoyuzPatcher
    {
        public static SoyuzPatchInfo[] patches = null;

        private readonly static Harmony harmony = new Harmony(Finder.HarmonyID + ".Soyuz");

        [Main.OnDefsLoaded]
        public static void PatchAll()
        {
            foreach (var patch in patches)
                patch.Patch(harmony);
            Log.Message($"SOYUZ: Patching finished");
            RocketEnvironmentInfo.SoyuzLoaded = true;
        }

        [Main.OnInitialization]
        public static void Intialize()
        {
            IEnumerable<Type> flaggedTypes = GetPatchTypes();
            LogTypesToFile(flaggedTypes);
            List<SoyuzPatchInfo> patchList = new List<SoyuzPatchInfo>();
            foreach (var type in flaggedTypes)
            {
                SoyuzPatchInfo patch = new SoyuzPatchInfo(type);
                patchList.Add(patch);
                if (RocketDebugPrefs.Debug) Log.Message($"SOYUZ: found patch in {type} and is {(patch.IsValid ? "valid" : "invalid") }");
            }
            patches = patchList.Where(p => p.IsValid).ToArray();
        }

        private static IEnumerable<Type> GetPatchTypes()
        {
            List<Type> types = new List<Type>();
            types.AddRange(AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()));
            types.AddRange(AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()).SelectMany(t => t.GetNestedTypes()));
            return types
                .Where(t => t.HasAttribute<SoyuzPatch>())
                .Distinct();
        }

        private static void LogTypesToFile(IEnumerable<Type> types)
        {
            string report = string.Empty;
            foreach (Type t in types)
            {
                report += t.FullName + "\n";
            }
            Logger.Debug(Assembly.GetExecutingAssembly().FullName, file: "Types.Soyuz.log");
            Logger.Debug(report, file: "Types.Soyuz.log");
        }
    }
}