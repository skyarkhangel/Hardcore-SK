using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RocketMan
{
    public static class IncompatibilityHelper
    {
        private static List<ModMetaData> metaDatabase = new List<ModMetaData>();

        private struct ModMetaData
        {
            public string name;
            public string packageId;
        }

        public static List<string> incompatibleMods = new List<string>();

        public static void Register(string name, string packageId)
        {
            metaDatabase.Add(new ModMetaData()
            {
                name = name,
                packageId = packageId
            });
        }

        public static void Prepare()
        {
            foreach (ModMetaData info in metaDatabase)
            {
                if (ModsConfig.ActiveModsInLoadOrder
                    .Any(m => (m.Name.ToLower() == info.name || m.PackageIdPlayerFacing.ToLower() == info.packageId || m.PackageIdNonUnique.ToLower() == info.packageId)))
                {
                    incompatibleMods.Add(info.name);
                    Log.Warning($"ROCKETMAN: Detected {info.name} ({info.packageId}) which is an incompatible mod!");
                }
            }
            if (incompatibleMods.Count > 0)
            {
                RocketEnvironmentInfo.IncompatibilityUnresolved = true;
                Log.Warning("ROCKETMAN: Incompatiblity found!");
            }
        }
    }
}
