using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Fluffy
{
    public class MainTabWindow_Animals : MainTabWindow_PawnList
    {
        //private const float TopAreaHeight = 65f;

        //private const float MasterWidth = 90f;

        //private const float AreaAllowedWidth = 350f;

        public enum Orders
        {
            Default,
            Name,
            Gender,
            LifeStage,
            Slaughter,
            Training
        }

        public static Orders Order = Orders.Default;

        public static TrainableDef TrainingOrder;

        public static bool Asc;

        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(1050f, 65f + PawnsCount * 30f + 65f);
            }
        }

        public static bool IsDirty;

        public static readonly Texture2D[] GenderTextures = {
            ContentFinder<Texture2D>.Get("UI/Gender/none"),
            ContentFinder<Texture2D>.Get("UI/Gender/male"),
            ContentFinder<Texture2D>.Get("UI/Gender/female")
        };

        public static readonly Texture2D[] LifeStageTextures = {
            ContentFinder<Texture2D>.Get("UI/LifeStage/1"),
            ContentFinder<Texture2D>.Get("UI/LifeStage/2"),
            ContentFinder<Texture2D>.Get("UI/LifeStage/3"),
            ContentFinder<Texture2D>.Get("UI/LifeStage/unknown")
        };

        public static readonly Texture2D WorkBoxCheckTex = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxCheck");
        public static readonly Texture2D SlaughterTex = ContentFinder<Texture2D>.Get("UI/Buttons/slaughter");
        private static readonly Texture2D FilterTex = ContentFinder<Texture2D>.Get("UI/Buttons/filter_large");
        private static readonly Texture2D FilterOffTex = ContentFinder<Texture2D>.Get("UI/Buttons/filter_off_large");

        public override void PostOpen()
        {
            base.PostOpen();
            if (Dialog_FilterAnimals.Sticky)
            {
                Find.WindowStack.Add(new Dialog_FilterAnimals());
            }
        }

        protected override void BuildPawnList()
        {
            IEnumerable<Pawn> sorted;
            switch (Order)
            {
                case Orders.Default:
                    sorted = from p in Find.ListerPawns.PawnsInFaction(Faction.OfColony)
                             where p.RaceProps.Animal
                             orderby p.RaceProps.petness descending, p.RaceProps.baseBodySize, p.def.label
                             select p;
                    break;
                case Orders.Name:
                    sorted = from p in Find.ListerPawns.PawnsInFaction(Faction.OfColony)
                             where p.RaceProps.Animal
                             orderby p.Name.Numerical, p.Name.ToStringFull, p.def.label
                             select p;
                    break;
                case Orders.Gender:
                    sorted = from p in Find.ListerPawns.PawnsInFaction(Faction.OfColony)
                             where p.RaceProps.Animal
                             orderby p.KindLabel, p.gender
                             select p;
                    break;
                case Orders.LifeStage:
                    sorted = from p in Find.ListerPawns.PawnsInFaction(Faction.OfColony)
                             where p.RaceProps.Animal
                             orderby p.ageTracker.CurLifeStageRace.minAge descending, p.ageTracker.AgeBiologicalTicks descending
                             select p;
                    break;
                case Orders.Slaughter:
                    sorted = from p in Find.ListerPawns.PawnsInFaction(Faction.OfColony)
                             where p.RaceProps.Animal
                             orderby Find.DesignationManager.DesignationOn(p, DesignationDefOf.Slaughter) != null descending, p.BodySize descending
                             select p;
                    break;
                case Orders.Training:
                    bool dump;
                    sorted = from p in Find.ListerPawns.PawnsInFaction(Faction.OfColony)
                             where p.RaceProps.Animal
                             orderby p.training.IsCompleted(TrainingOrder) descending, p.training.GetWanted(TrainingOrder) descending, p.training.CanAssignToTrain(TrainingOrder, out dump).Accepted descending
                             select p;
                    break;
                default:
                    sorted = from p in Find.ListerPawns.PawnsInFaction(Faction.OfColony)
                             where p.RaceProps.Animal
                             orderby p.RaceProps.petness descending, p.RaceProps.baseBodySize, p.def.label
                             select p;
                    break;
            }

            Pawns = sorted.ToList();

            if (Widgets_Filter.Filter)
            {
                Pawns = Widgets_Filter.FilterAnimals(Pawns);
            }

            if (Asc && Pawns.Count > 1)
            {
                Pawns.Reverse();
            }
            
            IsDirty = false;
        }

        public override void DoWindowContents(Rect fillRect)
        {
            base.DoWindowContents(fillRect);
            Rect position = new Rect(0f, 0f, fillRect.width, 65f);
            GUI.BeginGroup(position);

            // ARRRGGHHH!!!
            // Allow other panels to trigger rebuilding the pawn list. (This took me forever to figure out...)
            if (IsDirty) BuildPawnList();


            Rect filterButton = new Rect(0f, 0f, 200f, Mathf.Round(position.height / 2f));
            Text.Font = GameFont.Small;
            if (Widgets.TextButton(filterButton, "Fluffy.Filter".Translate()))
            {
                if (Event.current.button == 0)
                {
                    Find.WindowStack.Add(new Dialog_FilterAnimals());
                } else if (Event.current.button == 1)
                {
                    List<PawnKindDef> list = Find.ListerPawns.PawnsInFaction(Faction.OfColony).Where(p => p.RaceProps.Animal)
                                                 .Select(p => p.kindDef).Distinct().OrderBy(p => p.LabelCap).ToList();

                    if (list.Count > 0)
                    {
                        List<FloatMenuOption> list2 = new List<FloatMenuOption>();
                        list2.AddRange(list.ConvertAll(p => new FloatMenuOption(p.LabelCap, delegate
                        {
                            Widgets_Filter.QuickFilterPawnKind(p);
                            IsDirty = true;
                        })));
                        Find.WindowStack.Add(new FloatMenu(list2));
                    }
                }
            }
            TooltipHandler.TipRegion(filterButton, "Fluffy.FilterTooltip".Translate());
            Rect filterIcon = new Rect(205f, (filterButton.height - 24f) / 2f, 24f, 24f);
            if (Widgets_Filter.Filter)
            {
                if(Widgets.ImageButton(filterIcon, FilterOffTex))
                {
                    Widgets_Filter.DisableFilter();
                    BuildPawnList();
                    SoundDefOf.ClickReject.PlayOneShotOnCamera();
                }
                TooltipHandler.TipRegion(filterIcon, "Fluffy.DisableFilter".Translate());
            } else if (Widgets_Filter.FilterPossible)
            {
                if (Widgets.ImageButton(filterIcon, FilterTex))
                {
                    Widgets_Filter.EnableFilter();
                    BuildPawnList();
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }
                TooltipHandler.TipRegion(filterIcon, "Fluffy.EnableFilter".Translate());
            }

            float num = 175f;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.LowerCenter;
            Rect rectname = new Rect(0f, 0f, num, position.height + 3f);
            Widgets.Label(rectname, "Fluffy.Name".Translate());
            if (Widgets.InvisibleButton(rectname))
            {
                if (Order == Orders.Name)
                {
                    Asc = !Asc;
                }
                else
                {
                    Order = Orders.Name;
                    Asc = false;
                }
                SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                BuildPawnList();
            }
            Rect highlightName = new Rect(0f, rectname.height - 30f, rectname.width, 30);
            TooltipHandler.TipRegion(highlightName, "Fluffy.SortByName".Translate());
            if (Mouse.IsOver(highlightName))
            {
                GUI.DrawTexture(highlightName, TexUI.HighlightTex);
            }
            
            Rect rect = new Rect(num, rectname.height - 30f, 90f, 30);
            Widgets.Label(rect, "Master".Translate());
            if(Widgets.InvisibleButton(rect)){
                    SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                if (Order == Orders.Default)
                {
                    Asc = !Asc;
                }
                else
                {
                    Order = Orders.Default;
                    Asc = false;
                }
                SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                BuildPawnList();
            }
            TooltipHandler.TipRegion(rect, "Fluffy.SortByPetness".Translate());
            if (Mouse.IsOver(rect))
            {
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            num += 90f;

            float x = 16f;

            Rect recta = new Rect(num, rectname.height - 30f, 50f, 30f);
            Rect recta1 = new Rect(num + 9, 48f, x, x);
            GUI.DrawTexture(recta1, GenderTextures[1]);
            num += 25f;

            Rect recta2 = new Rect(num, 48f, x, x);
            GUI.DrawTexture(recta2, GenderTextures[2]);
            num += 25f;

            if (Widgets.InvisibleButton(recta))
            {
                if (Order == Orders.Gender)
                {
                    Asc = !Asc;
                }
                else
                {
                    Order = Orders.Gender;
                    Asc = false;
                }
                SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                BuildPawnList();
            }
            TooltipHandler.TipRegion(recta, "Fluffy.SortByGender".Translate());
            if (Mouse.IsOver(recta))
            {
                GUI.DrawTexture(recta, TexUI.HighlightTex);
            }

            Rect rectb = new Rect(num, rectname.height - 30f, 50f, 30f);
            Rect rectb1 = new Rect(num + 1, 48f, x, x);
            GUI.DrawTexture(rectb1, LifeStageTextures[0]);
            num += 17f;

            Rect rectb2 = new Rect(num, 48f, x, x);
            GUI.DrawTexture(rectb2, LifeStageTextures[1]);
            num += 16f;

            Rect rectb3 = new Rect(num, 48f, x, x);
            GUI.DrawTexture(rectb3, LifeStageTextures[2]);
            num += 17f;

            if (Widgets.InvisibleButton(rectb))
            {
                if (Order == Orders.LifeStage)
                {
                    Asc = !Asc;
                }
                else
                {
                    Order = Orders.LifeStage;
                    Asc = false;
                }
                SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                BuildPawnList();
            }
            TooltipHandler.TipRegion(rectb, "Fluffy.SortByAge".Translate());
            if (Mouse.IsOver(rectb))
            {
                GUI.DrawTexture(rectb, TexUI.HighlightTex);
            }

            Rect rectc = new Rect(num, rectname.height - 30f, 50f, 30f);
            Rect rectc1 = new Rect(num + 17f, 48f, 16f, 16f);
            GUI.DrawTexture(rectc1, SlaughterTex);
            if (Widgets.InvisibleButton(rectc1))
            {
                if (Event.current.shift)
                {
                    Widgets_Animals.SlaughterAllAnimals(Pawns);
                }
                else
                {
                    if (Order == Orders.Slaughter)
                    {
                        Asc = !Asc;
                    }
                    else
                    {
                        Order = Orders.Slaughter;
                        Asc = false;
                    }
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    BuildPawnList();
                }
            }
            TooltipHandler.TipRegion(rectc, "Fluffy.SortByBodysizeSlaughter".Translate());
            if (Mouse.IsOver(rectc))
            {
                GUI.DrawTexture(rectc, TexUI.HighlightTex);
            }

            num += 50f;
            Rect headers = new Rect(num, rectname.height - 30f, 80f, 30f);
            Widgets_Animals.DoTrainingHeaders(headers, Pawns);

            num += 90f;

            Rect rect2 = new Rect(num, 0f, 350f, Mathf.Round(position.height / 2f));
            Text.Font = GameFont.Small;
            if (Widgets.TextButton(rect2, "ManageAreas".Translate()))
            {
                Find.WindowStack.Add(new Dialog_ManageAreas());
            }
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.LowerCenter;
            Rect rect3 = new Rect(num, position.height - 27f, 350f, 30f);
            Widgets_Animals.DoAllowedAreaHeaders(rect3, Pawns, AllowedAreaMode.Animal);
            GUI.EndGroup();
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, position.height, fillRect.width, fillRect.height - position.height);
            DrawRows(outRect);
        }

        protected override void DrawPawnRow(Rect rect, Pawn p)
        {
            // sizes for stuff
            float x = 16f;

            float heightOffset = (rect.height - x) / 2;
            float widthOffset = (50 - x) / 2;
            
            GUI.BeginGroup(rect);
            float num = 175f;

            if (p.training.IsCompleted(TrainableDefOf.Obedience))
            {
                Rect rect2 = new Rect(num, 0f, 90f, rect.height);
                Rect rect3 = rect2.ContractedBy(2f);
                string label = (p.playerSettings.master == null) ? "NoneLower".Translate() : p.playerSettings.master.LabelBaseShort;
                Text.Font = GameFont.Small;
                if (Widgets.TextButton(rect3, label))
                {
                    TrainableUtility.OpenMasterSelectMenu(p);
                }
            }
            num += 90f;
            
            Rect recta = new Rect(num + widthOffset, heightOffset, x, x);
            Texture2D labelSex = GenderTextures[(int)p.gender];
            TipSignal tipSex = p.gender.ToString();
            GUI.DrawTexture(recta, labelSex);
            TooltipHandler.TipRegion(recta, tipSex);
            num += 50f;

            Rect rectb = new Rect(num + widthOffset, heightOffset, x, x);
            var labelAge = p.RaceProps.lifeStageAges.Count > 3 ? LifeStageTextures[3] : LifeStageTextures[p.ageTracker.CurLifeStageIndex];
            TipSignal tipAge = p.ageTracker.CurLifeStage.LabelCap + ", " + p.ageTracker.AgeBiologicalYears;
            GUI.DrawTexture(rectb, labelAge);
            TooltipHandler.TipRegion(rectb, tipAge);
            num += 50f;

            Rect rectc = new Rect(num, 0f, 50f, 30f);
            Rect rectc1 = new Rect(num + 17f, heightOffset, x, x);
            bool slaughter = Find.DesignationManager.DesignationOn(p, DesignationDefOf.Slaughter) != null;

            if (slaughter)
            {
                GUI.DrawTexture(rectc1, WorkBoxCheckTex);
                TooltipHandler.TipRegion(rectc, "Fluffy.StopSlaughter".Translate());
            } else
            {
                TooltipHandler.TipRegion(rectc, "Fluffy.MarkSlaughter".Translate());
            }
            if (Widgets.InvisibleButton(rectc))
            {
                if (slaughter)
                {
                    Widgets_Animals.UnSlaughterAnimal(p);
                    SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
                }
                else
                {
                    Widgets_Animals.SlaughterAnimal(p);
                    SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
                }
            }
            if (Mouse.IsOver(rectc))
            {
                GUI.DrawTexture(rectc1, TexUI.HighlightTex);
            }

            num += 50f;

            Rect trainingRect = new Rect(num, 0f, 80f, 30f);
            Widgets_Animals.DoTrainingRow(trainingRect, p);

            num += 90f;

            Rect rect4 = new Rect(num, 0f, 350f, rect.height);
            AreaAllowedGUI.DoAllowedAreaSelectors(rect4, p, AllowedAreaMode.Animal);
            GUI.EndGroup();
        }
    }
}
