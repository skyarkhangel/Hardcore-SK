using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace Core_SK
{
    class Recipe_PatchScar : Recipe_MedicalOperation
    {
        public override void
            ApplyOnPawn(Verse.Pawn pawn, BodyPartRecord part, Verse.Pawn billDoer)
        {
            base.ApplyOnPawn(pawn, part, billDoer);
            foreach (Hediff hediff in
                     pawn.health.hediffSet.GetHediffs<Hediff>())
            {
                if (hediff is Hediff_Injury &&
                    part.def == hediff.Part.def)
                {
                    pawn.health.hediffSet.hediffs.Remove(hediff);
                }
            }
            return;
        }

        public override System.Collections.Generic.IEnumerable<BodyPartRecord>
            GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            List<BodyPartRecord> partsToApply = new List<BodyPartRecord>();
            List<BodyPartDef> appliableParts = recipe.appliedOnFixedBodyParts;
            foreach (Hediff hediff in pawn.health.hediffSet.GetHediffs<Hediff>())
            {
                if (hediff is Hediff_Injury &&
                    appliableParts.Contains(hediff.Part.def))
                {
                    partsToApply.Add(hediff.Part);
                }
            }
            return partsToApply;
        }
    }
}