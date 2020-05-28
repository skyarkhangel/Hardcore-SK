using System.Text;
using RimWorld;
using Verse;

namespace Hospitality
{
    public class StatWorker_RecruitEffectivity : StatWorker
    {
        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            var pawn = req.Thing as Pawn;
            if (pawn?.story == null) return 0;
            return stat.defaultBaseValue + pawn.skills.GetSkill(SkillDefOf.Social).Level/8f;
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            if (!req.HasThing || !(req.Thing is Pawn)) return base.GetExplanationUnfinalized(req, numberSense);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("StatsReport_BaseValue".Translate());
            float statValueAbstract = stat.defaultBaseValue;
            stringBuilder.AppendLine("    " + stat.ValueToString(statValueAbstract, numberSense));

            var pawn = (Pawn) req.Thing;
            
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("StatsReport_Skills".Translate());

            int level = pawn.skills.GetSkill(SkillDefOf.Social).Level;
            stringBuilder.AppendLine($"    {SkillDefOf.Social.LabelCap} ({level}): +{(level / 8f).ToStringDecimalIfSmall()}");

            return stringBuilder.ToString();
        }
    }
}