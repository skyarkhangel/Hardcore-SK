using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace Infused
{
    //Modifier will change item's stats.

    public class StatPart_InfusedModifier : StatPart
    {
        static readonly IEnumerable<StatCategoryDef> blacklistedCategories = new List<StatCategoryDef>()
        {
            StatCategoryDefOf.BasicsNonPawn,
            StatCategoryDefOf.Building,
            StatCategoryDefOf.StuffStatFactors,
        };

        static readonly IEnumerable<StatDef> blacklistedStats = new List<StatDef> {
            StatDefOf.MarketValue,
            StatDefOf.StuffEffectMultiplierInsulation_Cold,
            StatDefOf.StuffEffectMultiplierInsulation_Heat,
            StatDefOf.StuffEffectMultiplierArmor,
        };

        static bool IsNotBlacklistedForPawn(StatDef stat) {
            return !(blacklistedStats.Contains(stat) || blacklistedCategories.Contains(stat.category));
        }

        public StatPart_InfusedModifier(StatDef parentStat) 
        {
            this.parentStat = parentStat;
        }

        public override void TransformValue( StatRequest req, ref float val )
        {
            if (!req.HasThing) {
                return;
            }
            if (req.Thing.def.race != null && IsNotBlacklistedForPawn(parentStat)) {
                TransformValueForPawn (req, ref val);
            } else if (req.Thing.def.HasComp( typeof(CompInfused) )) {
                TransformValueForThing (req, ref val);
            }
        }

        void TransformValueForPawn( StatRequest req, ref float val )
        {
            var pawn = req.Thing as Pawn;
            //Just in case
            if ( pawn == null ) {
                return;
            }

            if ( pawn.equipment?.Primary != null ) {
                //Pawn has a primary weapon
                if (CompInfused.TryGetInfusedComp(pawn.equipment.Primary, out CompInfused inf))
                {
                    TransformValue(inf, parentStat, ref val);
                }
            }

            if (pawn.apparel != null) {
                //Pawn has apparels
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    if (CompInfused.TryGetInfusedComp(apparel, out CompInfused inf))
                    {
                        TransformValue(inf, parentStat, ref val);
                    }
                }
            }
        }

        void TransformValueForThing( StatRequest req, ref float val )
        {
            if (CompInfused.TryGetInfusedComp(req.Thing, out CompInfused inf)) {
                TransformValue(inf, parentStat, ref val);
            }
        }

        void TransformValue(CompInfused comp, StatDef stat, ref float val)
        {
            foreach (var infusion in comp.Infusions) {
                var statMod = infusion.stats?.TryGetValue(stat);
                if (statMod == null) {
                    continue;
                }

                val += statMod.offset;
                val *= statMod.multiplier;
            }
        }

        public override string ExplanationPart( StatRequest req )
        {
            if ( !req.HasThing ) {
                return null;
            }

            if (req.Thing.def.race != null && IsNotBlacklistedForPawn(parentStat)) {
                return ExplanationPartForPawn(req);
            }

            return ExplanationPartForThing(req);
        }

        string ExplanationPartForPawn( StatRequest req )
        {
            //Just in case
            var pawn = req.Thing as Pawn;
            if ( pawn == null ) {
                return null;
            }

            var result = new StringBuilder();

            if (pawn.equipment?.Primary != null) {
                //Pawn has a primary weapon
                if (CompInfused.TryGetInfusedComp(pawn.equipment.Primary, out CompInfused comp)) {
                    result.Append(WriteExplanation(pawn.equipment.Primary, comp));
                }
            }

            if (pawn.apparel != null) {
                //Pawn has apparels
                foreach (var apparel in pawn.apparel.WornApparel) {
                    if (CompInfused.TryGetInfusedComp(apparel, out CompInfused comp)) {
                        result.Append(WriteExplanation(apparel, comp));
                    }
                }
            }

            if (result.Length > 0) {
                return ResourceBank.Strings.DescBonus + "\n" + result;
            }

            return null;
        }


        string ExplanationPartForThing( StatRequest req )
        {
            if (CompInfused.TryGetInfusedComp(req.Thing, out CompInfused comp)) {
                return WriteExplanation(req.Thing, comp);
            }

            return null;
        }

        string WriteExplanation( Thing thing, CompInfused comp )
        {
            var result = new StringBuilder();

            foreach (var infusion in comp.Infusions) {
                var mod = infusion.stats.TryGetValue(parentStat);
                if (mod == null) {
                    continue;
                }

                if (System.Math.Abs(mod.offset) > 0.001f) {
                    result.Append("    " + infusion.label.CapitalizeFirst() + ": ");
                    result.Append(mod.offset > 0 ? "+" : "-");
                    result.AppendLine(parentStat.ValueToString(UnityEngine.Mathf.Abs(mod.offset)));
                }

                if (System.Math.Abs(mod.multiplier - 1f) > 0.001f) {
                    result.Append("    " + infusion.label.CapitalizeFirst() + ": x");
                    result.AppendLine(mod.multiplier.ToStringPercent());
                }
            }

            return result.ToString();
        }
    }
}
