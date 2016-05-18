using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace PSI
{
    public enum Icons
    {
        None,
        Aggressive,
        Bloodloss,
        Dazed,
        Sickness,
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
        Prosthophile,
        Prosthophobe,
        NightOwl,
        Greedy,
        Jealous,
        Love,
        Health,
        MedicalAttention,
        LeftUnburied,
        DeadColonist,
        Crowded,
        Pain,
        Bedroom,
        Toxic,
        Marriage,
        TargetHair,
        TargetSkin,
        Length
    }

    internal class Materials
    {
        private readonly Material[] _data = new Material[40];
        private readonly string _matLibName;

        public Material this[Icons icon] => _data[(int)icon];

        public Materials(string matLib = "default")
        {
            _matLibName = matLib;
        }

        private Material LoadIconMat(string path, bool smooth = false)
        {
            var tex = ContentFinder<Texture2D>.Get("UI/Overlays/PawnStateIcons/" + path, false);
            Material material;
            if (tex == null)
            {
                material = null;
            }
            else
            {
                if (smooth)
                {
                    tex.filterMode = FilterMode.Trilinear;
                    tex.mipMapBias = -0.5f;
                    tex.anisoLevel = 9;
                    tex.wrapMode = TextureWrapMode.Repeat;
                    tex.Apply();
                    tex.Compress(true);
                }
                else
                {
                    tex.filterMode = FilterMode.Point;
                    tex.wrapMode = TextureWrapMode.Repeat;
                    tex.Apply();
                    tex.Compress(true);
                }
                material = MaterialPool.MatFrom(new MaterialRequest(tex, ShaderDatabase.MetaOverlay));
            }
            return material;
        }

        public void ReloadTextures(bool smooth = false)
        {
            foreach (var icons in Enum.GetValues(typeof(Icons)).Cast<Icons>())
            {
                switch (icons)
                {
                    case Icons.None:
                    case Icons.Length:
                        continue;
                    default:
                        var path = _matLibName + "/" + Enum.GetName(typeof(Icons), icons);
                        _data[(int)icons] = LoadIconMat(path, smooth);
                        continue;
                }
            }
        }
    }
}
