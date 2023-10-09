using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout;

[HarmonyPatch(typeof(Dialog_ManageLoadouts))]
public class Dialog_ManageLoadouts_DrawSlotSelection_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DrawSlotSelection))]
    public static bool DrawSlotSelection(Dialog_ManageLoadouts __instance, Rect canvas)
    {
        const float rowHeight = 22f; //Dialog_ManageLoadouts._rowHeight = 28f

        /*
        FieldInfo sourceTypeInfo = typeof(Dialog_ManageLoadouts).GetField("_sourceType", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo sourceInfo = typeof(Dialog_ManageLoadouts).GetField("_source", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo sourceGenericInfo = typeof(Dialog_ManageLoadouts).GetField("_sourceGeneric", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo darkBackgroundInfo = typeof(Dialog_ManageLoadouts).GetField("_darkBackground", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo availableScrollPositionInfo = typeof(Dialog_ManageLoadouts).GetField("_availableScrollPosition", BindingFlags.NonPublic | BindingFlags.Instance);
        SourceSelection sourceType = (SourceSelection)sourceTypeInfo.GetValue(new SourceSelection());
        */
        int count = __instance._sourceType == SourceSelection.Generic ? __instance._sourceGeneric.Count : __instance._source.Count;
        GUI.DrawTexture(canvas, Dialog_ManageLoadouts._darkBackground);

        if ((__instance._sourceType != SourceSelection.Generic && __instance._source.NullOrEmpty()) || (__instance._sourceType == SourceSelection.Generic && __instance._sourceGeneric.NullOrEmpty()))
            return false;

        Rect viewRect = new Rect(canvas);
        viewRect.width -= 16f;
        viewRect.height = count * rowHeight;

        Widgets.BeginScrollView(canvas, ref __instance._availableScrollPosition, viewRect.AtZero());
        int startRow = (int)Math.Floor((decimal)(__instance._availableScrollPosition.y / rowHeight));
        startRow = (startRow < 0) ? 0 : startRow;
        int endRow = startRow + (int)(Math.Ceiling((decimal)(canvas.height / rowHeight)));
        endRow = (endRow > count) ? count : endRow;
        for (int i = startRow; i < endRow; i++)
        {
            // gray out weapons not in stock
            Color baseColor = GUI.color;

            Rect row = new Rect(0f, i * rowHeight, canvas.width, rowHeight);
            Rect labelRect = new Rect(row);
            if (__instance._sourceType == SourceSelection.Generic)
                TooltipHandler.TipRegion(row, __instance._sourceGeneric[i].GetWeightAndBulkTip());
            else
                TooltipHandler.TipRegion(row, __instance._source[i].thingDef.GetWeightAndBulkTip() /*DescriptionDetailed*/);

            labelRect.xMin += 6f; // _margin is now private const.
            if (i % 2 == 0)
                GUI.DrawTexture(row, Dialog_ManageLoadouts._darkBackground);

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;
            if (__instance._sourceType == SourceSelection.Generic) {
                if (__instance.GetVisibleGeneric(__instance._sourceGeneric[i]))
                    GUI.color = Color.gray;

                Widgets.Label(labelRect, __instance._sourceGeneric[i].LabelCap);
            } else {
                var descRect = row.LeftPart(0.90f);
                var def = __instance._source[i].thingDef;
                Widgets.DefIcon(descRect.LeftPart(.10f), def);

                if (__instance._source[i].isGreyedOut) // DefIcon reset GUI.color, so set after
                    GUI.color = Color.gray;

                Widgets.Label(descRect.RightPart(.85f), def.LabelCap);
                if (Widgets.ButtonImageFitted(row.RightPart(0.15f).ContractedBy(2f), Verse.TexButton.Info)) {
                    var stuff = def.MadeFromStuff ? GenStuff.AllowedStuffsFor(def).First() : null;
                    Find.WindowStack.Add(new Dialog_InfoCard(def, stuff));
                }
            }
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.DrawHighlightIfMouseover(row);
            if (Widgets.ButtonInvisible(row))
            {
                if (__instance._sourceType == SourceSelection.Generic)
                    AddLoadoutSlotGeneric(__instance.CurrentLoadout, __instance._sourceGeneric[i]);
                else
                    AddLoadoutSlotSpecific(__instance.CurrentLoadout, __instance._source[i].thingDef);
            }
            // revert to original color
            GUI.color = baseColor;
        }
        Widgets.EndScrollView();

        return false;

        void AddLoadoutSlotGeneric(Loadout loadout, LoadoutGenericDef generic) => loadout.AddSlot(new LoadoutSlot(generic));
        void AddLoadoutSlotSpecific(Loadout loadout, ThingDef def, int count = 1) => loadout.AddSlot(new LoadoutSlot(def, count));
    }
}