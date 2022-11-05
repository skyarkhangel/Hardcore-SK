using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using MapGlowerProp = System.Collections.Generic.Dictionary<Verse.CompGlower, RocketMan.Optimizations.GlowGrid_Patch.GlowerProperties>;
using MapGlowerPropHashSet = System.Collections.Generic.HashSet<RocketMan.Optimizations.GlowGrid_Patch.GlowerProperties>;

namespace RocketMan.Optimizations
{
    public static class GlowGrid_Patch
    {
        public sealed class GlowerProperties
        {
            private bool drawn = false;

            private CompGlower glower;

            public HashSet<int> Indices = new HashSet<int>();

            public Vector3 Postion
            {
                get => glower?.parent?.TrueCenter().Yto0() ?? Vector3.negativeInfinity;
            }

            public bool ShouldBeLitNow
            {
                get => glower?.ShouldBeLitNow ?? false;
            }

            public bool Drawn
            {
                get => drawn;
                set
                {
                    drawn = value;
                }
            }

            public CompGlower Glower
            {
                get => glower;
            }

            public GlowerProperties(CompGlower glower)
            {
                this.glower = glower;
            }

            public void Reset()
            {
                Indices.Clear();
            }

            public void AddCellIndex(int index)
            {
                Indices.Add(index);
            }
        }

        public static readonly MapGlowerProp Props = new MapGlowerProp();


        [RocketPatch(typeof(GlowGrid), nameof(GlowGrid.RegisterGlower))]
        internal static class RegisterGlower_Patch
        {
            public static void Prefix()
            {

            }
        }
        //private static readonly Map[] _map = new Map[20];
        //private static readonly MapGlowerProp[] _props = new MapGlowerProp[20];
        //private static readonly MapGlowerPropHashSet[] _changed = new MapGlowerPropHashSet[20];
        //private static readonly MapGlowerPropHashSet[] _removed = new MapGlowerPropHashSet[20];

        //private static int GetMapIndex(Map map)
        //{
        //    int mapIndex = map.Index;
        //    if (_props[mapIndex] == null || _map[mapIndex] != map)
        //    {
        //        _map[mapIndex] = map;
        //        _props[mapIndex] = new MapGlowerProp();
        //        _changed[mapIndex] = new MapGlowerPropHashSet();
        //        _removed[mapIndex] = new MapGlowerPropHashSet();
        //    }
        //    return map.Index;
        //}

        //public static GlowerProperties GetGlowerProperties(this CompGlower comp)
        //{
        //    if (comp.parent?.Destroyed ?? true)
        //        return null;
        //    int mapIndex = GetMapIndex(comp.parent.Map);
        //    return _props[mapIndex].TryGetValue(comp, out GlowerProperties prop)
        //        ? prop : _props[mapIndex][comp] = new GlowerProperties(comp);
        //}

        //public static void RemoveGlowerProperties(this CompGlower comp)
        //{
        //    if (comp.parent?.Map == null)
        //        return;
        //    int mapIndex = GetMapIndex(comp.parent.Map);
        //    if (_props[mapIndex].ContainsKey(comp))
        //        _props[mapIndex].Remove(comp);
        //}

        //public static IEnumerable<GlowerProperties> GetAllGlowersProperties(Map map)
        //{
        //    int mapIndex = GetMapIndex(map);
        //    return _props[mapIndex].Values;
        //}

        //public static MapGlowerPropHashSet GetChangedGlowerPropSet(Map map)
        //{
        //    int mapIndex = GetMapIndex(map);
        //    return _changed[mapIndex];
        //}

        //public static MapGlowerPropHashSet GetRemovedGlowerPropSet(Map map)
        //{
        //    int mapIndex = GetMapIndex(map);
        //    return _removed[mapIndex];
        //}

        //public 
    }
}
