using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace AnimalsLogic
{
    public class Alert_LifeThreateningHediffAnimal : Alert_Critical
    {

        private IEnumerable<Thing> SickAnimals
        {
            get
            {
                foreach (Pawn p in PawnsFinder.AllMaps_Spawned.Where(p => p.PlayerColonyAnimal_Alive_NoCryptosleep()))
                    for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
                    {
                        Hediff diff = p.health.hediffSet.hediffs[i];
                        if (diff.CurStage != null && diff.CurStage.lifeThreatening && !diff.FullyImmune())
                        {
                            yield return p;
                            break;
                        }
                    }
            }
        }

        public override string GetLabel()
        {
            return "AnimalsWithLifeThreateningDisease".Translate();
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool amputatable = false;
            foreach (Pawn pawn in AnimalAlertsUtility.SortedAnimalList(SickAnimals))
            {
                stringBuilder.AppendLine($"    {pawn.LabelShort} {((pawn.Name != null && !pawn.Name.Numerical) ? "(" + pawn.KindLabel + ")" : "")} {(pawn.HasBondRelation() ? "BondBrackets".Translate().ToString() : "")}");
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff.CurStage != null && hediff.CurStage.lifeThreatening && hediff.Part != null && hediff.Part != pawn.RaceProps.body.corePart)
                    {
                        amputatable = true;
                        break;
                    }
                }
            }
            if (amputatable)
                return string.Format("AnimalsWithLifeThreateningDiseaseAmputationDesc".Translate(), stringBuilder.ToString());
            return string.Format("AnimalsWithLifeThreateningDiseaseDesc".Translate(), stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            return (Settings.medical_alerts) ? AlertReport.CulpritsAre(SickAnimals.ToList<Thing>()) : false;
        }

    }
}
