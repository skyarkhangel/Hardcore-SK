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

namespace SK_GH
{
    public class PlaceWorker_NearSteamGeyser : PlaceWorker
    {
        public const int maxDistanceFromSteamGeyser = 10;
        public const int minDistanceBetweenGH = 15;
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            List<Thing> SteamGeyserList = Find.ListerThings.ThingsOfDef(ThingDef.Named("SteamGeyser"));
            List<Thing> GHList = Find.ListerThings.ThingsOfDef(Util_GH.GHDef);
            List<Thing> GHBlueprintList = Find.ListerThings.ThingsOfDef(Util_GH.GHDef.blueprintDef);
            List<Thing> GHFrameList = Find.ListerThings.ThingsOfDef(Util_GH.GHDef.frameDef);

                IEnumerable<Thing> SteamGeyserInTheArea = SteamGeyserList.Where(Thing => loc.InHorDistOf(Thing.Position, maxDistanceFromSteamGeyser));
                if (SteamGeyserInTheArea.Count() < 1)
                {
                    return (AcceptanceReport)Translator.Translate("NoGeothemalSource");
                }
                if (GHList != null)
                {
                    IEnumerable<Thing> GHInTheArea = GHList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenGH));
                    if (GHInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherGH");
                    }
                }
                if (GHBlueprintList != null)
                {
                    IEnumerable<Thing> fishingPierBlueprintInTheArea = GHBlueprintList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenGH));
                    if (fishingPierBlueprintInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherGH");
                    }
                }
                if (GHFrameList != null)
                {
                    IEnumerable<Thing> fishingPierFrameInTheArea = GHFrameList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenGH));
                    if (fishingPierFrameInTheArea.Count() > 0)
                    {
                        return (AcceptanceReport)Translator.Translate("AnotherGH");
                    }
                }
                return (AcceptanceReport)true;
        }
    }
}