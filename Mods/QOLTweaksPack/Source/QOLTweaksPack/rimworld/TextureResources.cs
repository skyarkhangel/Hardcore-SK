using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace QOLTweaksPack
{
    [StaticConstructorOnStartup]
    class TextureResources
    {
        public static readonly Texture2D drawPocket = ContentFinder<Texture2D>.Get("drawPocket", true);

        public static readonly Texture2D tradeStockpileOn = ContentFinder<Texture2D>.Get("tradeStockpileOn", true);
        public static readonly Texture2D tradeStockpileOff = ContentFinder<Texture2D>.Get("tradeStockpileOff", true);

        public static readonly Texture2D missingMedicine = ContentFinder<Texture2D>.Get("missingMedicine", true);
    }
}
