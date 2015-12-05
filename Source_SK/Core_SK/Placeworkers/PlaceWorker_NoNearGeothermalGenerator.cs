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

namespace SK_GG
{
    public static class Util_GG
    {
        public static ThingDef GGDef
        {
            get
            {
                return (ThingDef.Named("GeothermalGenerator"));
            }
        }
    }
    public class PlaceWorker_NoNearGeothermalGenerator : PlaceWorker
    {
        public const int minDistanceBetweenGG = 15;
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            List<Thing> GeothermalGeneratorList = Find.ListerThings.ThingsOfDef(ThingDef.Named("GeothermalGenerator"));
            List<Thing> GGList = Find.ListerThings.ThingsOfDef(Util_GG.GGDef);
            List<Thing> GGBlueprintList = Find.ListerThings.ThingsOfDef(Util_GG.GGDef.blueprintDef);
            List<Thing> GGFrameList = Find.ListerThings.ThingsOfDef(Util_GG.GGDef.frameDef);

                if (GGList != null)
                {
                    IEnumerable<Thing> GGInTheArea = GGList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenGG));
                    if (GGInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherGG");
                    }
                }
                if (GGBlueprintList != null)
                {
                    IEnumerable<Thing> fishingPierBlueprintInTheArea = GGBlueprintList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenGG));
                    if (fishingPierBlueprintInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherGG");
                    }
                }
                if (GGFrameList != null)
                {
                    IEnumerable<Thing> fishingPierFrameInTheArea = GGFrameList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenGG));
                    if (fishingPierFrameInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherGG");
                    }
                }
                return (AcceptanceReport)true;
        }
    }
}