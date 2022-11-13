using System;
using System.Collections.Generic;
using Verse;

namespace Gagarin
{
    public static class Context
    {
        public static ModContentPack core;

        public static List<Def> defs = new List<Def>();

        public static List<ModContentPack> runningMods = new List<ModContentPack>();

        public static Dictionary<string, int> packageIdLoadIndexlookup = new Dictionary<string, int>();

        public static Dictionary<string, List<LoadableXmlAsset>> assetPackageIdlookup = new Dictionary<string, List<LoadableXmlAsset>>();
    }
}
