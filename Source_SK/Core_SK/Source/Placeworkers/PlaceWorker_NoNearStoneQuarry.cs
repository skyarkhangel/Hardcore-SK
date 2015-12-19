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

namespace SK_SQ
{
    public static class Util_SQ
    {
        public static ThingDef SQDef
        {
            get
            {
                return (ThingDef.Named("Extractor"));
            }
        }
    }
    public class PlaceWorker_NoNearStoneQuarry : PlaceWorker
    {
        public const int minDistanceBetweenSQ = 15;
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            List<Thing> ExtractorList = Find.ListerThings.ThingsOfDef(ThingDef.Named("StoneQuarry"));
            List<Thing> SQList = Find.ListerThings.ThingsOfDef(Util_SQ.SQDef);
            List<Thing> SQBlueprintList = Find.ListerThings.ThingsOfDef(Util_SQ.SQDef.blueprintDef);
            List<Thing> SQFrameList = Find.ListerThings.ThingsOfDef(Util_SQ.SQDef.frameDef);

                if (SQList != null)
                {
                    IEnumerable<Thing> SQInTheArea = SQList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenSQ));
                    if (SQInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherSQ");
                    }
                }
                if (SQBlueprintList != null)
                {
                    IEnumerable<Thing> fishingPierBlueprintInTheArea = SQBlueprintList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenSQ));
                    if (fishingPierBlueprintInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherSQ");
                    }
                }
                if (SQFrameList != null)
                {
                    IEnumerable<Thing> fishingPierFrameInTheArea = SQFrameList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenSQ));
                    if (fishingPierFrameInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherSQ");
                    }
                }
                return (AcceptanceReport)true;
        }
    }
}