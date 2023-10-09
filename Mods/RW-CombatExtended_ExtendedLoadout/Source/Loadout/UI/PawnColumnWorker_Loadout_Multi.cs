using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout;

/// <summary>
/// Replaced pawn.GetLoadout() => pawn.GetLoadout().Loadout2, and SetLoadout2
/// </summary>
[HotSwappable]
public class PawnColumnWorker_Loadout_Multi : PawnColumnWorker_Loadout
{
    private static Dictionary<string, ThingDef> _firstGenericCache = new();

    static int GetSlotCountClamped(Loadout loadout, int max) => (loadout.Slots.Count < 0) ? 0 : (loadout.Slots.Count > max) ? max : loadout.Slots.Count;

    private int GetIndexFromDefName(string defName)
    {
        return int.Parse(defName.Split('_')[1]);
    }

    protected IEnumerable<Widgets.DropdownMenuElement<Loadout>> Btn_GenerateMenu(Pawn pawn)
    {
        const int elementHeight = 25;
        using List<Loadout>.Enumerator enu = LoadoutManager.Loadouts.GetEnumerator();
            
        while (enu.MoveNext())
        {
            Loadout loadout = enu.Current;
            int slotCount = GetSlotCountClamped(loadout, 5); // clamp 0:5
            yield return new Widgets.DropdownMenuElement<Loadout>
            {
                option = new FloatMenuOption(loadout.LabelCap, delegate
                {
                    pawn.SetLoadout(loadout, GetIndexFromDefName(def.defName));
                }, extraPartWidth: elementHeight * slotCount, extraPartOnGUI: rect => {
                    var iconsRect = rect.RightPartPixels((elementHeight) * slotCount);
                    for (int i = 0; i < slotCount; i++) {
                        Rect iconRect = new(iconsRect) { width = elementHeight, height = elementHeight };
                        iconRect.x += i * (elementHeight);
                        var slot = loadout.Slots[i];
                        ThingDef def;
                        if (slot.genericDef != null) {
                            if (!_firstGenericCache.TryGetValue(slot.genericDef.defName, out def)) {
                                def = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => slot.genericDef.lambda(x));
                                _firstGenericCache.Add(slot.genericDef.defName, def);
                            }
                        }
                        else def = slot.thingDef;
                        Widgets.DefIcon(iconRect, def);

                        if (def != null && Mouse.IsOver(iconRect)) {
                            TooltipHandler.TipRegion(iconRect, def.DescriptionDetailed);
                            Widgets.DrawHighlight(iconRect);
                        }
                    }
                    return false;
                }),
                payload = loadout
            };
        }
    }

    public override void DoHeader(Rect rect, PawnTable table)
    {
        if (GetIndexFromDefName(def.defName) == 0)
        {
            base.DoHeader(rect, table);
            return;
        }

        // dont call base.DoHeader(rect, table) (PawnColumnWorker_Loadout.DoHeader), because he draw button ManageLoadouts
        // instead draw vanilla base code
        if (!def.label.NullOrEmpty())
        {
            Text.Font = DefaultHeaderFont;
            GUI.color = DefaultHeaderColor;
            Text.Anchor = TextAnchor.LowerCenter;
            var rect2 = rect;
            rect2.y += 3f;
            Widgets.Label(rect2, def.LabelCap.Resolve().Truncate(rect.width));
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
        }
        else if (def.HeaderIcon != null)
        {
            var headerIconSize = def.HeaderIconSize;
            var num = (int)((rect.width - headerIconSize.x) / 2f);
            GUI.DrawTexture(new Rect(rect.x + num, rect.yMax - headerIconSize.y, headerIconSize.x, headerIconSize.y).ContractedBy(2f), def.HeaderIcon);
        }

        // Yup they have been made private static readonly with no way to access them.

        if (table.SortingBy == def)
        {
            var texture2D = table.SortingDescending ? ContentFinder<Texture2D>.Get("UI/Icons/SortingDescending") : ContentFinder<Texture2D>.Get("UI/Icons/Sorting");
            GUI.DrawTexture(new Rect(rect.xMax - texture2D.width - 1f, rect.yMax - texture2D.height - 1f, texture2D.width, texture2D.height), texture2D);
        }

        if (def.HeaderInteractable)
        {
            var interactableHeaderRect = GetInteractableHeaderRect(rect, table);
            if (Mouse.IsOver(interactableHeaderRect))
            {
                Widgets.DrawHighlight(interactableHeaderRect);
                var headerTip = GetHeaderTip(table);
                if (!headerTip.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(interactableHeaderRect, headerTip);
                }
            }

            if (Widgets.ButtonInvisible(interactableHeaderRect))
            {
                HeaderClicked(rect, table);
            }
        }
    }

    public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
    {
        if (pawn.outfits == null)
        {
            return;
        }

        int index = GetIndexFromDefName(def.defName);

        //changed: int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
        //changed: int num = Mathf.FloorToInt((rect.width - 4f) - IconSize);
        int num = Mathf.FloorToInt((rect.width - 4f) - 16f);
        //changed: int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
        //changed: Mathf.FloorToInt(IconSize);
        int num2 = Mathf.FloorToInt(16f);
        float num3 = rect.x;
        //added:
        float num4 = rect.y + ((rect.height - 16f) / 2);

        // Reduce width if we're adding a clear forced button
        Rect loadoutRect = new(num3, rect.y + 2f, num, rect.height - 4f);

        if (GetIndexFromDefName(def.defName) == 0)
        {
            int personalIconSize = 24;
            Rect personalLoadoutRect = new(loadoutRect.x, loadoutRect.y, personalIconSize, personalIconSize);
            loadoutRect.x += 4f + personalIconSize;
            loadoutRect.width -= 4f + personalIconSize;
            num3 += 4f + personalIconSize;

            if (Widgets.ButtonImage(personalLoadoutRect, Textures.PersonalLoadout))
            {
                Find.WindowStack.Add(new Dialog_ManageLoadouts_Extended(pawn, (pawn.GetLoadout() as Loadout_Multi)!.PersonalLoadout));
            }
            TooltipHandler.TipRegion(personalLoadoutRect, new TipSignal("CE_Extended.PersonalLoadoutTip".Translate(), pawn.GetHashCode() * 6178));
        }

        // Main loadout button
#if DEBUG
        DbgLog.Msg($"Pawn:{pawn.Name}");
        DbgLog.Msg($"Loadout:{pawn.GetLoadout().GetUniqueLoadID()}");
        DbgLog.Msg($"Loadout as Multi:{(pawn.GetLoadout() as Loadout_Multi)?.uniqueID}");
        DbgLog.Msg($"Current index:{index}, loadout:{(pawn.GetLoadout() as Loadout_Multi)?[index].label}");

#endif
        string label = (pawn.GetLoadout() as Loadout_Multi)![index].label.Truncate(loadoutRect.width);
        Widgets.Dropdown(loadoutRect, pawn, p => (p.GetLoadout() as Loadout_Multi)![index], Btn_GenerateMenu, label, null, null, null, null, true);

        // Clear forced button
        num3 += loadoutRect.width;
        num3 += 4f;

        //changed: Rect assignTabRect = new Rect(num3, rect.y + 2f, (float)num2, rect.height - 4f);
        Rect assignTabRect = new(num3, num4, num2, num2);
        //changed: if (Widgets.ButtonText(assignTabRect, "AssignTabEdit".Translate(), true, false, true))
        if (Widgets.ButtonImage(assignTabRect, EditImage))
        {
            Find.WindowStack.Add(new Dialog_ManageLoadouts_Extended((pawn.GetLoadout() as Loadout_Multi)![index]));
        }
        // Added this next line.
        TooltipHandler.TipRegion(assignTabRect, new TipSignal("CE_Loadouts".Translate(), pawn.GetHashCode() * 613));
        num3 += num2;
    }
}