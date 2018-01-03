using System.Collections.Generic;
using RimWorld;
using Verse;

namespace JTFieldSurgery
{

    class Recipe_DrawPlasma : Recipe_Surgery
    {
        internal const float PLASMA_PACK_SEVERITY = 0.20f;

        public override void ApplyOnPawn (Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            HealthUtility.AdjustSeverity (pawn, HediffDefOf.BloodLoss, PLASMA_PACK_SEVERITY);
            var bloodpack = ThingMaker.MakeThing (LocalDefOf.JTFieldSurgeryPlasmaPack);
            if (!GenPlace.TryPlaceThing (bloodpack, billDoer.Position, billDoer.Map, ThingPlaceMode.Near)) {
                GenSpawn.Spawn (LocalDefOf.JTFieldSurgeryPlasmaPack, billDoer.Position, billDoer.Map);
            }
        }

        public override bool IsViolationOnPawn (Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
        {
            //TODO: allow convincing other factions' pawns to donate blood voluntarily (check for valid thought) -- which will also require them to become temporary patients
            return pawn.Faction != billDoerFaction || pawn.health.hediffSet.HasHediff (HediffDefOf.BloodLoss);
        }
    }

}
