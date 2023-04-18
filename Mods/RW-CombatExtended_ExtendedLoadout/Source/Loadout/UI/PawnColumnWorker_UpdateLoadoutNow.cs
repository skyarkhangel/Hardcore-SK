using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended.ExtendedLoadout;

[HotSwappable]
public class PawnColumnWorker_UpdateLoadoutNow : PawnColumnWorker
{
    private const int TopAreaHeight = 65;

    private const int ManageOutfitsButtonHeight = 32;

    // Transpiler referenced items should be changed with extreme caution.  Values can be changed but visibility, type, and name should not be.
    #region TranspilerReferencedItems
    internal const float _MinWidth = 158f;  //194f default
    internal const float _OptimalWidth = 188;  //251f default

    internal static float IconSize = 16f;
    // using property format since I don't know what the lambda expression '=>' gets compiled into in this context.
    public static Texture2D ClearImage => ContentFinder<Texture2D>.Get("UI/Icons/clear");

    #endregion TranspilerReferencedItems

    private static void HoldTrackerClear(Pawn pawn) => HoldTrackerClear(pawn);

    private static void UpdateLoadoutNow(Pawn pawn)
    {
        Job? job = pawn.thinker?.GetMainTreeThinkNode<JobGiver_UpdateLoadout>()?.TryGiveJob(pawn);
        if (job != null)
        {
            pawn.jobs.StartJob(job, JobCondition.InterruptForced);
            if (pawn.mindState != null)
                pawn.mindState.nextApparelOptimizeTick = -99999;
        }
    }

    public override void DoHeader(Rect rect, PawnTable table)
    {
        base.DoHeader(rect, table);
        Rect rect2 = new Rect(rect.x, rect.y + (rect.height - TopAreaHeight), Mathf.Min(rect.width, 360f), ManageOutfitsButtonHeight);
        if (Widgets.ButtonText(rect2, "CE_UpdateLoadoutNow".Translate(), true, false, true))
        {
            foreach (var pawn in Find.CurrentMap?.mapPawns?.AllPawnsSpawned ?? Enumerable.Empty<Pawn>())
            {
                UpdateLoadoutNow(pawn);
            }
        }
        UIHighlighter.HighlightOpportunity(rect2, "CE_UpdateLoadoutNow");
    }

    /* (ProfoundDarkness) I've intentionally left some code remarked in the following code because it's a useful guide on how to create
     * and maintain the transpilers that will do nearly identical changes to RimWorld's code for the other 2 PawnColumnWorkers.
     */
    public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
    {
        if (pawn.outfits == null)
        {
            return;
        }
        //changed: int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
        int num = Mathf.FloorToInt((rect.width - 4f) - IconSize);
        //changed: int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
        int num2 = Mathf.FloorToInt(IconSize);
        float num3 = rect.x;
        //added:
        float num4 = rect.y + ((rect.height - IconSize) / 2);
        float equipNowWidth = "CE_UpdateLoadoutNow".Translate().GetWidthCached();

        // Reduce width if we're adding a clear forced button
        List<HoldRecord> holdRecords = LoadoutManager.GetHoldRecords(pawn);
        bool somethingIsForced = holdRecords != null && !holdRecords.NullOrEmpty<HoldRecord>() && holdRecords.Any<HoldRecord>((Predicate<HoldRecord>)(r => r.pickedUp));
        Rect loadoutRect = new Rect(num3, rect.y + 2f, (float)num, rect.height - 4f);

        if (pawn.Spawned)
        {
            Rect forceEquipNow = loadoutRect;
            if (Widgets.ButtonText(forceEquipNow, "CE_UpdateLoadoutNow".Translate()))
            {
                UpdateLoadoutNow(pawn);
            }
        }

        // Clear forced button
        num3 += loadoutRect.width;
        num3 += 4f;
        //changed: Rect forcedHoldRect = new Rect(num3, rect.y + 2f, (float)num2, rect.height - 4f);
        Rect forcedHoldRect = new Rect(num3, num4, (float)num2, (float)num2);
        if (somethingIsForced)
        {
            if (Widgets.ButtonImage(forcedHoldRect, ClearImage))
            {
                //Directly calling the main method since we do not support multiplayer mod. But still don't know why the multiplayer one is privatized while this one doesn't
                pawn.HoldTrackerClear(); // yes this will also delete records that haven't been picked up and thus not shown to the player...
            }
            TooltipHandler.TipRegion(forcedHoldRect, new TipSignal(delegate
            {
                string text = "CE_ForcedHold".Translate() + ":\n";
                foreach (HoldRecord rec in LoadoutManager.GetHoldRecords(pawn))
                {
                    if (!rec.pickedUp) continue;
                    text = text + "\n   " + rec.thingDef.LabelCap + " x" + rec.count;
                }
                return text;
            }, pawn.GetHashCode() * 613));
            num3 += (float)num2;
            num3 += 4f;
        }
    }

    public override int GetMinWidth(PawnTable table)
    {
        return Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(_MinWidth));
    }

    public override int GetOptimalWidth(PawnTable table)
    {
        return Mathf.Clamp(Mathf.CeilToInt(_OptimalWidth), this.GetMinWidth(table), this.GetMaxWidth(table));
    }

    public override int GetMinHeaderHeight(PawnTable table)
    {
        return Mathf.Max(base.GetMinHeaderHeight(table), TopAreaHeight);
    }

    public override int Compare(Pawn a, Pawn b)
    {
        return a.GetLoadoutId().CompareTo(b.GetLoadoutId());
    }
}