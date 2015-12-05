using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace SK_Enviro.AI
{
    public class ThinkNode_Conditions : ThinkNode_Conditional
    {
        public bool invertResult;

        public ThinkNode_Conditions()
        {
        }

        protected override bool Satisfied(Pawn pawn)
        {
            bool result = false;
            Pawn pet = pawn as Pawn;
            if (pet != null)
                result = pet.Faction == Faction.OfColony;
            if (invertResult)
                result = !result;
            return result;
        }
    }
}
