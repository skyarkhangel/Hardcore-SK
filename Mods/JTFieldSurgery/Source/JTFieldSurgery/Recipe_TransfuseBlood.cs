using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace JTFieldSurgery
{

    class Recipe_TransfuseBlood : RecipeWorker
    {
        protected float transfusion_amount = 1.0f;
        protected HediffDef hediffDef = LocalDefOf.JTFieldSurgeryBloodPackHediff;
        protected ThingDef filthDef = ThingDefOf.FilthBlood;

        public override void ApplyOnPawn (Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (ingredients.Count > 0) {
                Thing bloodpack = ingredients [0];
                transfusion_amount = Math.Max (1.0f, Math.Min (0.0f, (float)bloodpack.HitPoints) / (float)bloodpack.MaxHitPoints);
            }

            int medical_skill = billDoer.skills.GetSkill (SkillDefOf.Medicine).Level;
            if (medical_skill < 6) {
                float spillage = Verse.Rand.Range (0.0f, transfusion_amount * ((float)(6 - medical_skill)) / 6.0f);
                transfusion_amount -= spillage;
                int count = (int)(spillage * 8.0f);
                if (filthDef != null && count > 0) {
                    FilthMaker.MakeFilth (pawn.Position, pawn.Map, filthDef, count);
                }
            }

            if (hediffDef is HediffDef) {
                HealthUtility.AdjustSeverity (pawn, hediffDef, transfusion_amount);
            }
        }

        public override bool IsViolationOnPawn (Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
        {
            return !pawn.health.hediffSet.HasHediff (HediffDefOf.BloodLoss);
        }
    }

    class Recipe_TransfusePlasma : Recipe_TransfuseBlood
    {
        public Recipe_TransfusePlasma ()
        {
            transfusion_amount = Recipe_DrawBlood.BLOOD_PACK_SEVERITY; //do not change to plasma_pack_severity
            hediffDef = LocalDefOf.JTFieldSurgeryPlasmaPackHediff;
            filthDef = ThingDefOf.FilthSlime;
        }
    }

    class Recipe_TransfuseSaline : Recipe_TransfuseBlood
    {
        public Recipe_TransfuseSaline ()
        {
            transfusion_amount = Recipe_DrawBlood.BLOOD_PACK_SEVERITY;
            hediffDef = LocalDefOf.JTFieldSurgerySalinePackHediff;
        }
    }

}
