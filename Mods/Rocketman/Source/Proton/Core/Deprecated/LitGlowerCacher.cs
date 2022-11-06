using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using RocketMan;
using UnityEngine;
using Verse;

namespace Proton
{
    public class LitGlowerCacher
    {
        private bool initialized = false;

        private Color32[] buffer;

        private Color32 zeors = new Color32(0, 0, 0, 0);

        public readonly Map map;

        public readonly GlowGrid glowGrid;

        public readonly GlowFlooder flooder;

        public readonly List<LitGlowerInfo> AllLitGlowers = new List<LitGlowerInfo>();

        public readonly Dictionary<CompGlower, LitGlowerInfo> InfoByComp = new Dictionary<CompGlower, LitGlowerInfo>();

        public List<LitCell>[] grid;

        public List<LitCell>[] gridNoCavePlants;

        public FloodingMode FloodingMode = FloodingMode.none;

        public LitGlowerInfo CurrentFloodingGlower;

        public LitGlowerCacher(Map map)
        {
            this.map = map;
            this.glowGrid = map.glowGrid;
            this.flooder = map.glowFlooder;
            this.grid = new List<LitCell>[glowGrid.glowGrid.Length];

            for (int i = 0; i < this.grid.Length; i++)
                this.grid[i] = new List<LitCell>();

            this.buffer = new Color32[glowGrid.glowGrid.Length];

            for (int i = 0; i < this.grid.Length; i++)
                this.buffer[i] = new Color32(0, 0, 0, 0);
        }

        public void Register([NotNull] CompGlower comp)
        {
            if (!initialized)
            {
                Initialize();
            }
            if (comp == null)
            {
                throw new ArgumentNullException("Argument can't be null!");
            }
            if (InfoByComp.TryGetValue(comp, out LitGlowerInfo glowerInfo))
            {
                glowerInfo.Changed = true;
                Log.Warning("PROTON: Tried to regiseter an existing  CompGlower!");
                return;
            }
            AllLitGlowers.Add(InfoByComp[comp] = new LitGlowerInfo(comp));
        }

        public void DeRegister([NotNull] CompGlower comp)
        {
            if (!initialized)
            {
                Initialize();
            }
            if (comp == null)
            {
                throw new ArgumentNullException("Argument can't be null!");
            }
            if (!InfoByComp.TryGetValue(comp, out LitGlowerInfo glowerInfo))
            {
                Log.Warning("PROTON: Tried to deregister a not registered CompGlower!");
                return;
            }
            RemoveAllCells(glowerInfo);
            InfoByComp.Remove(comp);
            AllLitGlowers.Remove(glowerInfo);
        }

        public void Notify_DirtyAt(IntVec3 position)
        {
            if (!initialized)
            {
                Initialize();
            }
            foreach (LitGlowerInfo glowerInfo in AllLitGlowers.Where(g => g.Contains(position.ToVector3().Yto0())))
            {
                RemoveAllCells(glowerInfo);
                glowerInfo.Flooded = false;
            }
        }

        public void Recalculate()
        {
            if (!initialized)
            {
                Initialize();
            }
            foreach (LitGlowerInfo glowerInfo in AllLitGlowers.Where(g => !g.Flooded))
            {
                Flood(glowerInfo);
            }
        }

        private void Initialize()
        {
            int numGridCells = this.map.cellIndices.NumGridCells;
            for (int i = 0; i < numGridCells; i++)
            {
                this.glowGrid.glowGrid[i] = zeors;
                this.glowGrid.glowGridNoCavePlants[i] = zeors;
            }
            this.initialized = true;
        }

        private void Flood(LitGlowerInfo glowerInfo)
        {
            try
            {
                CurrentFloodingGlower = glowerInfo;
                CurrentFloodingGlower.Reset();

                FloodingMode = FloodingMode.normal;
                flooder.AddFloodGlowFor(glowerInfo.glower, buffer);

                CurrentFloodingGlower.Flooded = true;
                EmitCells(glowerInfo);
            }
            catch (Exception er)
            {
                RocketMan.Logger.Debug($"PROTON: Error while flooding {glowerInfo.glower.parent}", exception: er);
            }
            finally
            {
                CurrentFloodingGlower = null;
                FloodingMode = FloodingMode.none;
            }
        }

        private void RemoveAllCells(LitGlowerInfo glowerInfo)
        {
            foreach (LitCell cell in glowerInfo.AllGlowingCells)
            {
                grid[cell.index].RemoveAll(c => c.glowerInfo.glower == cell.glowerInfo.glower || c.glowerInfo.glower == cell.glowerInfo.glower);
                glowGrid.glowGrid[cell.index] = SumAt(cell.index);
                glowGrid.glowGridNoCavePlants[cell.index] = SumAtNoCavePlants(cell.index);
            }
            glowerInfo.Reset();
        }

        private void EmitCells(LitGlowerInfo glowerInfo)
        {
            foreach (LitCell cell in glowerInfo.AllGlowingCells)
            {
                glowGrid.glowGrid[cell.index] = SumAt(cell.index);
                if (RocketDebugPrefs.DrawGlowerUpdates)
                    map.debugDrawer.FlashCell(map.cellIndices.IndexToCell(cell.index), 0.1f, "_+_");
            }

            if (glowerInfo.FloodNoCavePlants)
            {
                foreach (LitCell cell in glowerInfo.AllGlowingCellsNoCavePlants)
                {
                    glowGrid.glowGridNoCavePlants[cell.index] = SumAtNoCavePlants(cell.index);
                    if (RocketDebugPrefs.DrawGlowerUpdates)
                        map.debugDrawer.FlashCell(map.cellIndices.IndexToCell(cell.index), 0.1f, "_+_");
                }
            }
        }

        private Color32 SumAt(int index)
        {
            ColorInt result = new ColorInt(0, 0, 0, 0);
            foreach (LitCell part in grid[index])
                result = part + result;
            return result.ToColor32;
        }

        private Color32 SumAtNoCavePlants(int index)
        {
            ColorInt result = new ColorInt(0, 0, 0, 0);
            foreach (LitCell part in grid[index].Where(c => c.glowerInfo.FloodNoCavePlants))
                result = (part + result).ToColor32.AsColorInt();
            return result.ToColor32;
        }
    }
}
