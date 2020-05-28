using System;
using System.Linq;
using RimWorld;
using Verse;

namespace Hospitality
{
    internal static class DefsUtility
    {
        /// <summary>
        /// Make sure other mods don't have invalid defs that I get the blame for when groups don't spawn...
        /// </summary>
        public static void CheckForInvalidDefs()
        {
            try
            {
                CheckChemicalDefs();
                CheckFactionDefs();
                CheckBedDefs();
                CheckRoyalTitleDefs();
            }
            catch (Exception e)
            {
                Log.Error($"{e}");
            }
        }

        // All beds must be assignable to pawns
        private static void CheckBedDefs()
        {
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefsListForReading.Where(d=>d.thingClass == null))
            {
                LogMisconfiguration(thingDef, $"ThingDef {thingDef.defName} has no thingClass.");
            }
            foreach (var bedDef in DefDatabase<ThingDef>.AllDefsListForReading.Where(d=>d.thingClass != null && d.thingClass.IsSubclassOf(typeof(Building_Bed))))
            {
                if (!bedDef.HasComp(typeof(CompAssignableToPawn_Bed)))
                {
                    LogMisconfiguration(bedDef, $"ThingDef {bedDef.defName} must have a 'CompAssignableToPawn_Bed' in comps.");
                }
            }
        }

        // Must have at least one pawn group maker of type "Peaceful", if ever non hostile
        private static void CheckFactionDefs()
        {
            foreach (var factionDef in DefDatabase<FactionDef>.AllDefsListForReading.Where(f=>!f.isPlayer && !f.hidden && f.CanEverBeNonHostile))
            {
                if (factionDef.pawnGroupMakers?.Any(pgm => pgm.kindDef.defName == "Peaceful") != true)
                {
                    LogMisconfiguration(factionDef, $"FactionDef {factionDef.defName} must have at least one pawnGroupMaker with kindDef 'Peaceful', or 'permanentEnemy', 'isPlayer' or 'hidden' should be set to true. Otherwise no guests from this faction will arrive.");
                }
            }
        }

        private static void CheckChemicalDefs()
        {
            foreach (var def in DefDatabase<ChemicalDef>.AllDefsListForReading.Where(x => x.addictionHediff == null))
            {
                LogMisconfiguration(def, $"The ChemicalDef {def.defName} has no addictionHediff. Remove the ChemicalDef or add an addiction hediff. Otherwise this will cause random groups and raids to not spawn.");
            }
        }

        private static void CheckRoyalTitleDefs()
        {
            foreach (var faction in DefDatabase<FactionDef>.AllDefsListForReading.Where(def=>def.HasRoyalTitles))

            foreach (var title in faction.RoyalTitlesAllInSeniorityOrderForReading)
            {
                if (!title.bedroomRequirements.NullOrEmpty())
                    if (title.bedroomRequirements.Any(req => req == null))
                        LogMisconfiguration(title, $"Bedroom requirement of title '{title.defName}' is null.");

                if (!title.requiredApparel.NullOrEmpty())
                    if (title.requiredApparel.Any(req => req == null))
                        LogMisconfiguration(title, $"Apparel requirement of title '{title.defName}' is null.");
            }
        }

        private static void LogMisconfiguration(Def def, string message)
        {
            //var commaList = LoadedModManager.RunningModsListForReading.Where(m => m.AllDefs.Contains(def)).Select(m => m.Name).ToCommaList(true);
            if (def.modContentPack == null)
            {
                Log.Warning($"{message} This is a misconfiguration in another mod, but the mod name could not be identified (likely a generated def).");
                
            }
            else
            {
                var modName = def.modContentPack.Name;
                Log.Warning($"{message} This is a misconfiguration in {modName}.");
            }
        }
    }
}