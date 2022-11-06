using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Gagarin
{
    public static class LoadableXmlAsset_Constructor_Patch
    {
        [Main.OnInitialization]
        public static void Start()
        {
            Finder.Harmony.Patch(AccessTools.Constructor(typeof(LoadableXmlAsset), new[] { typeof(string), typeof(string), typeof(string) }),
                postfix: new HarmonyMethod(
                    AccessTools.Method(typeof(LoadableXmlAsset_Constructor_Patch), nameof(LoadableXmlAsset_Constructor_Patch.Postfix))));
        }

        public static void Postfix(LoadableXmlAsset __instance, string contents)
        {
            if (Context.IsLoadingModXML || Context.IsLoadingPatchXML)
            {
                try
                {
                    UInt64 current = CalculateHash(contents);
                    string id = __instance.GetLoadableId();

                    lock (Context.AssetsHashes)
                    {
                        if (!Context.AssetsHashes.TryGetValue(id, out UInt64 old) || current != old)
                        {
                            try
                            {
                                if (GagarinEnvironmentInfo.CacheExists && Context.IsUsingCache)
                                {
                                    string message = Context.IsLoadingPatchXML ? "Patches changed!" : "Asset changed!";

                                    Log.Warning($"GAGARIN: {message}" +
                                        $"<color=red>{__instance.name}</color>:<color=red>{Context.CurrentLoadingMod?.PackageId ?? "Unknown"}</color> " +
                                        $"in {__instance.fullFolderPath}");
                                }
                            }
                            finally
                            {
                                Context.IsUsingCache = false;
                            }
                        }
                        Context.AssetsHashes[id] = current;
                    }
                }
                catch (Exception er)
                {
                    Context.IsUsingCache = false;
                    Logger.Debug("GAGARIN: Failed in LoadableXmlAsset", exception: er);
                }
            }
        }

        private static UInt64 CalculateHash(string text)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            if (!text.NullOrEmpty())
            {
                for (int i = 0; i < text.Length; i++)
                {
                    hashedValue += text[i];
                    hashedValue *= 3074457345618258799ul;
                }
            }
            return hashedValue;
        }
    }
}
