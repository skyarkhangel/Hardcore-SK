using RimWorld;
using UnityEngine;
using Verse;
using StorageSelector.UI.Logging;

namespace StorageSelector.UI.Controls
{
    public static class RepeatModeControls
    {
        public static void BuildRepeatModeSection(Dialog_BillConfig dialog, Bill_Production bill, Listing_Standard listingStandard, float height, float gapBefore, float gapAfter)
        {
            try
            {
                if (dialog == null)
                {
                    UILogger.LogControlError("repeat mode section", "null dialog", new System.ArgumentNullException(nameof(dialog)));
                    return;
                }

                if (bill == null)
                {
                    UILogger.LogControlError("repeat mode section", "null bill", new System.ArgumentNullException(nameof(bill)));
                    return;
                }

                if (listingStandard == null)
                {
                    UILogger.LogControlError("repeat mode section", "null listing", new System.ArgumentNullException(nameof(listingStandard)));
                    return;
                }

                var listingStandard1 = listingStandard.BeginSection(height, gapBefore, gapAfter);

                try
                {
                    if (listingStandard1.ButtonText(bill.repeatMode.LabelCap))
                    {
                        try
                        {
                            BillRepeatModeUtility.MakeConfigFloatMenu(bill);
                        }
                        catch (System.Exception e)
                        {
                            UILogger.LogControlError("repeat mode menu", "showing", e);
                        }
                    }
                    listingStandard1.Gap(12f);

                    if (bill.repeatMode == BillRepeatModeDefOf.RepeatCount)
                    {
                        BuildRepeatCountSection(dialog, bill, listingStandard1);
                    }
                    else if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
                    {
                        BuildTargetCountSection(dialog, bill, listingStandard1);
                    }
                }
                catch (System.Exception e)
                {
                    UILogger.LogControlError("repeat mode controls", "building", e);
                }
                finally
                {
                    listingStandard.EndSection(listingStandard1);
                }
            }
            catch (System.Exception e)
            {
                UILogger.LogControlError("repeat mode section", "initializing", e);
            }
        }

        private static void BuildRepeatCountSection(Dialog_BillConfig dialog, Bill_Production bill, Listing_Standard listing)
        {
            try
            {
                listing.Label("RepeatCount".Translate(bill.repeatCount));
                string buffer = dialog.GetType().GetField("repeatCountEditBuffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(dialog) as string;
                if (buffer != null)
                {
                    listing.IntEntry(ref bill.repeatCount, ref buffer, 1);
                }
                else
                {
                    UILogger.LogWarning("Repeat count buffer not found");
                }
            }
            catch (System.Exception e)
            {
                UILogger.LogControlError("repeat count section", "building", e);
            }
        }

        private static void BuildTargetCountSection(Dialog_BillConfig dialog, Bill_Production bill, Listing_Standard listing)
        {
            try
            {
                BuildTargetCountInfo(bill, listing);
                BuildTargetCountEntry(dialog, bill, listing);
                BuildProductOptionsSection(bill, listing);
                BuildPauseSection(dialog, bill, listing);
            }
            catch (System.Exception e)
            {
                UILogger.LogControlError("target count section", "building", e);
            }
        }

        private static void BuildTargetCountInfo(Bill_Production bill, Listing_Standard listing)
        {
            try
            {
                string text = "CurrentlyHave".Translate() + ": ";
                text += bill.recipe.WorkerCounter.CountProducts(bill);
                text += " / ";
                text += bill.targetCount >= 999999 ? "Infinite".Translate().ToLower() : bill.targetCount.ToString();

                string productsDesc = bill.recipe.WorkerCounter.ProductsDescription(bill);
                if (!productsDesc.NullOrEmpty())
                {
                    text += "\n" + "CountingProducts".Translate() + ": " + productsDesc.CapitalizeFirst();
                }

                listing.Label(text);
            }
            catch (System.Exception e)
            {
                UILogger.LogControlError("target count info", "building", e);
            }
        }

        private static void BuildTargetCountEntry(Dialog_BillConfig dialog, Bill_Production bill, Listing_Standard listing)
        {
            try
            {
                string buffer = dialog.GetType().GetField("targetCountEditBuffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(dialog) as string;
                if (buffer != null)
                {
                    int oldCount = bill.targetCount;
                    listing.IntEntry(ref bill.targetCount, ref buffer, bill.recipe.targetCountAdjustment);
                    bill.unpauseWhenYouHave = Mathf.Max(0, bill.unpauseWhenYouHave + (bill.targetCount - oldCount));
                }
                else
                {
                    UILogger.LogWarning("Target count buffer not found");
                }
            }
            catch (System.Exception e)
            {
                UILogger.LogControlError("target count entry", "building", e);
            }
        }

        private static void BuildProductOptionsSection(Bill_Production bill, Listing_Standard listing)
        {
            try
            {
                ThingDef producedThingDef = bill.recipe.ProducedThingDef;
                if (producedThingDef != null)
                {
                    if (producedThingDef.IsWeapon || producedThingDef.IsApparel)
                    {
                        listing.CheckboxLabeled("IncludeEquipped".Translate(), ref bill.includeEquipped);
                    }
                    if (producedThingDef.IsApparel && producedThingDef.apparel.careIfWornByCorpse)
                    {
                        listing.CheckboxLabeled("IncludeTainted".Translate(), ref bill.includeTainted);
                    }

                    var products = bill.recipe.products;
                    if (products.Any(p => p.thingDef.useHitPoints))
                    {
                        Widgets.FloatRange(listing.GetRect(28f), 975643279, ref bill.hpRange, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
                        bill.hpRange.min = Mathf.Round(bill.hpRange.min * 100f) / 100f;
                        bill.hpRange.max = Mathf.Round(bill.hpRange.max * 100f) / 100f;
                    }
                    if (producedThingDef.HasComp(typeof(CompQuality)))
                    {
                        Widgets.QualityRange(listing.GetRect(28f), 1098906561, ref bill.qualityRange);
                    }
                    if (producedThingDef.MadeFromStuff)
                    {
                        listing.CheckboxLabeled("LimitToAllowedStuff".Translate(), ref bill.limitToAllowedStuff);
                    }
                }
            }
            catch (System.Exception e)
            {
                UILogger.LogControlError("product options", "building", e);
            }
        }

        private static void BuildPauseSection(Dialog_BillConfig dialog, Bill_Production bill, Listing_Standard listing)
        {
            try
            {
                if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
                {
                    listing.CheckboxLabeled("PauseWhenSatisfied".Translate(), ref bill.pauseWhenSatisfied);
                    if (bill.pauseWhenSatisfied)
                    {
                        listing.Label("UnpauseWhenYouHave".Translate() + ": " + bill.unpauseWhenYouHave.ToString("F0"));
                        string buffer = dialog.GetType().GetField("unpauseCountEditBuffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(dialog) as string;
                        if (buffer != null)
                        {
                            listing.IntEntry(ref bill.unpauseWhenYouHave, ref buffer, bill.recipe.targetCountAdjustment);
                            if (bill.unpauseWhenYouHave >= bill.targetCount)
                            {
                                bill.unpauseWhenYouHave = bill.targetCount - 1;
                                buffer = bill.unpauseWhenYouHave.ToString();
                            }
                        }
                        else
                        {
                            UILogger.LogWarning("Unpause count buffer not found");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                UILogger.LogControlError("pause section", "building", e);
            }
        }
    }
}
