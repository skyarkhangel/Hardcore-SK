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

namespace SK_ME
{
    public static class Util_ME
    {
        public static ThingDef MEDef
        {
            get
            {
                return (ThingDef.Named("Extractor"));
            }
        }
    }
    public class PlaceWorker_NoNearMineExtractor : PlaceWorker
    {
        public const int minDistanceBetweenME = 15;
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            List<Thing> ExtractorList = Find.ListerThings.ThingsOfDef(ThingDef.Named("Extractor"));
            List<Thing> MEList = Find.ListerThings.ThingsOfDef(Util_ME.MEDef);
            List<Thing> MEBlueprintList = Find.ListerThings.ThingsOfDef(Util_ME.MEDef.blueprintDef);
            List<Thing> MEFrameList = Find.ListerThings.ThingsOfDef(Util_ME.MEDef.frameDef);

                if (MEList != null)
                {
                    IEnumerable<Thing> MEInTheArea = MEList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenME));
                    if (MEInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherME");
                    }
                }
                if (MEBlueprintList != null)
                {
                    IEnumerable<Thing> fishingPierBlueprintInTheArea = MEBlueprintList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenME));
                    if (fishingPierBlueprintInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherME");
                    }
                }
                if (MEFrameList != null)
                {
                    IEnumerable<Thing> fishingPierFrameInTheArea = MEFrameList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenME));
                    if (fishingPierFrameInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherME");
                    }
                }
                return (AcceptanceReport)true;
        }
    }
}