using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Analyzer.Performance
{

    internal class H_FixWallsNConduits : PerfPatch
    {
        public static bool Enabled = false;
        public override string Name => "performance.wallsnconduits";

        public static void Swapclasses()
        {
            void felch(string s, DrawerType t)
            {
                var d = DefDatabase<ThingDef>.GetNamed(s, false);
                if (d != null)
                {
                    d.drawerType = t;
                }
            }

            if (Enabled)
            {
                felch("PowerConduit", DrawerType.MapMeshOnly);
                felch("WaterproofConduit", DrawerType.MapMeshOnly);
                felch("Wall", DrawerType.MapMeshOnly);
            }
            else
            {
                felch("PowerConduit", DrawerType.MapMeshAndRealTime);
                felch("WaterproofConduit", DrawerType.MapMeshAndRealTime);
                felch("Wall", DrawerType.MapMeshAndRealTime);
            }

            if (Find.Maps == null) return;

            foreach (var map in Find.Maps)
            {
                void reg(string s)
                {
                    var d = DefDatabase<ThingDef>.GetNamed(s, false);
                    if (d == null)
                    {
                        return;
                    }
                    if (map.listerThings.listsByDef.ContainsKey(d))
                    {
                        foreach (var def in map.listerThings.listsByDef[d])
                        {
                            map.dynamicDrawManager.RegisterDrawable(def);
                        }
                    }
                }

                void dereg(string s)
                {
                    var d = DefDatabase<ThingDef>.GetNamed(s, false);
                    if (d == null)
                    {
                        return;
                    }
                    if (map.listerThings.listsByDef.ContainsKey(d))
                    {
                        foreach (var def in map.listerThings.listsByDef[d])
                        {
                            map.dynamicDrawManager.DeRegisterDrawable(def);
                        }
                    }
                }

                if (Enabled)
                {
                    dereg("PowerConduit");
                    dereg("WaterproofConduit");
                    dereg("Wall");
                }
                else
                {
                    reg("PowerConduit");
                    reg("WaterproofConduit");
                    reg("Wall");
                }
            }
        }

        public override void OnEnabled(Harmony harmony)
        {
            Swapclasses();
        }
    }
}