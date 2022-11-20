using System.Collections.Generic;
using Verse;

namespace yayoAni.Data
{
    public static class DataUtility
    {
        public static readonly Dictionary<Pawn, PawnDrawData> DrawDataDictionary = new();

        public static PawnDrawData GetData(Pawn key)
        {
            if (DrawDataDictionary.TryGetValue(key, out var data))
                return data;
            
            data = new PawnDrawData();
            DrawDataDictionary[key] = data;
            return data;
        }

        public static void Remove(Pawn key)
        {
            DrawDataDictionary.Remove(key);
        }

        public static void GC()
        {
            int prev = DrawDataDictionary.Keys.Count;
            DrawDataDictionary.RemoveAll(a => a.Key?.Map == null);
            Log.Message($"GC : animation data count [{prev} -> {DrawDataDictionary.Keys.Count}]");
        }

        public static void Reset()
        {
            int prev = DrawDataDictionary.Keys.Count;
            DrawDataDictionary.Clear();
            Log.Message($"GC : animation data count [{prev} -> {DrawDataDictionary.Keys.Count}]");
        }
    }
}
