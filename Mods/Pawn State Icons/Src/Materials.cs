using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorldIconMod
{
    public enum Icons
    {
        None = 0,
        Aggressive,
        Bloodloss,
        Dazed,
        Disease,
        Draft,
        Effectiveness,
        Freezing,
        Hot,
        Hungry,
        Idle,
        Leave,
        Naked,
        Panic,
        Sad,
        Target,
        Tired,
        Unarmed,
        Drunk,
        ApparelHealth,
        Pacific,
        Length
    };

    class Materials
    {
        private Material[] data = new Material[(int)Icons.Length];
        public readonly string matLibName;

        public Materials(string matLib = "default")
        {
            matLibName = matLib;
        }

        public Material loadIconMat(String path, bool smooth = false/*, int size = 0*/)
        {
            
            Texture2D tex = ContentFinder<Texture2D>.Get("UI/Overlays/PawnStateIcons/" + path, false);
            if (tex == null)
            {
                //Log.Message("Texture not found: " + "UI/Overlays/PawnStateIcons/" + path);
                return null;
            }
            
            if (smooth)
            {
                //Texture2D tex2 = new Texture2D(tex.width, tex.height, tex.format, true, true);
                //Texture2D tex2 = tex;
                tex.filterMode = FilterMode.Trilinear;
                tex.mipMapBias = -0.50f;

                tex.anisoLevel = 9;
                tex.wrapMode = TextureWrapMode.Repeat;

                                
                //tex2.SetPixels(tex.GetPixels(0),0);
                tex.Apply();


                tex.Compress(true);
              //  tex2.Compress(true);
                //tex = tex2;
            }
            else
            {
                //Texture2D tex2 = new Texture2D(tex.width, tex.height, tex.format, false,false);
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Repeat;

                //tex2.SetPixels(tex.GetPixels());
                tex.Apply();

                tex.Compress(true);
              //  tex2.Compress(true);
                //tex = tex2;
            }
           // else if (size > 0) TextureScale.Bilinear(tex, size, size);

            return MaterialPool.MatFrom(new MaterialRequest(tex, ShaderDatabase.MetaOverlay));
        }


        public Material this[Icons icon]
        {
            get { return data[(int)icon]; }
        }

        public void reloadTextures(bool smooth = false)
        {
            foreach (Icons ic in Enum.GetValues(typeof(Icons)).Cast<Icons>())
            {
                if (ic == Icons.None || ic == Icons.Length) continue;
                string path = matLibName + "/" + Enum.GetName(typeof(Icons), ic);
                data[(int)ic] = loadIconMat(path, smooth);
            }
        }

    }
}
