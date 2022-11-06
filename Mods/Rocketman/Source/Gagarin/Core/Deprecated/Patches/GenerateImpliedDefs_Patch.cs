using System;
using HarmonyLib;
using RimWorld;
using Verse;
using RocketMan;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace Gagarin
{

    public static class GenerateImpliedDefs_Patch
    {
        [Main.OnInitialization]
        public static void Start()
        {
            new GagarinPatchInfo(typeof(GenerateImpliedDefs_PostResolve_Patch)).Patch(Finder.Harmony);
        }

        [GagarinPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PostResolve))]
        public static class GenerateImpliedDefs_PostResolve_Patch
        {
            [HarmonyPriority(Priority.First)]
            public static void Postfix()
            {
                foreach (Type type in typeof(Def).AllSubclassesNonAbstract())
                {
                    foreach (Def def in GenDefDatabase.GetAllDefsInDatabaseForDef(type))
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
}
