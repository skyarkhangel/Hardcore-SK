using System;
using RimWorld;
using Verse;

namespace Soyuz.Core
{
    public class HediffTracker
    {
        private Pawn pawn;
        private bool pregnant = false;

        public Pawn Pawn
        {
            get => pawn;
        }

        public bool Pregnant
        {
            get => pregnant;
            set => pregnant = value;
        }

        public HediffTracker(Pawn pawn)
        {
            this.pawn = pawn;
            if (this.pawn?.health?.hediffSet?.HasHediff(HediffDefOf.Pregnant) ?? false)
            {
                this.pregnant = true;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look<bool>(ref pregnant, "pregnant", false);
        }
    }
}
