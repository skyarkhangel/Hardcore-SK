using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace Hospitality
{
    public abstract class Alert_Guest : Alert
    {
        protected List<Pawn> affectedPawnsResult = new List<Pawn>();
        protected string explanationKey;

        protected abstract List<Pawn> AffectedPawns { get; }

        public override string GetLabel()
        {
            int count = AffectedPawns.Count;
            string label = base.GetLabel();
            if (count > 1)
                return $"{label} x{count.ToString()}";
            return label;
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(AffectedPawns);
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Pawn affectedPawn in AffectedPawns)
                stringBuilder.AppendLine("  - " + affectedPawn.NameShortColored.Resolve());
            return explanationKey.Translate(stringBuilder.ToString());
        }
    }
}
