using PeteTimesSix.SimpleSidearms;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;

using static PeteTimesSix.SimpleSidearms.SimpleSidearms;

namespace SimpleSidearms.rimworld.alerts
{
    public class Alert_MissingSidearm : Alert
    {
        public string explanation;

        public Alert_MissingSidearm()
        {
            this.defaultLabel = "Alert_MissingSidearm_label".Translate();
            explanation = "Alert_MissingSidearm_desc";
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Pawn current in this.AffectedPawns())
            {
                stringBuilder.AppendLine("    " + current.Name);
            }
            return explanation.Translate(stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            //return true;
            Pawn pawn = this.AffectedPawns().FirstOrDefault<Pawn>();
            if (pawn != null)
            {
                return AlertReport.CulpritIs(pawn);
            }
            return AlertReport.Inactive;
        }

        [DebuggerHidden]
        public IEnumerable<Pawn> AffectedPawns()
        {
            if (!Settings.ShowAlertsMissingSidearm)
                yield break;
            else 
            {
                HashSet<Pawn> pawns = new HashSet<Pawn>();
                if (PawnsFinder.AllMaps_FreeColonistsSpawned != null)
                {
                    foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsSpawned)
                    {
                        if (!pawn.IsValidSidearmsCarrier())
                        {
                            continue;
                        }
                        else
                        {
                            if (pawn.health != null && pawn.Downed)
                                continue;
                            if (pawn.drafter != null && pawn.Drafted)
                                continue;
                            if (pawn.CurJob != null && pawn.CurJob.def != null && (pawn.CurJob.def == SidearmsDefOf.EquipSecondary))
                                continue;

                            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                            if (pawnMemory != null)
                            {
                                foreach (ThingDefStuffDefPair weaponMemory in pawnMemory.RememberedWeapons)
                                {
                                    if (!pawn.hasWeaponType(weaponMemory))
                                    {
                                        if (pawns.Add(pawn))
                                            yield return pawn;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
