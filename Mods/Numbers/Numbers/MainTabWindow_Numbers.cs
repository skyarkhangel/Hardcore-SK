namespace Numbers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;

    public class MainTabWindow_Numbers : MainTabWindow_PawnTable
    {
        public const float buttonWidth = 110f;
        public const float buttonHeight = 35f;
        public const float buttonGap = 4f;
        public const float extraTopSpace = 83f;

        public static List<Func<Pawn, bool>> filterValidator = new List<Func<Pawn, bool>>
                                                        { Find.World.GetComponent<WorldComponent_Numbers>().primaryFilter.Value };

        private readonly IEnumerable<StatDef> pawnHumanlikeStatDef;
        private readonly IEnumerable<StatDef> pawnAnimalStatDef;
        private readonly IEnumerable<StatDef> corpseStatDef;
        private readonly IEnumerable<NeedDef> pawnHumanlikeNeedDef;
        private readonly IEnumerable<NeedDef> pawnAnimalNeedDef;

        private readonly MethodInfo StatsToDraw;

        public readonly OptionsMaker optionsMaker;

        //Code style: Use GetNamedSilentFail in cases where there is null-handling, so any columns that get run through TryGetBestPawnColumnDefLabel() or AddPawnColumnAtBestPositionAndRefresh() can silently fail.
        //Use GetNamed anywhere a null column would throw a null ref.
        private static readonly string workTabName = DefDatabase<MainButtonDef>.GetNamed("Work").ShortenedLabelCap;

        private IEnumerable<StatDef> StatDefs => PawnTableDef.Ext().Corpse ? corpseStatDef :
                        PawnTableDef.Ext().Animallike ? pawnAnimalStatDef : pawnHumanlikeStatDef;

        private IEnumerable<NeedDef> NeedDefs => PawnTableDef.Ext().Animallike ? pawnAnimalNeedDef : pawnHumanlikeNeedDef;

        private IEnumerable<PawnColumnDef> HealthStats
            => new[] { DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_HediffList"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_HediffBadList"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Pain"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Bleedrate"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_NeedsTreatment"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_DiseaseProgress") };

        //ctor to populate lists.
        public MainTabWindow_Numbers()
        {
            optionsMaker = new OptionsMaker(this);

            StatsToDraw = typeof(StatsReportUtility).GetMethod("StatsToDraw",
                                                              BindingFlags.NonPublic | BindingFlags.Static |
                                                              BindingFlags.InvokeMethod, null,
                                                              new[] { typeof(Thing) }, null);

            if (StatsToDraw == null)
                Log.Error("ReflectionTypeLoadException in Numbers: statsToDraw was null. Please contact mod author.");

            if (pawnHumanlikeNeedDef == null)
                pawnHumanlikeNeedDef = GetHumanLikeNeedDef();

            if (pawnHumanlikeStatDef == null)
                pawnHumanlikeStatDef = GetHumanLikeStatDefs();

            if (pawnAnimalNeedDef == null || pawnAnimalStatDef == null || corpseStatDef == null)
            {
                var statDefs = PopulateLists();
                pawnAnimalStatDef = statDefs.pawnAnimalStatDef;
                pawnAnimalNeedDef = statDefs.pawnAnimalNeedDef;
                corpseStatDef = statDefs.corpseStatDef;
            }

            PawnTableDef defaultTable = WorldComponent_Numbers.PrimaryFilter.First().Key;
            if (Find.World.GetComponent<WorldComponent_Numbers>().sessionTable.TryGetValue(defaultTable, out List<PawnColumnDef> list))
                pawnTableDef.columns = list;

            UpdateFilter();
        }

        protected internal PawnTableDef pawnTableDef = NumbersDefOf.Numbers_MainTable;

        protected override PawnTableDef PawnTableDef => pawnTableDef;

        protected override IEnumerable<Pawn> Pawns
        {
            get
            {
                var corpseList = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).Cast<Corpse>();

                foreach (Corpse corpse in corpseList)
                {
                    if (filterValidator.All(validator => validator(corpse.InnerPawn)))
                        yield return corpse.InnerPawn;
                }

                foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned)
                {
                    if (filterValidator.All(validator => validator(pawn)))
                        yield return pawn;
                }
            }
        }

        protected override float ExtraTopSpace => extraTopSpace;

        public override void DoWindowContents(Rect rect)
        {
            float x = 0f;
            Text.Font = GameFont.Small;

            // row count:
            Rect thingCount = new Rect(3f, 40f, 200f, 30f);
            Widgets.Label(thingCount, "koisama.Numbers.Count".Translate() + ": " + Pawns.Count());

            //pawn selector
            Rect sourceButton = new Rect(x, 0f, buttonWidth, buttonHeight);
            DoButton(PawnTableDef.label, optionsMaker.PawnSelector(), ref x);
            TooltipHandler.TipRegion(sourceButton, new TipSignal("koisama.Numbers.ClickToToggle".Translate(), sourceButton.GetHashCode()));

            //stats
            DoButton("TabStats".Translate(), optionsMaker.OptionsMakerForGenericDef(StatDefs), ref x);

            //worktypes
            if (PawnTableDef == NumbersDefOf.Numbers_MainTable)
            {
                DoButton(workTabName, optionsMaker.FloatMenuOptionsFor(DefDatabase<PawnColumnDef>.AllDefsListForReading.Where(pcd => pcd.workType != null).Reverse()), ref x);
            }

            //skills
            if (new[] { NumbersDefOf.Numbers_Enemies, NumbersDefOf.Numbers_Prisoners, NumbersDefOf.Numbers_MainTable }.Contains(PawnTableDef))
            {
                DoButton("Skills".Translate(), optionsMaker.OptionsMakerForGenericDef(DefDatabase<SkillDef>.AllDefsListForReading), ref x);
            }

            //needs btn (for living things)
            if (!new[] { NumbersDefOf.Numbers_AnimalCorpses, NumbersDefOf.Numbers_Corpses }.Contains(PawnTableDef))
            {
                DoButton("TabNeeds".Translate(), optionsMaker.OptionsMakerForGenericDef(NeedDefs), ref x);
            }

            //cap btn (for living things)
            if (!new[] { NumbersDefOf.Numbers_AnimalCorpses, NumbersDefOf.Numbers_Corpses }.Contains(PawnTableDef))
            {
                List<PawnColumnDef> optionalList = new List<PawnColumnDef>();

                if (new[] { NumbersDefOf.Numbers_MainTable, NumbersDefOf.Numbers_Prisoners, NumbersDefOf.Numbers_Animals }.Contains(PawnTableDef))
                {
                    optionalList.Add(DefDatabase<PawnColumnDef>.GetNamedSilentFail("MedicalCare"));
                    optionalList.Add(DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Operations"));

                    if (PawnTableDef == NumbersDefOf.Numbers_MainTable)
                        optionalList.Add(DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_SelfTend"));
                }

                optionalList.AddRange(HealthStats);

                var tmp = optionsMaker.OptionsMakerForGenericDef(DefDatabase<PawnCapacityDef>.AllDefsListForReading)
                                      .Concat(optionsMaker.FloatMenuOptionsFor(optionalList));

                DoButton("TabHealth".Translate(), tmp.ToList(), ref x);
            }

            //abilities btn
            if (ModLister.RoyaltyInstalled)
            {
                DoButton("Abilities".Translate(), optionsMaker.OptionsMakerForGenericDef(DefDatabase<AbilityDef>.AllDefsListForReading.OrderBy(y => y.label)), ref x);
            }

            //records btn
            DoButton("TabRecords".Translate(), optionsMaker.OptionsMakerForGenericDef(DefDatabase<RecordDef>.AllDefsListForReading), ref x);

            //other btn
            DoButton("MiscRecordsCategory".Translate(), optionsMaker.OtherOptionsMaker(), ref x);

            //presets button
            float startPositionOfPresetsButton = Mathf.Max(rect.xMax - buttonWidth - Margin, x);
            Rect addPresetBtn = new Rect(startPositionOfPresetsButton, 0f, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(addPresetBtn, "koisama.Numbers.SetPresetLabel".Translate()))
            {
                Find.WindowStack.Add(new FloatMenu(optionsMaker.PresetOptionsMaker()));
            }

            base.DoWindowContents(rect);
        }

        void DoButton(string label, List<FloatMenuOption> list, ref float x)
        {
            Rect addColumnButton = new Rect(x, 0f, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(addColumnButton, label))
            {
                Find.WindowStack.Add(new FloatMenu(list));
            }
            x += buttonWidth + buttonGap;
        }

        public override void PostOpen()
        {
            UpdateFilter();
            base.PostOpen();
            Find.World.renderer.wantedMode = WorldRenderMode.None;
        }

        public void RefreshAndStoreSessionInWorldComp()
        {
            SetDirty();
            Notify_ResolutionChanged();
            Find.World.GetComponent<WorldComponent_Numbers>().sessionTable[PawnTableDef] = PawnTableDef.columns;
        }

        public void UpdateFilter()
        {
            filterValidator.Clear();
            filterValidator.Insert(0, WorldComponent_Numbers.PrimaryFilter[PawnTableDef]);
        }

        private (List<NeedDef> pawnAnimalNeedDef, List<StatDef> pawnAnimalStatDef, List<StatDef> corpseStatDef) PopulateLists()
        {
            PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Thrumbo, Faction.OfPirates, forceNoIdeo: true);
            Pawn tmpPawn = PawnGenerator.GeneratePawn(request);

            var pawnAnimalNeedDef = tmpPawn.needs.AllNeeds.Where(x => x.def.showOnNeedList).Select(x => x.def).ToList();

            var animalStatDef = ((IEnumerable<StatDrawEntry>)StatsToDraw?.Invoke(null, new[] { tmpPawn })) ?? Enumerable.Empty<StatDrawEntry>();

            var pawnAnimalStatDef = animalStatDef
                           .Where(s => s.stat != null && s.ShouldDisplay && s.stat.Worker != null)
                           .Select(s => s.stat)
                           .OrderBy(stat => stat.LabelCap.Resolve()).ToList();

            Corpse corpse = (Corpse)ThingMaker.MakeThing(tmpPawn.RaceProps.corpseDef);
            corpse.InnerPawn = tmpPawn;

            var deadAnimalStatDef = ((IEnumerable<StatDrawEntry>)StatsToDraw?.Invoke(null, new[] { corpse })) ?? Enumerable.Empty<StatDrawEntry>();

            var corpseStatDef = deadAnimalStatDef
                           .Concat(tmpPawn.def.SpecialDisplayStats(StatRequest.For(tmpPawn)))
                           .Where(s => s.stat != null && s.ShouldDisplay && s.stat.Worker != null)
                           .Select(s => s.stat)
                           .OrderBy(stat => stat.LabelCap.Resolve()).ToList();

            tmpPawn.Destroy(DestroyMode.KillFinalize);
            corpse.Destroy();

            return (pawnAnimalNeedDef, pawnAnimalStatDef, corpseStatDef);
        }

        private List<NeedDef> GetHumanLikeNeedDef() => DefDatabase<NeedDef>.AllDefsListForReading;

        private List<StatDef> GetHumanLikeStatDefs()
        {
            PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.AncientSoldier, Faction.OfAncients, forceNoIdeo: true);
            Pawn tmpPawn = PawnGenerator.GeneratePawn(request);

            var source = ((IEnumerable<StatDrawEntry>)StatsToDraw?.Invoke(null, new[] { tmpPawn })) ?? Enumerable.Empty<StatDrawEntry>();

            var pawnHumanlikeStatDef = source
                           .Concat(tmpPawn.def.SpecialDisplayStats(StatRequest.For(tmpPawn)))
                           .Where(s => s.stat != null && s.ShouldDisplay && s.stat.Worker != null)
                           .Select(s => s.stat);

            tmpPawn.Destroy(DestroyMode.KillFinalize);

            return pawnHumanlikeStatDef.OrderBy(stat => stat.LabelCap.Resolve()).ToList();
        }
    }
}
