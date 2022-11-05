using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace aRandomKiwi.HFM
{
    class PawnColumnWorker_HuntingCanAssist : PawnColumnWorker_Checkbox
    {
        protected override bool HasCheckbox(Pawn pawn)
        {
            return pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer && pawn.training.HasLearned(DefDatabase<TrainableDef>.GetNamed("HuntingTraining", true));
        }

        protected override bool GetValue(Pawn pawn)
        {
            if (pawn.TryGetComp<Comp_Hunting>() != null)
                return pawn.TryGetComp<Comp_Hunting>().huntingCanAssist;
            else
                return false;
        }

        protected override void SetValue(Pawn pawn, bool value, PawnTable table)
        {
            if (pawn.TryGetComp<Comp_Hunting>() != null)
                pawn.TryGetComp<Comp_Hunting>().huntingCanAssist = value;
        }
    }
}
