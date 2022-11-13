using System;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RocketMan;
using UnityEngine;
using UnityEngine.Assertions;
using Verse;

namespace Proton
{
    public static class GlowFlooder_Patch
    {
        private static LitGlowerInfo current;

        private static LitGlowerCacher cacher;

        private static List<LitCell> cells;

        private static List<LitCell>[] cellGrid;

        private static void Finilize()
        {
            cacher = null;

            current = null;

            cellGrid = null;

            cells = null;
        }

        [ProtonPatch(typeof(GlowFlooder), nameof(GlowFlooder.AddFloodGlowFor))]
        public static class AddFloodGlowFor_Patch
        {
            public static void Prefix(GlowFlooder __instance, CompGlower theGlower)
            {
                cacher = __instance.map.GetGlowerCacher();
                current = cacher.CurrentFloodingGlower;
                switch (cacher.FloodingMode)
                {
                    case FloodingMode.normal:
                        cells = current.AllGlowingCells;
                        cellGrid = cacher.grid;
                        break;
                }
            }

            public static void Postfix()
            {
                Finilize();
            }
        }

        [ProtonPatch(typeof(GlowFlooder), nameof(GlowFlooder.SetGlowGridFromDist))]
        public static class SetGlowGridFromDist_Patch
        {
            private static int index;

            private static MethodBase mClampToNonNegative = AccessTools.Method(typeof(ColorInt), nameof(ColorInt.ClampToNonNegative));

            public static bool Prefix(int index)
            {
                Assert.IsNotNull(cacher);
                Assert.IsNotNull(current);
                SetGlowGridFromDist_Patch.index = index;
                return true;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = instructions.ToList();
                for (var i = 0; i < codes.Count - 1; i++)
                    yield return codes[i];

                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Ldloc_1);
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SetGlowGridFromDist_Patch), nameof(SetGlowGridFromDist_Patch.AddCell)));
                yield return codes.Last();
            }

            private static void AddCell(int index, ColorInt colorInt, float distance)
            {
                if (colorInt.r > 0 || colorInt.g > 0 || colorInt.b > 0)
                {
                    if (cellGrid[index].Any(g => g.glowerInfo == current))
                        throw new InvalidProgramException("PROTON: Tried to doulbe add a cell");

                    if (RocketDebugPrefs.DrawGlowerUpdates)
                        cacher.map.debugDrawer.FlashCell(cacher.map.cellIndices.IndexToCell(index), 0.1f, "0__");

                    LitCell cell = new LitCell(glowerInfo: current, colorInt: colorInt, index: index, distance: distance);
                    cellGrid[index].Add(cell);
                    cells.Add(cell);
                }
            }
        }
    }
}
