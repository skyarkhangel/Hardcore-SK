using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects armaxDistanceFromFissuree here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace SK_FG
{
        public static class Util_FG
    {
        public static ThingDef FGDef
        {
            get
            {
                return (ThingDef.Named("FissureGenerator"));
            }
        }
    }

        public class PlaceWorker_NoNearFissureGenerator : PlaceWorker
    {
        public const int minDistanceFromFissure = 15;
        public const int minDistanceBetweenFG = 15;
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            List<Thing> FissureList = Find.ListerThings.ThingsOfDef(ThingDef.Named("Fissure"));
            List<Thing> FGList = Find.ListerThings.ThingsOfDef(Util_FG.FGDef);
            List<Thing> FGBlueprintList = Find.ListerThings.ThingsOfDef(Util_FG.FGDef.blueprintDef);
            List<Thing> FGFrameList = Find.ListerThings.ThingsOfDef(Util_FG.FGDef.frameDef);

                IEnumerable<Thing> FissureInTheArea = FissureList.Where(Thing => loc.InHorDistOf(Thing.Position, minDistanceFromFissure));
                if (FissureInTheArea.Count() > 0)
                {
                    return (AcceptanceReport)Translator.Translate("NearWithAnotherFissure");
                }
                if (FGList != null)
                {
                    IEnumerable<Thing> FGInTheArea = FGList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenFG));
                    if (FGInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherFG");
                    }
                }
                if (FGBlueprintList != null)
                {
                    IEnumerable<Thing> fishingPierBlueprintInTheArea = FGBlueprintList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenFG));
                    if (fishingPierBlueprintInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherFG");
                    }
                }
                if (FGFrameList != null)
                {
                    IEnumerable<Thing> fishingPierFrameInTheArea = FGFrameList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenFG));
                    if (fishingPierFrameInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherFG");
                    }
                }
                return (AcceptanceReport)true;
        }
    }
}