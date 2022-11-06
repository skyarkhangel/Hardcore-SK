using System;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Gagarin
{
    public static class GenGeneric_Patch
    {
        [Main.OnInitialization]
        public static void Start()
        {
            new GagarinPatchInfo(typeof(GenGeneric_InvokeStaticMethodOnGenericType_Patch)).Patch(Finder.Harmony);
        }

        [GagarinPatch(typeof(GenGeneric), nameof(GenGeneric.InvokeStaticMethodOnGenericType), parameters = new[] { typeof(Type), typeof(Type), typeof(string) })]
        public static class GenGeneric_InvokeStaticMethodOnGenericType_Patch
        {
            [HarmonyPriority(Priority.First)]
            public static void Postfix(Type genericBase, Type genericParam, string methodName)
            {
                if (genericBase != typeof(DefDatabase<>))
                {
                    return;
                }
                if (!genericParam.IsSubclassOf(typeof(Def)))
                {
                    return;
                }
                if (methodName != "AddAllInMods")
                {
                    return;
                }
                foreach (Def def in GenDefDatabase.GetAllDefsInDatabaseForDef(genericParam))
                {
                    if (def.modContentPack != null)
                    {
                        continue;
                    }
                    // TODO 
                    // Do somthing                        
                    def.modContentPack = Context.core;
                }
            }
        }
    }
}
