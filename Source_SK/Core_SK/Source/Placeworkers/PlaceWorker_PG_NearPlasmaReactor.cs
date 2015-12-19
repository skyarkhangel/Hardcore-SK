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

namespace SK_Plasmagenerator
{
    public class PlaceWorker_PGNearPlasmaGeothermalPlant : PlaceWorker
    {
        public const int maxDistanceFromPlasmaGeothermalPlant = 10;
        public const int minDistanceBetweenPG = 10;
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            List<Thing> PlasmaGeothermalPlantList = Find.ListerThings.ThingsOfDef(ThingDef.Named("PlasmaGeothermalPlant"));
            List<Thing> PGList = Find.ListerThings.ThingsOfDef(Util_PG.PGDef);
            List<Thing> PGBlueprintList = Find.ListerThings.ThingsOfDef(Util_PG.PGDef.blueprintDef);
            List<Thing> PGFrameList = Find.ListerThings.ThingsOfDef(Util_PG.PGDef.frameDef);

            IEnumerable<Thing> PlasmaGeothermalPlantInTheArea = PlasmaGeothermalPlantList.Where(Thing => loc.InHorDistOf(Thing.Position, maxDistanceFromPlasmaGeothermalPlant));
            if (PlasmaGeothermalPlantInTheArea.Count() < 1)
                {
                    return (AcceptanceReport)Translator.Translate("NoPlasmaGeothermalPlant");
                }
                if (PGList != null)
                {
                    IEnumerable<Thing> PGInTheArea = PGList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenPG));
                    if (PGInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherPG");
                    }
                }
                if (PGBlueprintList != null)
                {
                    IEnumerable<Thing> fishingPierBlueprintInTheArea = PGBlueprintList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenPG));
                    if (fishingPierBlueprintInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherPG");
                    }
                }
                if (PGFrameList != null)
                {
                    IEnumerable<Thing> fishingPierFrameInTheArea = PGFrameList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenPG));
                    if (fishingPierFrameInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherPG");
                    }
                }
                return (AcceptanceReport)true;
        }
    }
}