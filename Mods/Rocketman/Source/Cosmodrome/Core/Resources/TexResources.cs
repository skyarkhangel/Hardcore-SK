using System;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public static class TexTab
    {
        public static readonly Texture2D Settings = ContentFinder<Texture2D>.Get("RocketMan/UI/gear_icon", true);

        public static readonly Texture2D Alerts = ContentFinder<Texture2D>.Get("RocketMan/UI/bell_icon", true);

        public static readonly Texture2D Dilation = ContentFinder<Texture2D>.Get("RocketMan/UI/clock_icon", true);

        public static readonly Texture2D Stats = ContentFinder<Texture2D>.Get("RocketMan/UI/stat_icon", true);

        public static readonly Texture2D Debug = ContentFinder<Texture2D>.Get("RocketMan/UI/debug_icon", true);

        public static readonly Texture2D World = ContentFinder<Texture2D>.Get("RocketMan/UI/world_icon", true);

        public static readonly Texture2D Graphing = ContentFinder<Texture2D>.Get("RocketMan/UI/graph_icon", true);

        public static readonly Texture2D Gagarin = ContentFinder<Texture2D>.Get("RocketMan/UI/gagarin_gear", true);
    }
}
