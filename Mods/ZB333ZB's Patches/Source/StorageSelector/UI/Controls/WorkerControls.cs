using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using StorageSelector.UI.Logging;

namespace StorageSelector.UI.Controls
{
    public static class WorkerControls
    {
        public static void BuildWorkerSection(Bill_Production bill, Listing_Standard listingStandard, float height, float gapBefore, float gapAfter)
        {
            try
            {
                if (bill == null)
                {
                    UILogger.LogControlError("worker section", "null bill", new System.ArgumentNullException(nameof(bill)));
                    return;
                }

                if (listingStandard == null)
                {
                    UILogger.LogControlError("worker section", "null listing", new System.ArgumentNullException(nameof(listingStandard)));
                    return;
                }

                var listingStandard3 = listingStandard.BeginSection(height, gapBefore, gapAfter);

                try
                {
                    string workerLabel = GetWorkerLabel(bill);
                    if (listingStandard3.ButtonText(workerLabel))
                    {
                        ShowWorkerMenu(bill);
                    }

                    if (bill.PawnRestriction == null && bill.recipe.workSkill != null && !bill.MechsOnly)
                    {
                        listingStandard3.Label("AllowedSkillRange".Translate(bill.recipe.workSkill.label));
                        listingStandard3.IntRange(ref bill.allowedSkillRange, 0, 20);
                    }
                }
                catch (System.Exception e)
                {
                    UILogger.LogControlError("worker controls", "building", e);
                }
                finally
                {
                    listingStandard.EndSection(listingStandard3);
                }
            }
            catch (System.Exception e)
            {
                UILogger.LogControlError("worker section", "initializing", e);
            }
        }

        private static string GetWorkerLabel(Bill_Production bill)
        {
            try
            {
                if (bill.PawnRestriction != null)
                    return bill.PawnRestriction.LabelShortCap;
                if (ModsConfig.IdeologyActive && bill.SlavesOnly)
                    return "AnySlave".Translate();
                if (ModsConfig.BiotechActive && bill.recipe.mechanitorOnlyRecipe)
                    return "AnyMechanitor".Translate();
                if (!ModsConfig.BiotechActive || !bill.MechsOnly)
                    return !ModsConfig.BiotechActive || !bill.NonMechsOnly ? "AnyWorker".Translate() : "AnyNonMech".Translate();
                return "AnyMech".Translate();
            }
            catch (System.Exception e)
            {
                UILogger.LogControlError("worker label", "getting", e);
                return "AnyWorker".Translate();
            }
        }

        private static void ShowWorkerMenu(Bill_Production bill)
        {
            try
            {
                var options = new List<FloatMenuOption>
                {
                    new("AnyWorker".Translate(), () =>
                    {
                        try
                        {
                            bill.SetAnyPawnRestriction();
                        }
                        catch (System.Exception e)
                        {
                            UILogger.LogControlError("worker menu", "setting any worker", e);
                        }
                    })
                };

                if (ModsConfig.IdeologyActive)
                {
                    options.Add(new FloatMenuOption("AnySlave".Translate(), () =>
                    {
                        try
                        {
                            bill.SetAnySlaveRestriction();
                        }
                        catch (System.Exception e)
                        {
                            UILogger.LogControlError("worker menu", "setting slave restriction", e);
                        }
                    }));
                }

                if (ModsConfig.BiotechActive && bill.recipe.workSkill != null)
                {
                    try
                    {
                        AddBiotechOptions(bill, options);
                    }
                    catch (System.Exception e)
                    {
                        UILogger.LogControlError("worker menu", "adding biotech options", e);
                    }
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }
            catch (System.Exception e)
            {
                UILogger.LogControlError("worker menu", "showing", e);
            }
        }

        private static void AddBiotechOptions(Bill_Production bill, List<FloatMenuOption> options)
        {
            if (bill.recipe.mechanitorOnlyRecipe)
            {
                var mechanitors = bill.Map.mapPawns.FreeColonists.Where(p => MechanitorUtility.IsMechanitor(p));
                foreach (var pawn in mechanitors.OrderBy(p => p.LabelShortCap))
                {
                    options.Add(new FloatMenuOption(pawn.LabelShortCap, () =>
                    {
                        try
                        {
                            bill.SetPawnRestriction(pawn);
                        }
                        catch (System.Exception e)
                        {
                            UILogger.LogControlError("worker menu", $"setting mechanitor {pawn.LabelShortCap}", e);
                        }
                    }));
                }
            }
            else
            {
                if (MechWorkUtility.AnyWorkMechCouldDo(bill.recipe))
                {
                    options.Add(new FloatMenuOption("AnyMech".Translate(), () =>
                    {
                        try
                        {
                            bill.SetAnyMechRestriction();
                        }
                        catch (System.Exception e)
                        {
                            UILogger.LogControlError("worker menu", "setting mech restriction", e);
                        }
                    }));
                    options.Add(new FloatMenuOption("AnyNonMech".Translate(), () =>
                    {
                        try
                        {
                            bill.SetAnyNonMechRestriction();
                        }
                        catch (System.Exception e)
                        {
                            UILogger.LogControlError("worker menu", "setting non-mech restriction", e);
                        }
                    }));
                }

                var pawns = bill.Map.mapPawns.FreeColonists
                    .Where(p => p.skills != null && p.skills.GetSkill(bill.recipe.workSkill).Level >= bill.allowedSkillRange.min)
                    .OrderBy(p => p.LabelShortCap);

                foreach (var pawn in pawns)
                {
                    options.Add(new FloatMenuOption(pawn.LabelShortCap, () =>
                    {
                        try
                        {
                            bill.SetPawnRestriction(pawn);
                        }
                        catch (System.Exception e)
                        {
                            UILogger.LogControlError("worker menu", $"setting pawn restriction {pawn.LabelShortCap}", e);
                        }
                    }));
                }
            }
        }
    }
}
