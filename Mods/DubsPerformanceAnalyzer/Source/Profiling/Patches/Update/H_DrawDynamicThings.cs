using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.update.dynamicdraw", Category.Update)]
    internal class H_DrawDynamicThings
    {
        public static bool Active = false;

        [Setting("By Def")]
        public static bool ByDef = false;

        public static void ProfilePatch()
        {
            Modbase.Harmony.Patch(AccessTools.Method(typeof(DynamicDrawManager), nameof(DynamicDrawManager.DrawDynamicThings)), prefix: new HarmonyMethod(typeof(H_DrawDynamicThings), "Prefix"));
        }

        public static bool Prefix(MethodBase __originalMethod, DynamicDrawManager __instance)
        {
            if (!Active)
            {
                return true;
            }

            __instance.drawingNow = true;
            try
            {
                bool[] fogGrid = __instance.map.fogGrid.fogGrid;
                CellRect cellRect = Find.CameraDriver.CurrentViewRect;
                cellRect.ClipInsideMap(__instance.map);
                cellRect = cellRect.ExpandedBy(1);
                CellIndices cellIndices = __instance.map.cellIndices;
                foreach (Thing thing in __instance.drawThings)
                {
                    IntVec3 position = thing.Position;
                    if (cellRect.Contains(position) || thing.def.drawOffscreen)
                    {
                        if (!fogGrid[cellIndices.CellToIndex(position)] || thing.def.seeThroughFog)
                        {
                            if (thing.def.hideAtSnowDepth >= 1f || __instance.map.snowGrid.GetDepth(thing.Position) <=
                                thing.def.hideAtSnowDepth)
                            {
                                try
                                {
                                    string Namer()
                                    {
                                        string n = string.Empty;
                                        if (ByDef)
                                        {
                                            n = $"{thing.def.defName} - {thing?.def?.modContentPack?.Name}";
                                        }
                                        else
                                        {
                                            n = thing.GetType().ToString();
                                        }

                                        return n;
                                    }

                                    string name = string.Empty;
                                    if (ByDef)
                                    {
                                        name = thing.def.defName;
                                    }
                                    else
                                    {
                                        name = thing.GetType().Name;
                                    }

                                    var prof = ProfileController.Start(name, Namer, thing.GetType(), thing.def, null, __originalMethod);
                                    thing.Draw();
                                    prof.Stop();
                                }
                                catch (Exception ex)
                                {
                                    Log.Error(string.Concat("Exception drawing ", thing, ": ", ex.ToString()));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception arg)
            {
                Log.Error("Exception drawing dynamic things: " + arg);
            }

            __instance.drawingNow = false;
            return false;
        }
    }
}