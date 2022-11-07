using System.Text;
using RimWorld;
using Verse;

namespace Hospitality
{
    public class StatWorker_RelationshipDamage : StatWorker
    {
        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (!req.HasThing) return 0;
            if (!(req.Thing is Pawn pawn)) return 0;
            var factor = PriceUtility.PawnQualityPriceFactor(pawn);
            return factor*stat.defaultBaseValue+stat.defaultBaseValue/5;
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            var stringBuilder = new StringBuilder();
            if (!req.HasThing || !(req.Thing is Pawn)) return base.GetExplanationUnfinalized(req, numberSense);

            stringBuilder.AppendLine("StatsReport_BaseValue".Translate());
            float statValueAbstract = stat.defaultBaseValue;
            stringBuilder.AppendLine("    " + stat.ValueToString(statValueAbstract, numberSense));
            
            var pawn = (Pawn) req.Thing;
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"{"StatsReport_CharacterQuality".Translate()}: x{PriceUtility.PawnQualityPriceFactor(pawn).ToStringPercent()} +{stat.defaultBaseValue / 5}");
            return stringBuilder.ToString();
        }
    }
}