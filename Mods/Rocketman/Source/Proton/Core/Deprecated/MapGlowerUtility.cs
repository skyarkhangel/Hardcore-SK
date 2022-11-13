using System;
using Verse;

namespace Proton
{
    public static class MapGlowerUtility
    {
        private const int _MaxMapNum = 40;

        private static readonly Map[] _maps = new Map[_MaxMapNum];
        private static readonly LitGlowerCacher[] _trackers = new LitGlowerCacher[_MaxMapNum];

        public static LitGlowerCacher GetGlowerCacher(this Map map)
        {
            int mapIndex = map.Index;
            if (mapIndex > _MaxMapNum || mapIndex < 0)
            {
                return null;
            }
            LitGlowerCacher tracker = _trackers[mapIndex];
            if (_maps[mapIndex] != map || tracker?.map != map)
            {
                _maps[mapIndex] = map;
                _trackers[mapIndex] = tracker = new LitGlowerCacher(map);
            }
            return tracker;
        }

        public static LitGlowerInfo GetInfo(this CompGlower comp)
        {
            return GetGlowerCacher(comp.parent.Map).InfoByComp.TryGetValue(comp, out LitGlowerInfo info) ? info : null;
        }
    }
}
