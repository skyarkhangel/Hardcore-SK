using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Fluffy
{
    public class Dialog_FilterAnimals : Window
    {
        public List<PawnKindDef> PawnKinds;

        public static bool Sticky;

        public static Rect Location;

        public override void PreClose()
        {
            base.PreClose();
            Location = currentWindowRect;
        }

        // kinda hacky, but sticky can't be set without opening, which populates location.
        public override void PostOpen()
        {
            base.PostOpen();
            if (Sticky)
            {
                currentWindowRect = Location;
            }
        }

        private float _actualHeight = 500f;

        public override Vector2 InitialWindowSize
        {
            get
            {
                return new Vector2(600f, 65f + Mathf.Max(200f, _actualHeight));
            }
        }

        public Dialog_FilterAnimals()
        {
            PawnKinds = Find.ListerPawns.PawnsInFaction(Faction.OfColony).Where(x => x.RaceProps.Animal)
                .Select(x => x.kindDef).Distinct().OrderBy(x => x.LabelCap).ToList();
            forcePause = true;
            closeOnEscapeKey = true;
            absorbInputAroundWindow = false;
            draggable = true;
        }

        float ColWidth
        {
            get
            {
                return (InitialWindowSize.x / 2f) - 10f;
            }
        }

        float _rowHeight = 30f;
        float _iconSize = 24f;
        float LabWidth => ColWidth - 50f;
        float _iconWidthOffset = (50f - 24f) / 2f;
/*
        float IconHeightOffset => (_rowHeight - _iconSize) / 2f;
*/

        float _x, _y, _x2;

        public override void DoWindowContents(Rect inRect)
        {
            // Close if animals tab closed
            if (Find.WindowStack.WindowOfType<MainTabWindow_Animals>() == null)
            {
                Find.WindowStack.TryRemove(this);
            }

            Text.Font = GameFont.Small;
            

            // Pawnkinds on the left.
            _x = 5f;
            _y = 5f;
            _x2 = ColWidth - 45f;
            
            Rect rect = new Rect(_x, _y, ColWidth, _rowHeight);
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.LowerLeft;
            Widgets.Label(rect, "Fluffy.FilterByRace".Translate());
            Text.Font = GameFont.Small;


            _y += _rowHeight;

            foreach (PawnKindDef pawnKind in PawnKinds)
            {
                DrawPawnKindRow(pawnKind);
            }

            // set window's actual height
            if (_y > _actualHeight) _actualHeight = _y;

            // specials on the right.
            _x = inRect.width / 2f + 5f;
            _x2 = inRect.width / 2f + ColWidth - 45f;
            _y = 5f;

            Text.Font = GameFont.Tiny;
            Rect rectAttributes = new Rect(_x, 5f, ColWidth, _rowHeight);
            Widgets.Label(rectAttributes, "Fluffy.FilterByAttributes".Translate());
            Text.Font = GameFont.Small;

            _y += _rowHeight;

            // draw filter rows.
            foreach (IFilter filter in Widgets_Filter.Filters)
            {
                DrawFilterRow(filter);
            }

            // set window's actual height
            if (_y > _actualHeight) _actualHeight = _y;

            // sticky option
            Rect stickyRect = new Rect(5f, inRect.height - 35f, (inRect.width / 4) - 10, 35f);
            Widgets.LabelCheckbox(stickyRect, "Fluffy.FilterSticky".Translate(), ref Sticky);


            // buttons
            if (Widgets.TextButton(new Rect(inRect.width / 4f + 5f, inRect.height - 35f, inRect.width / 4f - 10f, 35f),
                                            "Fluffy.Clear".Translate()))
            {
                Widgets_Filter.ResetFilter();
                Widgets_Filter.DisableFilter();
                MainTabWindow_Animals.IsDirty = true;
                Event.current.Use();
            }

            if (!Widgets_Filter.Filter)
            {
                if (Widgets.TextButton(new Rect(_x, inRect.height - 35f, inRect.width / 4f - 10f, 35f),
                                            "Fluffy.Enable".Translate()))
                {
                    Widgets_Filter.EnableFilter();
                    MainTabWindow_Animals.IsDirty = true;
                    Event.current.Use();
                }
            }
            else
            {
                if (Widgets.TextButton(new Rect(_x, inRect.height - 35f, inRect.width / 4f - 10f, 35f),
                                            "Fluffy.Disable".Translate()))
                {
                    Widgets_Filter.DisableFilter();
                    MainTabWindow_Animals.IsDirty = true;
                    Event.current.Use();
                }
            }


            if (Widgets.TextButton(new Rect(_x + inRect.width / 4, inRect.height - 35f, inRect.width / 4f - 10f, 35f),
                                            "OK".Translate()))
            {
                Find.WindowStack.TryRemove(this);
                Event.current.Use();
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }

        public void DrawPawnKindRow(PawnKindDef pawnKind)
        {
            Rect rectRow = new Rect(_x, _y, ColWidth, _rowHeight);
            Rect rectLabel = new Rect(_x, _y, LabWidth, _rowHeight);
            Widgets.Label(rectLabel, pawnKind.LabelCap);
            Rect rectIcon = new Rect(_x2 + _iconWidthOffset, _y, _iconSize, _iconSize);
            bool inList = Widgets_Filter.FilterPawnKind.Contains(pawnKind);
            GUI.DrawTexture(rectIcon, inList ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
            if (Mouse.IsOver(rectRow))
            {
                GUI.DrawTexture(rectRow, TexUI.HighlightTex);
            }
            if (Widgets.InvisibleButton(rectRow))
            {
                if (inList)
                {
                    SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
                }
                else
                {
                    SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
                }
                Widgets_Filter.TogglePawnKindFilter(pawnKind, inList);
                MainTabWindow_Animals.IsDirty = true;
            }
            _y += _rowHeight;
        }

        public void DrawFilterRow(IFilter filter)
        {
            var label = new StringBuilder();
            label.Append(("Fluffy." + filter.label + "Label").Translate()).Append(" ");
            Rect rect = new Rect(_x, _y, ColWidth, _rowHeight);
            Rect rectLabel = new Rect(_x, _y, LabWidth, _rowHeight);
            Rect rectIcon = new Rect(_x2 + _iconWidthOffset, _y, _iconSize, _iconSize);
            switch (filter.state)
            {
                case FilterType.True:
                    GUI.DrawTexture(rectIcon, filter.textures[0]);
                    label.Append("(").Append(("Fluffy." + filter.label + "Yes").Translate()).Append(")");
                    break;
                case FilterType.False:
                    GUI.DrawTexture(rectIcon, filter.textures[1]);
                    label.Append("(").Append(("Fluffy." + filter.label + "No").Translate()).Append(")");
                    break;
                default:
                    GUI.DrawTexture(rectIcon, filter.textures[2]);
                    label.Append("(").Append(("Fluffy.Both").Translate()).Append(")");
                    break;
            }
            Widgets.Label(rectLabel, label.ToString());
            if (Widgets.InvisibleButton(rect))
            {
                filter.bump();
                SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                MainTabWindow_Animals.IsDirty = true;
            }
            if (Mouse.IsOver(rect))
            {
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            _y += _rowHeight;
        }
    }
}