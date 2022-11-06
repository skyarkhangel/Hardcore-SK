using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace AnimalsLogic
{
    public static class AnimalAlertsUtility
    {

        public static List<Pawn> SortedAnimalList(IEnumerable<Thing> pawnEnumerable)
        {
            List<Pawn> pawnList = pawnEnumerable.Cast<Pawn>().ToList();
            pawnList.SortBy(p => !p.HasBondRelation(), p => p.LabelShort);
            return pawnList;
        }

        public static bool PlayerColonyAnimal(this Pawn p) =>
            p.Faction == Faction.OfPlayer && p.RaceProps.Animal;

        public static bool PlayerColonyAnimal_Alive_NoCryptosleep(this Pawn p) =>
            p.PlayerColonyAnimal() && !p.Dead && !p.Suspended;

        public static bool HasBondRelation(this Pawn p) =>
            TrainableUtility.GetAllColonistBondsFor(p).Any();

    }
}
