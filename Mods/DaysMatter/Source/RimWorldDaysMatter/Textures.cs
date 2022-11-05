using UnityEngine;
using Verse;

namespace RimWorldDaysMatter
{
    [StaticConstructorOnStartup]
    internal static class Textures
    {
        public static readonly Texture2D ROW_DELETE = ContentFinder<Texture2D>.Get("UI/Buttons/Abandon");
        public static readonly Texture2D DAY_MINUS = ContentFinder<Texture2D>.Get("UI/Buttons/Minus");
        public static readonly Texture2D DAY_PLUS = ContentFinder<Texture2D>.Get("UI/Buttons/Plus");
    }
}
