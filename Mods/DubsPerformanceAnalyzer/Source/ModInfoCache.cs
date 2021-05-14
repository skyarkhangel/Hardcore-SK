using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace Analyzer
{
    public static class ModInfoCache
    {
        public static Dictionary<string, string> AssemblyToModname = new Dictionary<string, string>();

        public static void PopulateCache(string currentModName)
        {
            foreach (ModContentPack mod in LoadedModManager.RunningMods)
            {
                if (mod.Name == currentModName) continue;

                foreach (Assembly ass in mod.assemblies.loadedAssemblies)
                {
                    if (!AssemblyToModname.ContainsKey(ass.FullName))
                        AssemblyToModname.Add(ass.FullName, mod.Name);
                }
            }
        }

    }
}
