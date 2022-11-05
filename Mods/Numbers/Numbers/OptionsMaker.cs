namespace Numbers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class OptionsMaker
    {
        private readonly MainTabWindow_Numbers numbers;
        private readonly Numbers_Settings settings;

        private PawnColumnDef _allowedareawide;
        private PawnColumnDef AllowedAreaWide => _allowedareawide ??= _allowedareawide = DefDatabase<PawnColumnDef>.GetNamedSilentFail("AllowedAreaWide");

        private PawnColumnDef _allowedarea;
        private PawnColumnDef AllowedArea => _allowedarea ??= _allowedarea = DefDatabase<PawnColumnDef>.GetNamedSilentFail("AllowedArea");

        private PawnTableDef PawnTable
        {
            get => numbers.pawnTableDef;
            set => numbers.pawnTableDef = value;
        }

        private static readonly Func<PawnColumnDef, bool> filterRoyalty = pcd => ModLister.RoyaltyInstalled || !pcd.HasModExtension<DefModExtension_NeedsRoyalty>();
        private static readonly Func<PawnColumnDef, bool> filterBioTech = pcd => ModLister.BiotechInstalled || !pcd.HasModExtension<DefModExtension_NeedsBioTech>();
        private static readonly Func<PawnColumnDef, bool> filterIdeology = pcd => ModLister.BiotechInstalled || !pcd.HasModExtension<DefModExtension_NeedsIdeology>();

        public OptionsMaker(MainTabWindow_Numbers mainTabWindow)
        {
            numbers = mainTabWindow;
            settings = LoadedModManager.GetMod<Numbers>().GetSettings<Numbers_Settings>();
        }

        public List<FloatMenuOption> PresetOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>
            {
                new FloatMenuOption("Numbers_SaveCurrentLayout".Translate(), Save),
                new FloatMenuOption("Numbers_LoadSavedLayout".Translate(), Load),
                new FloatMenuOption("Numbers_Presets.Load".Translate("Numbers_Presets.Medical".Translate()), () => ChangeMainTableTo(StaticConstructorOnGameStart.medicalPreset)),
                new FloatMenuOption("Numbers_Presets.Load".Translate("Numbers_Presets.Combat".Translate()), () => ChangeMainTableTo(StaticConstructorOnGameStart.combatPreset)),
                new FloatMenuOption("Numbers_Presets.Load".Translate("Numbers_Presets.WorkTabPlus".Translate()), () => ChangeMainTableTo(StaticConstructorOnGameStart.workTabPlusPreset)),
                new FloatMenuOption("Numbers_Presets.Load".Translate("Numbers_Presets.ColonistNeeds".Translate()), () => ChangeMainTableTo(StaticConstructorOnGameStart.colonistNeedsPreset)),
                //maybe Psycasting here, index 6, referenced below
                new FloatMenuOption("Numbers_SetAsDefault".Translate(), SetAsDefault,
                        extraPartWidth: 29f,
                        extraPartOnGUI: rect => Numbers_Utility.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2, "Numbers_SetAsDefaultExplanation".Translate(PawnTable.LabelCap))),
                new FloatMenuOption("Numbers_LoadDefault".Translate(), LoadDefault)
            };
            if (ModLister.RoyaltyInstalled)
            {
                list.Insert(6, new FloatMenuOption("Numbers_Presets.Load".Translate("Numbers_Presets.Psycasting".Translate()), () => ChangeMainTableTo(StaticConstructorOnGameStart.psycastingPreset)));
            }

            return list;
        }

        private IEnumerable<FloatMenuOption> General()
        {
            yield return new FloatMenuOption("Race".Translate(), () => AddPawnColumnAtBestPositionAndRefresh(DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Race")));
            yield return new FloatMenuOption("Faction".Translate(), () => AddPawnColumnAtBestPositionAndRefresh(DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Faction")));
            yield return new FloatMenuOption("Gender", () => AddPawnColumnAtBestPositionAndRefresh(DefDatabase<PawnColumnDef>.GetNamedSilentFail("Gender")));
        }

        public List<FloatMenuOption> FloatMenuOptionsFor(IEnumerable<PawnColumnDef> pcdList)
            => pcdList.Select(pcd => new FloatMenuOption(GetBestLabelForPawnColumn(pcd), () => AddPawnColumnAtBestPositionAndRefresh(pcd))).ToList();

        public List<FloatMenuOption> OtherOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            list.AddRange(General());

            //equipment bearers
            if (new[] { NumbersDefOf.Numbers_MainTable,
                        NumbersDefOf.Numbers_Prisoners,
                        NumbersDefOf.Numbers_Enemies,
                        NumbersDefOf.Numbers_Corpses
                      }.Contains(PawnTable))
            {
                list.AddRange(FloatMenuOptionsFor(PawnColumnOptionDefOf.EquipmentBearers.options));
            }

            //all living things
            if (!new[] { NumbersDefOf.Numbers_AnimalCorpses, NumbersDefOf.Numbers_Corpses }.Contains(PawnTable))
            {
                list.AddRange(FloatMenuOptionsFor(PawnColumnOptionDefOf.LivingThings.options));
            }

            if (PawnTable == NumbersDefOf.Numbers_Prisoners)
            {
                list.AddRange(FloatMenuOptionsFor(PawnColumnOptionDefOf.Prisoners.options));
            }

            if (PawnTable == NumbersDefOf.Numbers_Animals)
            {
                list.AddRange(FloatMenuOptionsFor(PawnColumnOptionDefOf.Animals.options
                    .Concat(DefDatabase<PawnTableDef>.GetNamed("Animals").columns)
                    .Where(x => pcdValidator(x))
                    .Except(new[] { AllowedArea, AllowedAreaWide })));
            }

            if (PawnTable == NumbersDefOf.Numbers_MainTable)
            {
                list.AddRange(FloatMenuOptionsFor(PawnColumnOptionDefOf.MainTable.options
                    .Concat(DefDatabase<PawnTableDef>.GetNamed("Assign").columns)
                    .Concat(DefDatabase<PawnTableDef>.GetNamed("Restrict").columns)
                    .Where(x => pcdValidator(x))
                    .Except(new[] { AllowedArea, AllowedAreaWide })));
            }

            if (PawnTable == NumbersDefOf.Numbers_WildAnimals)
            {
                list.RemoveAll(x => x.Label == "Gender"); //duplicate
                list.AddRange(FloatMenuOptionsFor(PawnColumnOptionDefOf.WildAnimals.options
                    .Concat(DefDatabase<PawnTableDef>.GetNamed("Wildlife").columns)
                    .Where(x => pcdValidator(x))));
            }

            //all dead things
            if (new[] { NumbersDefOf.Numbers_AnimalCorpses, NumbersDefOf.Numbers_Corpses }.Contains(PawnTable))
            {
                list.AddRange(FloatMenuOptionsFor(PawnColumnOptionDefOf.DeadThings.options));
            }

            return list.OrderBy(x => x.Label).ToList();
        }

        public List<FloatMenuOption> OptionsMakerForGenericDef<T>(IEnumerable<T> listOfDefs) where T : Def
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            foreach (var defCurrent in listOfDefs)
            {
                void Action()
                {
                    PawnColumnDef pcd = DefDatabase<PawnColumnDef>.GetNamedSilentFail(HorribleStringParsersForSaving.CreateDefNameFromType(defCurrent));
                    AddPawnColumnAtBestPositionAndRefresh(pcd);
                }
                string label = defCurrent.LabelCap;
                list.Add(new FloatMenuOption(label, Action));
            }

            return list;
        }

        public List<FloatMenuOption> PawnSelector()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (KeyValuePair<PawnTableDef, Func<Pawn, bool>> filter in WorldComponent_Numbers.PrimaryFilter)
            {
                void Action()
                {
                    if (filter.Value != MainTabWindow_Numbers.filterValidator.First())
                    {
                        PawnTable = filter.Key;

                        if (Find.World.GetComponent<WorldComponent_Numbers>().sessionTable.TryGetValue(filter.Key, out List<PawnColumnDef> listPawnColumDef))
                            PawnTable.columns = listPawnColumDef;

                        numbers.UpdateFilter();
                        numbers.Notify_ResolutionChanged();
                    }
                }
                list.Add(new FloatMenuOption(filter.Key.label, Action));
            }
            return list;
        }

        private void Save()
        {
            //not actually saved like this, just the easiest way to pass it around
            PawnTableDef ptdPawnTableDef = new PawnTableDef
            {
                columns = PawnTable.columns,
                modContentPack = PawnTable.modContentPack,
                workerClass = PawnTable.workerClass,
                defName = PawnTable.defName,
                label = "NumbersTable" + Rand.Range(0, 10000)
            };
            Find.WindowStack.Add(new Dialog_IHaveToCreateAnEntireFuckingDialogForAGODDAMNOKAYBUTTONFFS(ref ptdPawnTableDef));
        }

        private void Load()
        {
            List<FloatMenuOption> loadOptions = new List<FloatMenuOption>();
            foreach (string tableDefToBe in settings.storedPawnTableDefs)
            {
                void ApplySetting()
                {
                    PawnTableDef ptD = HorribleStringParsersForSaving.TurnCommaDelimitedStringIntoPawnTableDef(tableDefToBe);

                    PawnTable = DefDatabase<PawnTableDef>.GetNamed(ptD.defName);
                    PawnTable.columns = ptD.columns;

                    numbers.UpdateFilter();
                    numbers.RefreshAndStoreSessionInWorldComp();
                }
                string label = tableDefToBe.Split(',')[1] == "Default" ? tableDefToBe.Split(',')[0].Split('_')[1] + " (" + tableDefToBe.Split(',')[1] + ")" : tableDefToBe.Split(',')[1];
                loadOptions.Add(new FloatMenuOption(label, ApplySetting));
            }

            if (loadOptions.NullOrEmpty())
                loadOptions.Add(new FloatMenuOption("Numbers_NothingSaved".Translate(), null));

            Find.WindowStack.Add(new FloatMenu(loadOptions));
        }

        private void ChangeMainTableTo(List<PawnColumnDef> list)
        {
            PawnTable = NumbersDefOf.Numbers_MainTable;
            PawnTable.columns = new List<PawnColumnDef>(list);
            numbers.UpdateFilter();
            numbers.Notify_ResolutionChanged();
        }

        private void SetAsDefault()
        {
            string pawnTableDeftoSave = HorribleStringParsersForSaving.TurnPawnTableDefIntoCommaDelimitedString(PawnTable, true);
            settings.StoreNewPawnTableDef(pawnTableDeftoSave);
        }

        private void LoadDefault()
        {
            bool foundSomething = false;
            foreach (string tableDefToBe in settings.storedPawnTableDefs)
            {
                string[] ptdToBe = tableDefToBe.Split(',');
                if (ptdToBe[1] == "Default" && PawnTable.defName == ptdToBe[0])
                {
                    foundSomething = true;
                    PawnTableDef ptD = HorribleStringParsersForSaving.TurnCommaDelimitedStringIntoPawnTableDef(tableDefToBe);

                    PawnTable = DefDatabase<PawnTableDef>.GetNamed(ptD.defName);
                    PawnTable.columns = ptD.columns;
                    numbers.UpdateFilter();
                    numbers.RefreshAndStoreSessionInWorldComp();
                    break;
                }
            }
            if (!foundSomething)
                Messages.Message("Numbers_NoDefaultStoredForThisView".Translate(), MessageTypeDefOf.RejectInput);
        }

        private static string GetBestLabelForPawnColumn(PawnColumnDef pcd)
        {
            if (pcd == null)
                return string.Empty;

            if (pcd.workType != null)
                return pcd.workType.labelShort;

            if (!pcd.LabelCap.NullOrEmpty())
                return pcd.LabelCap;

            if (!pcd.headerTip.NullOrEmpty())
                return pcd.headerTip;

            return pcd.defName;
        }

        private void AddPawnColumnAtBestPositionAndRefresh(PawnColumnDef pcd)
        {
            if (pcd == null)
                return;
            int lastIndex = PawnTable.columns.FindLastIndex(x => x.Worker is PawnColumnWorker_RemainingSpace);
            PawnTable.columns.Insert(Mathf.Max(1, lastIndex), pcd);

            numbers.RefreshAndStoreSessionInWorldComp();
        }

        private static readonly Func<PawnColumnDef, bool> pcdValidator = pcd =>
        {
            try
            {
                return pcd.Worker is not PawnColumnWorker_Gap
                    && pcd.Worker is not PawnColumnWorker_Label
                    && pcd.Worker is not PawnColumnWorker_RemainingSpace
                    && pcd.Worker is not PawnColumnWorker_CopyPaste
                    && pcd.Worker is not PawnColumnWorker_MedicalCare
                    && pcd.Worker is not RimWorld.PawnColumnWorker_Ideo //definitely don't want the vanilla one.
                    && pcd.Worker is not RimWorld.PawnColumnWorker_MentalState //definitely don't want the vanilla one.
                    && pcd.Worker is not PawnColumnWorker_Timetable
                    || (!(pcd.label.NullOrEmpty() && pcd.HeaderIcon == null) && !pcd.HeaderInteractable)
                    && filterRoyalty(pcd)
                    && filterIdeology(pcd)
                    && filterBioTech(pcd);
            }
            catch (ArgumentNullException ex)
            {
                Log.Error($"{pcd.defName} from {pcd.modContentPack.Name} threw ArgumentNullException {ex}");
                return false;
            }
        };
        //basically all that are already present, don't have an interactable header, and uh
    }
}
