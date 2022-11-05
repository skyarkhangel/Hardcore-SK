using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RocketMan
{
    public static class RocketAssembliesInfo
    {
        private static string versionString;

        private static HashSet<Assembly> assemblies = new HashSet<Assembly>();

        public static readonly string[] ApprovedDependencies = new string[]
        {
            "System.Numerics.Vectors.dll",
            "BCnEncoderNet47.dll",
            "XmlDiffPatch.dll",
            "XmlDiffPatch.View.dll",
        };

        public static string Version
        {
            get
            {
                if (versionString != null)
                    return versionString;
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                versionString = $"{version.Major}" +
                    $".{version.Minor}" +
                    $".{version.Build}" +
                    $".{version.Revision}";
                return versionString;
            }
        }

        public static HashSet<Assembly> Assemblies
        {
            get
            {
                Assembly mainAssembly = Assembly.GetExecutingAssembly();
                if (!assemblies.Contains(mainAssembly))
                    assemblies.Add(mainAssembly);
                return assemblies;
            }
        }

        public static IEnumerable<Assembly> RocketManAssembliesInAppDomain
        {
            get => AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(a => a.FullName is string name && name != null && (name.StartsWith("Proton") || name.StartsWith("Cosmodrome") || name.StartsWith("Gagarin") || name.StartsWith("Soyuz")))
                    .Distinct();
        }
    }
}
