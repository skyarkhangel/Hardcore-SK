namespace Numbers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using JetBrains.Annotations;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;

    public class Numbers : Mod
    {
        private readonly Numbers_Settings settings;

        private static int reorderableGroupId = -1;

        public Numbers(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("mehni.rimworld.numbers");

#if DEBUG
            Harmony.DEBUG = true;
#endif

            harmony.Patch(AccessTools.Method(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve)),
                postfix: new HarmonyMethod(typeof(Numbers), nameof(Columndefs)));

            harmony.Patch(AccessTools.Method(typeof(PawnColumnWorker), "HeaderClicked"),
                prefix: new HarmonyMethod(typeof(Numbers), nameof(RightClickToRemoveHeader)));

            harmony.Patch(AccessTools.Method(typeof(PawnTable), nameof(PawnTable.PawnTableOnGUI)),
                transpiler: new HarmonyMethod(typeof(Numbers), nameof(MakeHeadersReOrderable)));

            harmony.Patch(AccessTools.Method(typeof(PawnColumnWorker), nameof(PawnColumnWorker.DoHeader)),
                transpiler: new HarmonyMethod(typeof(Numbers), nameof(UseWordWrapOnHeaders)));

            harmony.Patch(AccessTools.Method(typeof(PawnColumnWorker_Text), nameof(PawnColumnWorker_Text.DoCell)),
                transpiler: new HarmonyMethod(typeof(Numbers), nameof(CentreCell)));

            harmony.Patch(AccessTools.Method(typeof(ReorderableWidget), nameof(ReorderableWidget.Reorderable)),
                transpiler: new HarmonyMethod(typeof(Numbers), nameof(ReorderWidgetFromEventToInputTranspiler)));

            //we meet again, Fluffy.
            Type pawnColumWorkerType = GenTypes.GetTypeInAnyAssembly("WorkTab.PawnColumnWorker_WorkType");
            if (pawnColumWorkerType != null && typeof(PawnColumnWorker).IsAssignableFrom(pawnColumWorkerType))
            {
                harmony.Patch(AccessTools.Method(pawnColumWorkerType, "HeaderInteractions"),
                    prefix: new HarmonyMethod(typeof(Numbers), nameof(RightClickToRemoveHeader)));
            }

            //we meet Uuugggg again too. Credit where it's due:
            //  https://github.com/alextd/RimWorld-EnhancementPack/blob/master/Source/PawnTableHighlightSelected.cs
            harmony.Patch(AccessTools.Method(typeof(PawnColumnWorker_Label), nameof(PawnColumnWorker_Label.DoCell)),
                postfix: new HarmonyMethod(typeof(Numbers), nameof(AddHighlightToLabel_PostFix)),
                transpiler: new HarmonyMethod(typeof(Numbers), nameof(AddHighlightToLabel_Transpiler)));

            settings = GetSettings<Numbers_Settings>();
        }

        private static void Columndefs()
        {
            foreach (PawnColumnDef pawnColumnDef in ImpliedPawnColumnDefs())
            {
                DefGenerator.AddImpliedDef(pawnColumnDef);
            }
            var pred = DefDatabase<PawnColumnDef>.GetNamed("Predator");
            pred.sortable = true;
            pred.headerTip = "Predator";
        }

        private static bool RightClickToRemoveHeader(PawnColumnWorker __instance, Rect headerRect, PawnTable table)
        {
            if (Event.current.shift)
                return true;

            if (Event.current.button != 1)
                return true;

            if (table is not PawnTable_NumbersMain numbersTable)
                return true;

            if (!Mouse.IsOver(headerRect))
                return true;

            numbersTable.RemoveColumns(x => ReferenceEquals(__instance, x.Worker));

            if (Find.WindowStack.currentlyDrawnWindow is MainTabWindow_Numbers numbers)
                numbers.RefreshAndStoreSessionInWorldComp();

            return false;
        }

        private static IEnumerable<CodeInstruction> MakeHeadersReOrderable(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MethodInfo recacheIfDirty = AccessTools.Method(typeof(PawnTable), "RecacheIfDirty");
            MethodInfo reorderableGroup = AccessTools.Method(typeof(Numbers), nameof(ReorderableGroup));
            MethodInfo reorderableWidget = AccessTools.Method(typeof(Numbers), nameof(CallReorderableWidget));
            MethodInfo doHeader = AccessTools.Method(typeof(PawnColumnWorker), nameof(PawnColumnWorker.DoHeader));

            var local = generator.DeclareLocal(typeof(int));

            CodeInstruction[] codeInstructions = instructions.ToArray();
            for (int i = 0; i < codeInstructions.Length; i++)
            {
                CodeInstruction instruction = codeInstructions[i];
                if (i > 2 && codeInstructions[i - 1].Calls(recacheIfDirty))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, reorderableGroup);
                    yield return new CodeInstruction(OpCodes.Stloc, local.LocalIndex);
                }

                //IL_0092: callvirt instance class RimWorld.PawnColumnWorker RimWorld.PawnColumnDef::get_Worker()
                //IL_0097: ldloc.s 6 <-- insert here.
                //IL_0099: ldarg.0
                //IL_009a: callvirt instance void RimWorld.PawnColumnWorker::DoHeader(valuetype [UnityEngine.CoreModule]UnityEngine.Rect, class RimWorld.PawnTable)
                if (i < codeInstructions.Length + 3 && instruction.opcode == OpCodes.Ldloc_S && codeInstructions[i + 2].Calls(doHeader))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc, local.LocalIndex);
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Call, reorderableWidget);
                }
                yield return instruction;
            }
        }

        //public because of https://github.com/krypt-lynx/PawnTableGrouped
        public static int ReorderableGroup(PawnTable pawnTable)
        {
            if (pawnTable is not PawnTable_NumbersMain numbersPawnTable)
                reorderableGroupId = int.MinValue;

            if (Event.current.type == EventType.Repaint)
            {
                reorderableGroupId = ReorderableWidget.NewGroup(ReorderAction, ReorderableDirection.Horizontal, new Rect(0f, 0f, UI.screenWidth, UI.screenHeight));
            }
            return reorderableGroupId;
        }

        private static readonly Action<int, int> ReorderAction = delegate (int from, int to)
        {
            var numbers = Find.WindowStack.Windows.OfType<MainTabWindow_Numbers>().FirstOrDefault();

            if (numbers != null)
            {
                PawnColumnDef pawnColumnDef = numbers.pawnTableDef.columns[from];
                numbers.pawnTableDef.columns.Insert(to, pawnColumnDef);
                //if it got inserted at a lower number, the index shifted up 1. If not, stick to the old.
                numbers.pawnTableDef.columns.RemoveAt(from >= to ? from + 1 : from);

                numbers.RefreshAndStoreSessionInWorldComp();
            }
        };

        //public because of https://github.com/krypt-lynx/PawnTableGrouped
        public static void CallReorderableWidget(int groupId, Rect rect)
        {
            if (groupId == int.MinValue)
                return;

            ReorderableWidget.Reorderable(groupId, rect);
        }

        private static IEnumerable<CodeInstruction> UseWordWrapOnHeaders(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo Truncate = AccessTools.Method(typeof(GenText), nameof(GenText.Truncate), new Type[] { typeof(string), typeof(float), typeof(Dictionary<string, string>) });
            MethodInfo WordWrap = AccessTools.Method(typeof(Numbers_Utility), nameof(Numbers_Utility.WordWrapAt));

            var instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].opcode == OpCodes.Ldnull && instructionList[i + 1].Calls(Truncate))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Numbers), nameof(Numbers.GetGameFont)));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Text), nameof(Text.Font)).GetSetMethod());

                    instructionList[i].opcode = OpCodes.Ldarg_2;
                    instructionList[i + 1].operand = WordWrap;
                }
                yield return instructionList[i];
            }
        }

        private static GameFont GetGameFont(PawnTable table) => table is PawnTable_NumbersMain && Numbers_Settings.useTinyFont ? GameFont.Tiny : GameFont.Small;

        private static IEnumerable<CodeInstruction> CentreCell(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo anchorSetter = AccessTools.Property(typeof(Text), nameof(Text.Anchor)).GetSetMethod();
            MethodInfo transpilerHelper = AccessTools.Method(typeof(Numbers), nameof(TranspilerHelper));

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                if (instruction.opcode == OpCodes.Call && instructionList[i + 1].Calls(anchorSetter))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_3); //put Table on stack
                    instruction = new CodeInstruction(OpCodes.Call, transpilerHelper);
                }
                yield return instruction;
            }
        }

        //slight issue with job strings. Meh.
        private static TextAnchor TranspilerHelper(PawnTable table) => table is PawnTable_NumbersMain ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;

        /// <summary>
        /// TOO MUCH OF A MESS TO EXPLAIN
        /// </summary>
        /// <returns>Madness.</returns>
        private static IEnumerable<CodeInstruction> ReorderWidgetFromEventToInputTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MethodInfo GetCurrent = AccessTools.Property(typeof(Event), nameof(Event.current)).GetGetMethod();
            MethodInfo GetRawType = AccessTools.Property(typeof(Event), nameof(Event.rawType)).GetGetMethod();
            MethodInfo NoMouseButtonsPressed = AccessTools.Method(typeof(Numbers), nameof(NoMouseButtonsPressed));
            MethodInfo WasClicked = AccessTools.Method(typeof(Numbers), nameof(WasClicked));

            FieldInfo released = AccessTools.Field(typeof(ReorderableWidget), "released");

            bool yieldNext = true;

            List<CodeInstruction> instructionArr = instructions.ToList();
            for (int i = 0; i < instructionArr.ToArray().Length; i++)
            {
                CodeInstruction instruction = instructionArr[i];
                if (instruction.Calls(GetCurrent))
                {
                    if (instructionArr[i + 1].operand != null && instructionArr[i + 1].Calls(GetRawType))
                    {
                        //L_02bc: Label1
                        //L_02bc: call UnityEngine.Event get_current()
                        //L_02c1: callvirt EventType get_rawType()
                        //L_02c6: ldc.i4.1
                        // =>
                        // call Input.GetMouseButtonUp(1) (or 0)
                        yield return new CodeInstruction(OpCodes.Nop)
                        {
                            labels = new List<Label> { generator.DefineLabel() }
                        };
                        instruction.opcode = OpCodes.Call;
                        instruction.operand = NoMouseButtonsPressed;
                        instructionArr.RemoveAt(i + 1);
                    }
                }
                if (instruction.StoresField(released))
                {
                    yield return instruction;
                    CodeInstruction codeInst = new CodeInstruction(OpCodes.Ldarg_2)
                    {
                        labels = new List<Label> { generator.DefineLabel() }
                    };
                    codeInst.labels.AddRange(instructionArr[i + 1].labels);
                    yield return codeInst;
                    yield return new CodeInstruction(OpCodes.Call, WasClicked);
                    yieldNext = false;
                }

                if (!yieldNext && instruction.opcode == OpCodes.Ldarg_1)
                    yieldNext = true;

                if (yieldNext)
                    yield return instruction;

                if (instruction.Calls(AccessTools.Method(typeof(Mouse), nameof(Mouse.IsOver))))
                    yield return new CodeInstruction(OpCodes.And);
            }
        }

        [UsedImplicitly]
        public static bool NoMouseButtonsPressed()
            => !Input.GetMouseButton(0)
            && !Input.GetMouseButton(1);

        [UsedImplicitly]
        public static bool WasClicked(bool useRightButton)
            => useRightButton && Input.GetMouseButtonDown(1)
            || !useRightButton && Input.GetMouseButtonDown(0);


        private static void AddHighlightToLabel_PostFix(Rect rect, Pawn pawn)
        {
            if (!Numbers_Settings.pawnTableHighlightSelected) return;

            if (Find.Selector.IsSelected(pawn))
                Widgets.DrawHighlightSelected(rect);
        }

        //again, copied from https://github.com/alextd/RimWorld-EnhancementPack/blob/master/Source/PawnTableHighlightSelected.cs
        private static IEnumerable<CodeInstruction> AddHighlightToLabel_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo TryJumpAndSelectInfo = AccessTools.Method(typeof(CameraJumper), nameof(CameraJumper.TryJumpAndSelect));
            MethodInfo EscapeCurrentTabInfo = AccessTools.Method(typeof(MainTabsRoot), nameof(MainTabsRoot.EscapeCurrentTab));

            foreach (CodeInstruction i in instructions)
            {
                if (i.Calls(TryJumpAndSelectInfo))
                    i.operand = AccessTools.Method(typeof(Numbers), nameof(ClickPawn));
                if (i.Calls(EscapeCurrentTabInfo))
                    i.operand = AccessTools.Method(typeof(Numbers), nameof(SodThisImOut));

                yield return i;
            }
        }

        public static void ClickPawn(GlobalTargetInfo target, CameraJumper.MovementMode mode = CameraJumper.MovementMode.Pan)
        {
            if (!Numbers_Settings.pawnTableClickSelect)
            {
                CameraJumper.TryJumpAndSelect(target);
                return;
            }

            if (Current.ProgramState != ProgramState.Playing)
                return;

            if (target.Thing is Pawn pawn && pawn.Spawned)
            {
                if (Event.current.shift)
                {
                    if (Find.Selector.IsSelected(pawn))
                        Find.Selector.Deselect(pawn);
                    else
                        Find.Selector.Select(pawn);
                }
                else if (Event.current.alt)
                {
                    Find.MainTabsRoot.EscapeCurrentTab(false);
                    CameraJumper.TryJumpAndSelect(target);
                }
                else
                {
                    if (Find.Selector.IsSelected(pawn))
                        CameraJumper.TryJump(target);
                    if (!Find.Selector.IsSelected(pawn) || Find.Selector.NumSelected > 1 && Event.current.button == 1)
                    {
                        Find.Selector.ClearSelection();
                        Find.Selector.Select(pawn);
                    }
                }
            }
            else //default
            {
                CameraJumper.TryJumpAndSelect(target);
            }
        }

        public static void SodThisImOut(MainTabsRoot o1, bool o2)
        {
            if (!Numbers_Settings.pawnTableClickSelect)
                o1.EscapeCurrentTab(o2);
        }

        private static NeedDef DummyNeed => DefDatabase<NeedDef>.GetNamedSilentFail("Authority");
        // the xml says
        //  <!-- This is a temporary dummy need to suppress errors when loading older saves. -->
        // temporary my ass.

        private static IEnumerable<PawnColumnDef> ImpliedPawnColumnDefs()
            => DefDatabase<RecordDef>.AllDefsListForReading.Select(GenerateNewPawnColumnDefFor)
                   .Concat(DefDatabase<PawnCapacityDef>
                          .AllDefsListForReading.Select(GenerateNewPawnColumnDefFor))
                   .Concat(DefDatabase<NeedDef>
                          .AllDefsListForReading.Except(DummyNeed).Select(GenerateNewPawnColumnDefFor))
                   .Concat(DefDatabase<StatDef>
                          .AllDefsListForReading.Select(GenerateNewPawnColumnDefFor))
                   .Concat(DefDatabase<SkillDef>
                          .AllDefsListForReading.Select(GenerateNewPawnColumnDefFor))
                   .Concat(DefDatabase<AbilityDef>
                          .AllDefsListForReading.Select(GenerateNewPawnColumnDefFor));

        private static PawnColumnDef GenerateNewPawnColumnDefFor(Def def)
        {
            bool prependDescription = def is not PawnCapacityDef;
            PawnColumnDef pcd = new PawnColumnDef
            {
                defName = HorribleStringParsersForSaving.CreateDefNameFromType(def),
                sortable = true,
                headerTip = (prependDescription ? def.description + "\n\n" : "") + "Numbers_ColumnHeader_Tooltip".Translate(),
                generated = true,
                label = def.LabelCap,
                modContentPack = def.modContentPack,
                modExtensions = new List<DefModExtension> { new DefModExtension_PawnColumnDefs() }
            };
            switch (def)
            {
                case RecordDef recordDef:
                    pcd.workerClass = typeof(PawnColumnWorker_Record);
                    pcd.GetModExtension<DefModExtension_PawnColumnDefs>().record = recordDef;
                    break;
                case PawnCapacityDef pawnCapacityDef:
                    pcd.workerClass = typeof(PawnColumnWorker_Capacity);
                    pcd.GetModExtension<DefModExtension_PawnColumnDefs>().capacity = pawnCapacityDef;
                    break;
                case NeedDef needDef:
                    pcd.workerClass = typeof(PawnColumnWorker_Need);
                    pcd.GetModExtension<DefModExtension_PawnColumnDefs>().need = needDef;
                    break;
                case StatDef statDef:
                    pcd.workerClass = typeof(PawnColumnWorker_Stat);
                    pcd.GetModExtension<DefModExtension_PawnColumnDefs>().stat = statDef;
                    break;
                case SkillDef skillDef:
                    pcd.workerClass = typeof(PawnColumnWorker_Skill);
                    pcd.GetModExtension<DefModExtension_PawnColumnDefs>().skill = skillDef;
                    break;
                case AbilityDef abilityDef:
                    pcd.workerClass = typeof(PawnColumnWorker_Ability);
                    pcd.GetModExtension<DefModExtension_PawnColumnDefs>().ability = abilityDef;
                    break;
                default:
                    throw new ArgumentException($"Unsupported Def of type {def.GetType()}");
            }

            return pcd;
        }


        private static Vector2 scrollPosition;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Numbers_showMoreInfoThanVanilla".Translate(), ref Numbers_Settings.showMoreInfoThanVanilla);
            listingStandard.CheckboxLabeled("Numbers_coolerThanTheWildlifeTab".Translate(), ref Numbers_Settings.coolerThanTheWildlifeTab);
            listingStandard.CheckboxLabeled("Numbers_coolerThanTheAnimalTab".Translate(), ref Numbers_Settings.coolerThanTheAnimalTab);
            listingStandard.CheckboxLabeled("Numbers_pawnTableClickSelect".Translate(), ref Numbers_Settings.pawnTableClickSelect, "Numbers_pawnTableClickSelect_Desc".Translate());
            listingStandard.CheckboxLabeled("Numbers_pawnTableHighSelected".Translate(), ref Numbers_Settings.pawnTableHighlightSelected, "Numbers_pawnTableHighSelected_Desc".Translate());
            listingStandard.CheckboxLabeled("Numbers_useSmolFont".Translate(), ref Numbers_Settings.useTinyFont);
            listingStandard.SliderLabeled("Numbers_maxTableHeight".Translate(), ref Numbers_Settings.maxHeight, Numbers_Settings.maxHeight.ToStringPercent(), 0.3f);
            listingStandard.End();

            float rowHeight = 20f;
            float buttonHeight = 16f;

            float height = RegenPawnTableDefsFromSettings().Count * rowHeight;
            Rect outRect;
            float num = 0f;
            int num2 = 0;

            //erdelf unknowningly to the rescue
            Widgets.BeginScrollView(inRect.BottomPart(0.6f).TopPart(0.8f), ref scrollPosition, outRect = new Rect(inRect.x, inRect.y - rowHeight * 2, inRect.width - 18f, height));
            List<string> list = RegenPawnTableDefsFromSettings();
            for (int i = 0; i < list.Count; i++)
            {
                string current = list[i];

                if (num + rowHeight >= scrollPosition.y && num <= scrollPosition.y + outRect.height)
                {
                    Rect rect = new Rect(0f, num, outRect.width, rowHeight);
                    if (num2 % 2 == 0)
                    {
                        Widgets.DrawAltRect(rect);
                    }
                    GUI.BeginGroup(rect);
                    Rect rect2 = new Rect(rect.width - buttonHeight, (rect.height - buttonHeight) / 2f, buttonHeight, buttonHeight);
                    if (Widgets.ButtonImage(rect2, StaticConstructorOnGameStart.DeleteX, Color.white, GenUI.SubtleMouseoverColor))
                    {
                        settings.storedPawnTableDefs.RemoveAt(i);
                    }
                    TooltipHandler.TipRegion(rect2, "delet this");

                    Rect rect5 = new Rect(0, 0, rect.width - rowHeight - 2f, rect.height);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Text.Font = GameFont.Small;

                    Widgets.Label(rect5, current);
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                    GUI.EndGroup();
                }
                num += rowHeight;
                num2++;
            }
            Widgets.EndScrollView();
            //writing is done by closing the window.
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            Find.World?.GetComponent<WorldComponent_Numbers>()?.NotifySettingsChanged();
        }

        private readonly List<string> cachedList = new List<string>();

        private List<string> RegenPawnTableDefsFromSettings()
        {
            cachedList.Clear();

            foreach (string storedPawnTableDef in settings.storedPawnTableDefs)
            {
                if (storedPawnTableDef.Split(',')[1] == "Default")
                    cachedList.Add(storedPawnTableDef.Split(',')[0].Split('_')[1] + " (" + storedPawnTableDef.Split(',')[1] + ")");
                //Numbers_MainTable,Default => MainTable (Default)
                else
                    cachedList.Add(storedPawnTableDef.Split(',')[1]);
            }
            return cachedList;
        }

        public override string SettingsCategory() => "Numbers";
    }
}
