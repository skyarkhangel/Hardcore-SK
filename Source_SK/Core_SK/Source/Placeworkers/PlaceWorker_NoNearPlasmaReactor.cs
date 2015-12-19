using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace SK_Plasmareactor
{
    public static class Util_PRG
    {
        public static ThingDef PRGDef
        {
            get
            {
                return (ThingDef.Named("PlasmaGeothermalPlant"));
            }
        }
    }
    public class PlaceWorker_NoNearPlasmaGeothermalPlant : PlaceWorker
    {
        public const int minDistanceBetweenPRG = 15;
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            List<Thing> PlasmaGeothermalPlantList = Find.ListerThings.ThingsOfDef(ThingDef.Named("PlasmaGeothermalPlant"));
            List<Thing> PGRList = Find.ListerThings.ThingsOfDef(Util_PRG.PRGDef);
            List<Thing> PRGBlueprintList = Find.ListerThings.ThingsOfDef(Util_PRG.PRGDef.blueprintDef);
            List<Thing> PRGFrameList = Find.ListerThings.ThingsOfDef(Util_PRG.PRGDef.frameDef);

                if (PGRList != null)
                {
                    IEnumerable<Thing> PRGInTheArea = PGRList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenPRG));
                    if (PRGInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherPGR");
                    }
                }
                if (PRGBlueprintList != null)
                {
                    IEnumerable<Thing> fishingPierBlueprintInTheArea = PRGBlueprintList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenPRG));
                    if (fishingPierBlueprintInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherPGR");
                    }
                }
                if (PRGFrameList != null)
                {
                    IEnumerable<Thing> fishingPierFrameInTheArea = PRGFrameList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenPRG));
                    if (fishingPierFrameInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherPGR");
                    }
                }
                return (AcceptanceReport)true;
        }
    }
}