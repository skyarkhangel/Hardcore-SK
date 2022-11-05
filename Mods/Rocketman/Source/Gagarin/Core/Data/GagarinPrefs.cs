using System;
namespace Gagarin
{
    public static class GagarinPrefs
    {
        public static bool Enabled = false;

        public static int FilterMode = (int)UnityEngine.FilterMode.Trilinear;

        public static bool TextureCachingEnabled = false;

        public static float MipMapBias = float.MinValue;

        public static DateTime CacheCreationTime;
    }
}
