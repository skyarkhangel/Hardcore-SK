using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using RimWorld;

namespace Psychology.Detour
{
    internal static class _Recipe_InstallImplant
    {
        internal static void _ApplyOnPawn(this Recipe_InstallImplant r, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
        {
            if (billDoer != null)
            {
                if (_Recipe_MedicalOperation._CheckSurgeryFail(r,billDoer, pawn, ingredients))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                billDoer.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.ReplacedPartBleedingHeart, pawn);
            }
            pawn.health.AddHediff(r.recipe.addsHediff, part, null);
        }
    }
}
