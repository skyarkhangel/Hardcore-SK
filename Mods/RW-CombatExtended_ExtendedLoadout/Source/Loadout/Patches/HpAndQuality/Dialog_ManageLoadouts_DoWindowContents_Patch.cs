using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended.ExtendedLoadout;

/// <summary>
/// Draw HP and Quality ranges
/// </summary>
[HarmonyPatch(typeof(Dialog_ManageLoadouts), nameof(Dialog_ManageLoadouts.DoWindowContents))]
[HotSwappable]
public class Dialog_ManageLoadouts_DoWindowContents_Patch
{
    private static readonly float allowedMaxYIncrease = 100f;
    static bool Prepare() => ExtendedLoadoutMod.Instance.useHpAndQualityInLoadouts;

    [HarmonyTranspiler]
    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> DoWindowContents_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var drawSlotList = AccessTools.Method(typeof(Dialog_ManageLoadouts), nameof(Dialog_ManageLoadouts.DrawSlotList));

        bool heightFixed = false;
        bool drawHpQualityInjected = false;
        foreach (var ci in instructions)
        {
            if (!heightFixed && ci.opcode == OpCodes.Ldc_R4 && (float)ci.operand == 48f)
            {
                // decrease slotListRect height
                ci.operand = 160f; // canvas3..ctor(0f, canvas2.yMax + 6f, (canvas.width - 6f) / 2f, canvas.height - 30f - canvas2.height - 48f - 30f);
                yield return ci;
                heightFixed = true;
            }
#pragma warning disable 252,253
            else if (heightFixed && !drawHpQualityInjected && ci.opcode == OpCodes.Call && ci.operand == drawSlotList)
#pragma warning restore 252,253
            {
                // draw after DrawSlotList(slotListRect);
                yield return ci;
                yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                yield return new CodeInstruction(OpCodes.Ldloc_S, 10); // local: bulkBarRect
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Dialog_ManageLoadouts_DoWindowContents_Patch), nameof(DrawHpQuality)));
                drawHpQualityInjected = true;
            }
            else
            {
                yield return ci;
            }
        }
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (!drawHpQualityInjected || !heightFixed)
        {
            Log.Error($"drawHpQualityInjected = {drawHpQualityInjected}; heightFixed = {heightFixed}");
        }
    }

    public static void DrawHpQuality(Dialog_ManageLoadouts dialog, Rect bulkBarRect)
    {
        Rect refillRect = new(bulkBarRect.xMin, bulkBarRect.yMax + 36f, bulkBarRect.width, Dialog_ManageLoadouts._barHeight);
        Rect hpRect = new(refillRect.xMin, refillRect.yMax + Dialog_ManageLoadouts._margin, refillRect.width, Dialog_ManageLoadouts._barHeight);
        Rect qualityRect = new(hpRect.xMin, hpRect.yMax + Dialog_ManageLoadouts._margin, hpRect.width, Dialog_ManageLoadouts._barHeight);
        var loadoutExtended = dialog.CurrentLoadout.Extended();

        GUI.color = (Color)AccessTools.Field(typeof(Widgets), "RangeControlTextColor").GetValue(new Color());

        loadoutExtended.RefillThreshold = Widgets.HorizontalSlider_NewTemp(refillRect, loadoutExtended.RefillThreshold, 0f, 1f, false, "CE_Extended.RefillThreshold".Translate(Mathf.RoundToInt(loadoutExtended.RefillThreshold * 100)), null, null, -1f);
        GUI.color = Color.white;
        
        Widgets.FloatRange(hpRect, 976833333, ref loadoutExtended.HpRange, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
        
        Widgets.QualityRange(qualityRect, 976833334, ref loadoutExtended.QualityRange);
    }
}