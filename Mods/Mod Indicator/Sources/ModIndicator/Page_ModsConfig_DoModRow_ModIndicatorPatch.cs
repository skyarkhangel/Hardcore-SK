using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ModIndicator
{
    [HarmonyPatch(typeof(Page_ModsConfig))]
    [HarmonyPatch("DoModRow")]
    public class Page_ModsConfig_DoModRow_ModIndicatorPatch
    {
        private static readonly ModListerSettingsDef modListerSettingsDef = ModListerSettingsDefOfLocal.ModListerSettingsDef;

        private static Dictionary<string, ModIndicator> idByIndicator;
        private static void Postfix(ref Rect r, ModMetaData mod, List<ModMetaData> list, ref int index, ref bool isDragged)
        {
            if (idByIndicator == null)
            {
                idByIndicator = new Dictionary<string, ModIndicator>();
            }

            Rect statusRect = new Rect(r.x + r.width - r.height, r.y, r.height, r.height);
            statusRect = statusRect.ScaledBy(0.5f);

            string tip = $"{ModTypeDefOfLocal.UnknownCompatibility.LabelCap}\n\n{ModTypeDefOfLocal.UnknownCompatibility.description}";
            Color color = ModTypeDefOfLocal.UnknownCompatibility.color;

            if (!idByIndicator.TryGetValue(mod.PackageId, out ModIndicator modIndicator))
            {
                idByIndicator.Add(mod.PackageId, modListerSettingsDef.modIndicators.Find(x => x.mod.ToLower() == mod.PackageId.ToLower()));
            }

            if (modIndicator != null)
            {
                color = modIndicator.modTypeDef.color;
                tip = $"{modIndicator.modTypeDef.LabelCap}\n\n{modIndicator.modTypeDef.description}";
            }

            Widgets.DrawBoxSolid(statusRect, color);
            if (Mouse.IsOver(r))
            {
                TooltipHandler.TipRegion(r, tip);
            }
        }
    }
}
