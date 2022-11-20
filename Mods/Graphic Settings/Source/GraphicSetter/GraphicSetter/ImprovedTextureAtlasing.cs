using System;
using UnityEngine;
using Verse;

namespace GraphicSetter
{
    public enum AntiAliasing : byte
    {
        None = 0,
        TwoX = 2,
        FourX = 4,
        EightX = 8,
    }

    [Obsolete]
    public static class ImprovedTextureAtlasing
    {
        private const int BaseRes = PawnTextureAtlas.AtlasSize;

        [Obsolete]
        public static RenderTexture CreatePawnRenderTex(int referenceWidth = BaseRes, int referenceHeight = BaseRes)
        {
            var settings = GraphicsSettings.mainSettings;
            RenderTexture tex = new(referenceWidth * settings.pawnTexResScale, referenceHeight * settings.pawnTexResScale, 24, RenderTextureFormat.ARGB32);
            /*
            if (settings.useAntiA)
            {
                tex.antiAliasing = (int) settings.antiALevel;
            }
            */
            if (settings.useMipMap)
            {
                tex.useMipMap = true;
                tex.mipMapBias = settings.mipMapBias;
            }

            tex.filterMode = settings.filterMode;
            tex.anisoLevel = settings.anisoLevel;
            return tex;
        }

        [Obsolete]
        public static Texture2D MakeReadableTextureInstance_Fixed(Texture2D source)
        {
            RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            temporary.name = "MakeReadableTexture_Temp";
            Graphics.Blit(source, temporary);
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = temporary;
            Texture2D texture2D = new Texture2D(source.width, source.height);
            texture2D.ReadPixels(new Rect(0f, 0f, temporary.width, temporary.height), 0, 0);

            //Apply Settings
            var settings = GraphicsSettings.mainSettings;

            texture2D.filterMode = settings.filterMode;
            texture2D.anisoLevel = settings.anisoLevel;
            texture2D.mipMapBias = settings.mipMapBias;
            texture2D.Apply(true);

            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(temporary);
            return texture2D;
        }
    }
}
