namespace Numbers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using RimWorld.Planet;
    using Verse;

    public class WorldComponent_Numbers : WorldComponent
    {
        public WorldComponent_Numbers(World world) : base(world)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            primaryFilter = PrimaryFilter.First();

            if (!sessionTable.Any())
            {
                List<string> storedPawnTables = LoadedModManager.GetMod<Numbers>().GetSettings<Numbers_Settings>().storedPawnTableDefs;
                foreach (string item in storedPawnTables)
                {
                    if (item.Split(',')[1] == "Default" && sessionTable.All(x => x.Key.defName != item.Split(',')[0]))
                    {
                        PawnTableDef pawnTableDef = HorribleStringParsersForSaving.TurnCommaDelimitedStringIntoPawnTableDef(item);
                        sessionTable.Add(pawnTableDef, pawnTableDef.columns);
                    }
                }
            }
            NotifySettingsChanged();
        }

        public KeyValuePair<PawnTableDef, Func<Pawn, bool>> primaryFilter;

        private List<PawnColumnDef> loadList;

        public Dictionary<PawnTableDef, List<PawnColumnDef>> sessionTable = new Dictionary<PawnTableDef, List<PawnColumnDef>>();

        public override void ExposeData()
        {
            foreach (PawnTableDef type in DefDatabase<PawnTableDef>.AllDefsListForReading.Where(x => x.HasModExtension<DefModExtension_PawnTableDefs>()))
            {
                switch (Scribe.mode)
                {
                    case LoadSaveMode.Saving:
                        if (sessionTable.TryGetValue(type, out List<PawnColumnDef> workList))
                        {
                            Scribe_Collections.Look(ref workList, "Numbers_" + type, LookMode.Def);
                            sessionTable[type] = workList;
                        }
                        break;

                    case LoadSaveMode.LoadingVars:
                        Scribe_Collections.Look(ref loadList, "Numbers_" + type, LookMode.Def);
                        if (!loadList.NullOrEmpty())
                            sessionTable[type] = loadList;
                        break;
                }
            }
        }

        public static readonly Dictionary<PawnTableDef, Func<Pawn, bool>> PrimaryFilter = new Dictionary<PawnTableDef, Func<Pawn, bool>>
        {
            //{ "All",            (pawn) => true },
            { NumbersDefOf.Numbers_MainTable,      pawn => !pawn.Dead && pawn.IsVisible() && pawn.IsColonist },
            { NumbersDefOf.Numbers_Enemies,        pawn => !pawn.Dead && pawn.IsVisible() && pawn.IsEnemy() },
            { NumbersDefOf.Numbers_Prisoners,      pawn => !pawn.Dead && pawn.IsVisible() && pawn.IsPrisoner },
            { NumbersDefOf.Numbers_Guests,         pawn => !pawn.Dead && pawn.IsVisible() && pawn.IsGuest() },
            { NumbersDefOf.Numbers_Animals,        pawn => !pawn.Dead && pawn.IsVisible() && pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer },
            { NumbersDefOf.Numbers_WildAnimals,    pawn => !pawn.Dead && pawn.IsVisible() && pawn.IsWildAnimal() },
            { NumbersDefOf.Numbers_Corpses,        pawn => pawn.Dead && pawn.IsVisible() && !pawn.RaceProps.Animal },
            { NumbersDefOf.Numbers_AnimalCorpses,  pawn => pawn.Dead && pawn.IsVisible() && pawn.RaceProps.Animal }
        };

        internal void NotifySettingsChanged()
        {
            DefDatabase<MainButtonDef>.GetNamed("Wildlife").tabWindowClass
                = Numbers_Settings.coolerThanTheWildlifeTab
                      ? typeof(MainTabWindow_NumbersWildLife)
                      : StaticConstructorOnGameStart.wildLifeTab;

            DefDatabase<MainButtonDef>.GetNamed("Animals").tabWindowClass
                = Numbers_Settings.coolerThanTheAnimalTab
                      ? typeof(MainTabWindow_NumbersAnimals)
                      : StaticConstructorOnGameStart.animalTab;

            DefDatabase<MainButtonDef>.GetNamed("Wildlife").Notify_ClearingAllMapsMemory();
            DefDatabase<MainButtonDef>.GetNamed("Animals").Notify_ClearingAllMapsMemory();
        }
    }
}
