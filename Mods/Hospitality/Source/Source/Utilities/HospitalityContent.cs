using UnityEngine;
using Verse;

namespace Hospitality.Utilities
{
    [StaticConstructorOnStartup]
    public static class HospitalityContent
    {
        public static readonly Texture2D ButtonNumberUp = ContentFinder<Texture2D>.Get("UI/Commands/ButtonNumberUp");
        public static readonly Texture2D ButtonNumberDown = ContentFinder<Texture2D>.Get("UI/Commands/ButtonNumberDown");
        public static readonly Texture2D ButtonNumberAuto = ContentFinder<Texture2D>.Get("UI/Commands/ButtonNumberAuto");

    }
}
