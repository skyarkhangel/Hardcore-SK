using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace SK_Enviro.AI
{
    public static class HerdAIUtility_Pets
    {
        public const int HERD_DISTANCE = 20 * 20;

        public static IEnumerable<Pawn> FindHerdMembers(Pawn pawn)
        {
            return Find.ListerPawns.AllPawns.Where(herdMember => IsInHerd(pawn, herdMember));
        }

        public static bool IsInHerd(Pawn herdMember, Pawn target)
        {
            Pawn tameableMember = herdMember as Pawn;
            if (tameableMember == null)
            {
                return false;
            }
            Pawn tameableTarget = target as Pawn;
            if (tameableTarget == null) 
            {
                return false;
            }
            if (target.def != herdMember.def
                || target == herdMember
                || !WanderUtility.InSameRoom(herdMember.Position, target.Position)
                || (herdMember.Position - target.Position).LengthHorizontalSquared > HERD_DISTANCE)
            {
                return false;
            }
            return true;
        }

        public static Faction GetColonyPetFaction()
        {
            return Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ColonyPets"));
        }
    }
}
