using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace StorageSelector.Patches.BillConfig
{
    public static class BillConfigWorker
    {
        public static string GetWorkerLabel(Bill_Production bill)
        {
            if (bill.PawnRestriction != null)
                return bill.PawnRestriction.LabelShortCap;
            else if (ModsConfig.IdeologyActive && bill.SlavesOnly)
                return "AnySlave".Translate();
            else if (ModsConfig.BiotechActive && bill.recipe.mechanitorOnlyRecipe)
                return "AnyMechanitor".Translate();
            else if (!ModsConfig.BiotechActive || !bill.MechsOnly)
                return !ModsConfig.BiotechActive || !bill.NonMechsOnly ? "AnyWorker".Translate() : "AnyNonMech".Translate();
            else
                return "AnyMech".Translate();
        }

        public static void ShowWorkerMenu(Bill_Production bill)
        {
            var options = new List<FloatMenuOption>
            {
                new("AnyWorker".Translate(), () => bill.SetAnyPawnRestriction())
            };

            if (ModsConfig.IdeologyActive)
                options.Add(new FloatMenuOption("AnySlave".Translate(), () => bill.SetAnySlaveRestriction()));

            if (ModsConfig.BiotechActive && bill.recipe.workSkill != null)
            {
                if (bill.recipe.mechanitorOnlyRecipe)
                {
                    var mechanitors = bill.Map.mapPawns.FreeColonists.Where(p => MechanitorUtility.IsMechanitor(p));
                    foreach (var pawn in mechanitors.OrderBy(p => p.LabelShortCap))
                    {
                        options.Add(new FloatMenuOption(pawn.LabelShortCap, () => bill.SetPawnRestriction(pawn)));
                    }
                }
                else
                {
                    if (MechWorkUtility.AnyWorkMechCouldDo(bill.recipe))
                    {
                        options.Add(new FloatMenuOption("AnyMech".Translate(), () => bill.SetAnyMechRestriction()));
                        options.Add(new FloatMenuOption("AnyNonMech".Translate(), () => bill.SetAnyNonMechRestriction()));
                    }

                    var pawns = bill.Map.mapPawns.FreeColonists
                        .Where(p => p.skills != null && p.skills.GetSkill(bill.recipe.workSkill).Level >= bill.allowedSkillRange.min)
                        .OrderBy(p => p.LabelShortCap);

                    foreach (var pawn in pawns)
                    {
                        options.Add(new FloatMenuOption(pawn.LabelShortCap, () => bill.SetPawnRestriction(pawn)));
                    }
                }
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}
