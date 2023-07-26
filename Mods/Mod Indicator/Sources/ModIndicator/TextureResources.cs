using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ModIndicator
{
    internal class TextureResources
    {
        public static readonly Texture2D ExclamationMarkIcon = ContentFinder<Texture2D>.Get("UI/Icons/ExclamationMarkIcon", true);
        public static readonly Texture2D TexButtonInfo = ContentFinder<Texture2D>.Get("UI/Icons/TexButtonInfo", true);
    }
}
