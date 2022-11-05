using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using RocketMan;
using UnityEngine;
using Verse;
using Verse.Noise;
using static RimWorld.FleshTypeDef;

namespace Proton
{
    public class GlowGridHelper
    {
        private ColorInt[] tempGrid1;
        private ColorInt[] tempGrid2;
        private List<Bounds> dirtyBounds = new List<Bounds>();
        private readonly HashSet<GlowerInfo> dirtyGlowers = new HashSet<GlowerInfo>();

        public Map map;        
        public Dictionary<CompGlower, GlowerInfo> litGlowers = new Dictionary<CompGlower, GlowerInfo>();

        public GlowGrid GlowGrid
        {
            get => map.glowGrid;
        }       

        public GlowGridHelper(Map map) 
        {
            this.map = map;
        }        

        public void Recalculate()
        {            
            if(tempGrid1 == null || tempGrid2 == null)
            {
                tempGrid1 = new ColorInt[map.cellIndices.NumGridCells];
                tempGrid2 = new ColorInt[map.cellIndices.NumGridCells];
                for (int i = 0; i < tempGrid1.Length; i++)
                {
                    tempGrid1[i] = new ColorInt(0, 0, 0, 0);
                    tempGrid2[i] = new ColorInt(0, 0, 0, 0);
                }
            }      
            foreach (var pair in litGlowers)
            {                
                Bounds bounds = pair.Value.Bounds;
                if (dirtyBounds.Any(b => b.Intersects(bounds)))
                {
                    dirtyGlowers.Add(pair.Value);
                }
            }
            GlowGrid grid = GlowGrid;
            CellIndices cellIndices = grid.map.cellIndices;            
            foreach (Bounds bounds in dirtyBounds)
            {
                Vector3 center = bounds.center;
                Vector3 size = bounds.size;
                int xMin = Math.Max(Mathf.FloorToInt(center.x - size.x / 2), 0);
                int xMax = Math.Min(Mathf.CeilToInt(center.x + size.x / 2), cellIndices.mapSizeX - 1);
                int zMin = Math.Max(Mathf.FloorToInt(center.z - size.z / 2), 0);
                int zMax = Math.Min(Mathf.CeilToInt(center.z + size.z / 2), cellIndices.mapSizeZ - 1);
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        int index = cellIndices.CellToIndex(x, z);                        
                        tempGrid1[index] = new ColorInt(0, 0, 0, 0);
                        tempGrid2[index] = new ColorInt(0, 0, 0, 0);
                    }
                }
            }
            foreach (GlowerInfo info in dirtyGlowers)
            {
                map.glowFlooder.AddFloodGlowFor(info.glower, tempGrid1);

                if (info.glower.parent.def.category != ThingCategory.Plant || !info.glower.parent.def.plant.cavePlant)
                {
                    map.glowFlooder.AddFloodGlowFor(info.glower, tempGrid2);
                }
            }
            foreach (Bounds bounds in dirtyBounds)
            {
                Vector3 center = bounds.center;
                Vector3 size = bounds.size;
                int xMin = Math.Max(Mathf.FloorToInt(center.x - size.x / 2), 0);
                int xMax = Math.Min(Mathf.CeilToInt(center.x + size.x / 2), cellIndices.mapSizeX - 1);
                int zMin = Math.Max(Mathf.FloorToInt(center.z - size.z / 2), 0);
                int zMax = Math.Min(Mathf.CeilToInt(center.z + size.z / 2), cellIndices.mapSizeZ - 1);
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        int index = cellIndices.CellToIndex(x, z);
                        grid.glowGrid[index] = tempGrid1[index];
                        grid.glowGridNoCavePlants[index] = tempGrid2[index];
                        if (RocketDebugPrefs.Debug && RocketDebugPrefs.DrawGlowerUpdates)
                        {
                            map.debugDrawer.FlashCell(new IntVec3(x, 0, z), 0.5f, "b", 120);
                        }
                    }
                }
            }
            Reset();
        }

        public void Register(CompGlower glower)
        {
            if (!litGlowers.TryGetValue(glower, out GlowerInfo info))
            {
                litGlowers[glower] = info = new GlowerInfo(glower, glower.parent.Position);                
            }
            PushMergeBounds(info.Bounds);
            dirtyGlowers.Add(info);
        }

        public void DeRegister(CompGlower glower)
        {
            if (!litGlowers.TryGetValue(glower, out GlowerInfo info))
            {
                info = new GlowerInfo(glower, glower.parent.Position);
            }
            else
            {
                litGlowers.Remove(glower);
                dirtyGlowers.Remove(info);
            }            
            PushMergeBounds(info.Bounds);            
        }        

        public void MarkPositionDirty(IntVec3 pos)
        {            
            float maxRadius = 0;
            Vector3 maxSize = Vector3.one * 2;
            pos.y = 0;
            foreach (var pair in litGlowers)
            {
                GlowerInfo info = pair.Value;
                if (info.Contains(pos) && info.glower.Props.glowRadius > maxRadius)
                {
                    maxRadius = info.glower.Props.glowRadius;
                    maxSize = info.Bounds.size;
                }
            }
            Bounds bounds = new Bounds(pos.ToVector3().Yto0(), maxSize);
            PushMergeBounds(bounds);
        }

        public void Reset()
        {
            dirtyGlowers.Clear();
            dirtyBounds.Clear();
        }        

        private void PushMergeBounds(Bounds bounds)
        {
            Bounds temp = bounds;
            bool foundIntersection = true;
            while (foundIntersection)
            {
                foundIntersection = false;
                foreach (Bounds b in dirtyBounds)
                {
                    if (b.Intersects(bounds))
                    {
                        temp = b;
                        foundIntersection = true;
                        break;
                    }
                }
                if (foundIntersection)
                {
                    dirtyBounds.Remove(temp);
                    Vector3 min = new Vector3(Mathf.Min(bounds.min.x, temp.min.x), -0.5f, Mathf.Min(bounds.min.z, temp.min.z));
                    Vector3 max = new Vector3(Mathf.Max(bounds.max.x, temp.max.x), 0.5f, Mathf.Max(bounds.max.z, temp.max.z));
                    bounds.SetMinMax(min, max);
                }
            }
            dirtyBounds.Add(bounds);
        }

        public class GlowerInfo
        {
            private Bounds bounds;
            
            public CompGlower glower;

            public Bounds Bounds
            {
                get => bounds;
            }

            public IntVec3 Position
            {
                get => bounds.center.ToIntVec3();
            }            

            public GlowerInfo(CompGlower glower, IntVec3 pos)
            {
                this.glower = glower;
                this.bounds = new Bounds(pos.ToVector3().Yto0(), Vector3.one * (glower.Props.glowRadius * 2 + 2));
            }

            public void Update()
            {
                this.bounds = new Bounds(glower.parent.Position.ToVector3().Yto0(), Vector3.one * (glower.Props.glowRadius * 2 + 2));
            }

            public bool Intersects(GlowerInfo other)
            {
                return other.bounds.Intersects(bounds);
            }

            public bool Contains(IntVec3 pos)
            {
                return bounds.Contains(pos.ToVector3());
            }
        }        
    }
}

