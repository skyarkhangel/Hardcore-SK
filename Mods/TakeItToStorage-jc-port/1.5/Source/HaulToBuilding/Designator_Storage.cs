using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;

namespace HaulToBuilding;

public class Designator_Storage : Designator
{
    private readonly Bill_Production bill;
    private readonly ExtraBillData extraData;
    private bool dragging;
    private Mode mode;
    private Vector2 windowPos;

    public Designator_Storage(Bill_Production bill)
    {
        this.bill = bill;
        icon = TexStorage.StorageSelection;
        extraData = GameComponent_ExtraBillData.Instance.GetData(bill);
        windowPos = HaulToBuildingMod.Settings.Pos;
        useMouseIcon = true;
    }

    public override int DraggableDimensions => 2;

    private Vector2 WindowSize => new(300f,
        188 + (bill.repeatMode == BillRepeatModeDefOf.TargetCount ? 88 : 0) + extraData.TakeFrom.Count * 32);

    public override void Deselected()
    {
        base.Deselected();
        HaulToBuildingMod.Settings.Pos = windowPos;
        HaulToBuildingMod.Instance.WriteSettings();
        Find.Selector.Select(bill?.billStack?.billGiver, true, false);
        InspectPaneUtility.OpenTab(typeof(ITab_Bills));
    }

    private void DoWindow(Event ev)
    {
        Find.WindowStack.ImmediateWindow(2145132, new Rect(windowPos, WindowSize), WindowLayer.GameUI, delegate
        {
            var rect = new Rect(Vector2.zero, WindowSize).ContractedBy(10f);
            var listing = new Listing_Standard();
            listing.Begin(rect);
            var section = listing.BeginSection(64 + extraData.TakeFrom.Count * 32);
            if (section.ButtonText(mode == Mode.TakeFrom ? "HaulToBuilding.Designate.TakeFrom".Translate() : extraData.TakeFromText())) mode = Mode.TakeFrom;

            foreach (var parent in extraData.TakeFrom.ToList())
                if (section.ButtonText($"{parent.SlotYielderLabel()} ({"HaulToBuilding.Designate.Remove".Translate()})"))
                    extraData.TakeFrom.Remove(parent);

            if (section.ButtonText("HaulToBuilding.Clear".Translate()))
            {
                extraData.TakeFrom.Clear();
                GameComponent_ExtraBillData.Instance.SetData(bill, extraData);
            }

            listing.EndSection(section);
            listing.Gap(16);

            section = listing.BeginSection(64);
            if (section.ButtonText(mode == Mode.TakeTo ? "HaulToBuilding.Designate.TakeTo".Translate() : Utils.TakeToText(bill, extraData, out _)))
                mode = Mode.TakeTo;

            if (section.ButtonText("HaulToBuilding.Clear".Translate()))
            {
                bill.SetStoreMode(BillStoreModeDefOf.DropOnFloor);
                extraData.Storage = null;
                GameComponent_ExtraBillData.Instance.SetData(bill, extraData);
            }

            listing.EndSection(section);

            if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
            {
                listing.Gap(16);
                section = listing.BeginSection(64f);
                if (section.ButtonText(mode == Mode.LookIn ? "HaulToBuilding.Designate.LookIn".Translate() : bill.LookInText(extraData))) mode = Mode.LookIn;

                if (section.ButtonText("HaulToBuilding.Clear".Translate()))
                {
                    bill.includeGroup = null;
                    extraData.LookInStorage = null;
                    GameComponent_ExtraBillData.Instance.SetData(bill, extraData);
                }

                listing.EndSection(section);
            }

            listing.End();

            if (ev.isMouse)
                switch (ev.type)
                {
                    case EventType.MouseDown:
                        dragging = true;
                        ev.Use();
                        return;
                    case EventType.MouseUp when dragging:
                        dragging = false;
                        HaulToBuildingMod.Settings.Pos.x = windowPos.x;
                        HaulToBuildingMod.Settings.Pos.y = windowPos.y;
                        ev.Use();
                        break;
                    case EventType.MouseDrag when dragging:
                        windowPos += Event.current.delta;
                        ev.Use();
                        break;
                }
        });
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        if (c.GetSlotGroup(Map) is { parent: var parent }) Designate(parent);
    }

    public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
    {
        foreach (var c in cells)
            if (c.GetSlotGroup(Map) is { parent: { } parent })
                Designate(parent);
    }

    public override void DrawMouseAttachments()
    {
        base.DrawMouseAttachments();
        DoWindow(Event.current);
    }

    public override void RenderHighlight(List<IntVec3> dragCells)
    {
        DesignatorUtility.RenderHighlightOverSelectableThings(this, dragCells);
        DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
    }

    public override void DesignateThing(Thing t)
    {
        if (t is ISlotGroupParent parent) Designate(parent);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Designate(ISlotGroupParent parent)
    {
        switch (mode)
        {
            case Mode.TakeFrom:
                if (extraData.TakeFrom.Contains(parent)) break;
                extraData.TakeFrom.Add(parent);
                GameComponent_ExtraBillData.Instance.SetData(bill, extraData);
                break;
            case Mode.LookIn:
                switch (parent)
                {
                    case Zone_Stockpile stockpile:
                        bill.includeGroup = stockpile.slotGroup;
                        break;
                    case Building_Storage building:
                        extraData.LookInStorage = building;
                        GameComponent_ExtraBillData.Instance.SetData(bill, extraData);
                        break;
                }

                mode = Mode.None;
                break;
            case Mode.TakeTo:
                switch (parent)
                {
                    case Zone_Stockpile stockpile:
                        bill.SetStoreMode(BillStoreModeDefOf.SpecificStockpile,
                            stockpile.slotGroup);
                        break;
                    case Building_Storage building:
                        bill.SetStoreMode(HaulToBuildingDefOf.StorageBuilding);
                        extraData.Storage = building;
                        GameComponent_ExtraBillData.Instance.SetData(bill, extraData);
                        break;
                }

                mode = Mode.None;
                break;
        }
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 loc) => loc.GetSlotGroup(Map)?.parent is { } parent ? CanDesignate(parent) : false;

    public override AcceptanceReport CanDesignateThing(Thing t) => t is ISlotGroupParent parent ? CanDesignate(parent) : false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AcceptanceReport CanDesignate(ISlotGroupParent parent) => mode switch
    {
        Mode.TakeFrom => parent.ValidTakeFrom(bill) ? true : new AcceptanceReport("IncompatibleLower".Translate()),
        Mode.LookIn => parent.ValidLookIn(bill) ? true : new AcceptanceReport("IncompatibleLower".Translate()),
        Mode.TakeTo => parent.ValidTakeTo(bill) ? true : new AcceptanceReport("IncompatibleLower".Translate()),
        _ => false
    };

    private enum Mode
    {
        None,
        TakeFrom,
        LookIn,
        TakeTo
    }
}