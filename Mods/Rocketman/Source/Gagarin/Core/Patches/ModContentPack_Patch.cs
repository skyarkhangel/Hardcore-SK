using System;
using System.Collections.Generic;
using System.Linq;
using RocketMan;
using Verse;

namespace Gagarin
{
    public static class ModContentPack_Patch
    {
        [GagarinPatch(typeof(ModContentPack), nameof(ModContentPack.LoadDefs))]
        public class ModContentPack_LoadDefs_Patch
        {
            public static void Prefix(ModContentPack __instance)
            {
                Context.CurrentLoadingMod = __instance;
            }

            public static void Postfix(ModContentPack __instance)
            {
                Context.CurrentLoadingMod = null;

                CheckPatches(__instance);
            }

            private static void CheckPatches(ModContentPack mod)
            {
                Context.IsLoadingPatchXML = true;
                Context.CurrentLoadingMod = mod;
                Exception error = null;
                try
                {
                    DirectXmlLoader.XmlAssetsInModFolder(mod, "Patches/").ToList();
                }
                catch (Exception er)
                {
                    error = er;
                }
                finally
                {
                    Context.IsLoadingPatchXML = false;
                    Context.CurrentLoadingMod = null;
                }
                if (error != null)
                {
                    throw error;
                }
            }
        }
    }
}
