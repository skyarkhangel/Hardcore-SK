using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using StorageSelector.Integration.WhatsMissing;
using StorageSelector.Core;

namespace StorageSelector.Patches.BillConfig
{
    [HarmonyPatch]
    public static class BillConfigPatches
    {
        private static readonly FieldInfo BillGetter = AccessTools.Field(typeof(Dialog_BillConfig), "bill");
        private static readonly FieldInfo RepeatCountBufferField = AccessTools.Field(typeof(Dialog_BillConfig), "repeatCountEditBuffer");
        private static readonly FieldInfo TargetCountBufferField = AccessTools.Field(typeof(Dialog_BillConfig), "targetCountEditBuffer");
        private static readonly FieldInfo UnpauseCountBufferField = AccessTools.Field(typeof(Dialog_BillConfig), "unpauseCountEditBuffer");
        private static readonly MethodInfo DoIngredientConfigPaneMethod = AccessTools.Method(typeof(Dialog_BillConfig), "DoIngredientConfigPane");

        [HarmonyPatch(typeof(Dialog_BillConfig))]
        [HarmonyPatch("DoWindowContents")]
        public static class DoWindowContents_Patch
        {
            public static bool Prefix(Dialog_BillConfig __instance, Rect inRect)
            {
                try
                {
                    if (BillGetter.GetValue(__instance) is not Bill_Production bill)
                        return true;

                    var repeatCountBuffer = (string)RepeatCountBufferField.GetValue(__instance);
                    var targetCountBuffer = (string)TargetCountBufferField.GetValue(__instance);
                    var unpauseCountBuffer = (string)UnpauseCountBufferField.GetValue(__instance);

                    Text.Font = GameFont.Medium;
                    Widgets.Label(BillConfigLayout.GetHeaderRect(), bill.LabelCap);
                    Text.Font = GameFont.Small;

                    var (leftRect, middleRect, rightRect) = BillConfigLayout.GetColumnRects(inRect);

                    var leftListing = new Listing_Standard();
                    leftListing.Begin(leftRect);

                    if (bill.suspended)
                    {
                        if (leftListing.ButtonText("Suspended".Translate()))
                        {
                            bill.suspended = false;
                            SoundDefOf.Click.PlayOneShot(SoundInfo.OnCamera());
                        }
                    }
                    else if (leftListing.ButtonText("NotSuspended".Translate()))
                    {
                        bill.suspended = true;
                        SoundDefOf.Click.PlayOneShot(SoundInfo.OnCamera());
                    }

                    WMRequirements.RenderRequirements(bill, leftListing, leftRect);

                    leftListing.End();

                    var middleListing = new Listing_Standard();
                    middleListing.Begin(middleRect);

                    var (repeatSection, repeatRect) = middleListing.BeginStyledSection(BillConfigLayout.RepeatModeHeight, BillConfigLayout.SectionGapBefore, BillConfigLayout.SectionGapAfter);

                    if (repeatSection.ButtonText(bill.repeatMode.LabelCap))
                    {
                        BillRepeatModeUtility.MakeConfigFloatMenu(bill);
                    }
                    repeatSection.Gap(12f);

                    if (bill.repeatMode == BillRepeatModeDefOf.RepeatCount)
                    {
                        repeatSection.Label("RepeatCount".Translate(bill.repeatCount));
                        repeatSection.IntEntry(ref bill.repeatCount, ref repeatCountBuffer, 1);
                    }
                    else if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
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

                        repeatSection.Label(text);

                        int oldCount = bill.targetCount;
                        repeatSection.IntEntry(ref bill.targetCount, ref targetCountBuffer, bill.recipe.targetCountAdjustment);
                        bill.unpauseWhenYouHave = Mathf.Max(0, bill.unpauseWhenYouHave + (bill.targetCount - oldCount));

                        ThingDef producedThingDef = bill.recipe.ProducedThingDef;
                        if (producedThingDef != null)
                        {
                            if (producedThingDef.IsWeapon || producedThingDef.IsApparel)
                            {
                                repeatSection.CheckboxLabeled("IncludeEquipped".Translate(), ref bill.includeEquipped);
                            }
                            if (producedThingDef.IsApparel && producedThingDef.apparel.careIfWornByCorpse)
                            {
                                repeatSection.CheckboxLabeled("IncludeTainted".Translate(), ref bill.includeTainted);
                            }

                            List<ThingDefCountClass> products = bill.recipe.products;
                            if (products.Any(p => p.thingDef.useHitPoints))
                            {
                                Widgets.FloatRange(repeatSection.GetRect(28f), 975643279, ref bill.hpRange, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
                                bill.hpRange.min = Mathf.Round(bill.hpRange.min * 100f) / 100f;
                                bill.hpRange.max = Mathf.Round(bill.hpRange.max * 100f) / 100f;
                            }
                            if (producedThingDef.HasComp(typeof(CompQuality)))
                            {
                                Widgets.QualityRange(repeatSection.GetRect(28f), 1098906561, ref bill.qualityRange);
                            }
                            if (producedThingDef.MadeFromStuff)
                            {
                                repeatSection.CheckboxLabeled("LimitToAllowedStuff".Translate(), ref bill.limitToAllowedStuff);
                            }
                        }

                        repeatSection.CheckboxLabeled("PauseWhenSatisfied".Translate(), ref bill.pauseWhenSatisfied);
                        if (bill.pauseWhenSatisfied)
                        {
                            repeatSection.Label("UnpauseWhenYouHave".Translate() + ": " + bill.unpauseWhenYouHave.ToString("F0"));
                            repeatSection.IntEntry(ref bill.unpauseWhenYouHave, ref unpauseCountBuffer, bill.recipe.targetCountAdjustment);
                            if (bill.unpauseWhenYouHave >= bill.targetCount)
                            {
                                bill.unpauseWhenYouHave = bill.targetCount - 1;
                                unpauseCountBuffer = bill.unpauseWhenYouHave.ToString();
                            }
                        }
                    }

                    middleListing.EndStyledSection(repeatSection, repeatRect);
                    middleListing.Gap(BillConfigLayout.SectionGap);

                    var (storageSection, storageRect) = middleListing.BeginStyledSection(BillConfigLayout.StorageModeHeight, BillConfigLayout.SectionGapBefore, BillConfigLayout.SectionGapAfter);

                    var storage = ExtendedBillDataStorage.GetStorage();
                    if (storage != null)
                    {
                        var inputStorage = storage.GetInputStorage(bill);
                        var outputStorage = storage.GetOutputStorage(bill);

                        if (storageSection.ButtonText(BillConfigStorage.GetInputStorageButtonText(bill, inputStorage)))
                        {
                            BillConfigStorage.ShowInputStorageMenu(bill, storage);
                        }

                        if (storageSection.ButtonText(BillConfigStorage.GetOutputStorageButtonText(bill, outputStorage)))
                        {
                            BillConfigStorage.ShowOutputStorageMenu(bill, storage);
                        }
                    }

                    middleListing.EndStyledSection(storageSection, storageRect);
                    middleListing.Gap(BillConfigLayout.SectionGap);

                    var (workerSection, workerRect) = middleListing.BeginStyledSection(BillConfigLayout.WorkerSelectionHeight, BillConfigLayout.SectionGapBefore, BillConfigLayout.SectionGapAfter);

                    if (workerSection.ButtonText(BillConfigWorker.GetWorkerLabel(bill)))
                    {
                        BillConfigWorker.ShowWorkerMenu(bill);
                    }

                    if (bill.PawnRestriction == null && bill.recipe.workSkill != null && !bill.MechsOnly)
                    {
                        workerSection.Label("AllowedSkillRange".Translate(bill.recipe.workSkill.label));
                        workerSection.IntRange(ref bill.allowedSkillRange, 0, 20);
                    }

                    middleListing.EndStyledSection(workerSection, workerRect);
                    middleListing.End();

                    DoIngredientConfigPaneMethod.Invoke(__instance, new object[] {
                        rightRect.x, rightRect.y, rightRect.width, rightRect.height
                    });

                    if (bill.recipe.products.Count == 1)
                    {
                        var product = bill.recipe.products[0].thingDef;
                        Widgets.InfoCardButton(leftRect.x, rightRect.y, product, GenStuff.DefaultStuffFor(product));
                    }

                    return false;
                }
                catch (Exception e)
                {
                    Log.Error($"[StorageSelector] Error in DoWindowContents Prefix: {e}");
                    return true;
                }
            }
        }
    }
}
