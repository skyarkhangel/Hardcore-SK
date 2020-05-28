using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Hospitality {
    public class Alert_GuestNotEnoughBeds : Alert_Guest
    {
        private static List<Thought> tmpThoughts = new List<Thought>();
        public Alert_GuestNotEnoughBeds()
        {
            defaultLabel = "AlertNotEnoughBeds".Translate();
            explanationKey = "AlertNotEnoughBedsDesc";
        }

        protected ThoughtDef Thought => DefDatabase<ThoughtDef>.GetNamed("GuestBedCount");

        protected override List<Pawn> AffectedPawns
        {
            get
            {
                affectedPawnsResult.Clear();
                foreach (var map in Find.Maps)
                foreach (var pawn in map.GetMapComponent().PresentGuests)
                {
                    if (pawn.Dead) continue;

                    if (pawn.needs.mood != null)
                    {
                        pawn.needs.mood.thoughts.GetAllMoodThoughts(tmpThoughts);
                        try
                        {
                            foreach (var thought in tmpThoughts)
                            {
                                if (thought.def == Thought && thought.CurStageIndex < 2)
                                    affectedPawnsResult.Add(pawn);
                            }
                        }
                        finally
                        {
                            tmpThoughts.Clear();
                        }
                    }
                }

                return affectedPawnsResult;
            }
        }
    }
}
