using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorldDaysMatter
{
    public class DaysMatterTab : MainTabWindow
    {
        private const int ROW_HEIGHT = 32;
        private const int COL_MARGIN = 5;
        private const int ROW_ADD_GAP = 5;
        private const int STATIC_ROW_COUNT = 4;

        private const int SCROLLBAR_WIDTH = 16;

        private const int ROW_ADD_WIDTH = 600;

        private const int LBL_DATE_WIDTH = 50;
        private const int BTN_DATE_CONTROL_WIDTH = 12;
        private const int BTN_SEASON_WIDTH = 100;
        private const int BTN_DURATION_WIDTH = 100;
        private const int BTN_DELETE_WIDTH = 24;
        private const int BTN_DELETE_HEIGHT = 24;
        private const int BTN_ADD_WIDTH = 100;

        private static readonly List<Quadrum> QUADRUMS = new List<Quadrum> { Quadrum.Aprimay, Quadrum.Jugust, Quadrum.Septober, Quadrum.Decembary };

        private Vector2 _scrollPosition = Vector2.zero;

        private string _newEventName = "";
        private string _newEventDateBuffer = "1";
        private int _newEventDay = 1;
        private Quadrum _newEventQuadrum = Quadrum.Aprimay;
        private Duration _newEventDuration = Duration.Evening;

        private readonly DaysMatterWorldComponent _store;

        public DaysMatterTab()
        {
            layer = WindowLayer.GameUI;
            // closeOnClickedOutside = true;
            // closeOnEscapeKey = true;
            // forcePause = true;
            //_store = UtilityWorldObjectManager.GetUtilityWorldObject<MatteredDayStore>();
            _store = Find.World.GetComponent<DaysMatterWorldComponent>();
        }

        public override Vector2 RequestedTabSize => new Vector2(1010f, 425f);

        public override void DoWindowContents(Rect rect)
        {
            const float scrollRectOffsetTop = ROW_HEIGHT * 2 + ROW_ADD_GAP;

            Text.Font = GameFont.Small;

            // draw event add row
            Rect rectAdd = new Rect(0f, 0f, ROW_ADD_WIDTH, ROW_HEIGHT);
            DrawRowAdd(rectAdd, () =>
            {
                _scrollPosition = new Vector2(0, ROW_HEIGHT * _store.DMMatteredDays.Count + ROW_HEIGHT * STATIC_ROW_COUNT - (rect.height - scrollRectOffsetTop));
            });

            // draw header
            Rect rectHeader = new Rect(0f, ROW_HEIGHT + ROW_ADD_GAP, rect.width - SCROLLBAR_WIDTH, ROW_HEIGHT);
            DrawHeader(rectHeader);

            // draw list
            if (_store.DMMatteredDays == null)
                _store.DMMatteredDays = new List<MatteredDay>();
            List<MatteredDay> list = _store.DMMatteredDays;

            Rect scrollRect = new Rect(0f, scrollRectOffsetTop, rect.width, rect.height - scrollRectOffsetTop);
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - SCROLLBAR_WIDTH, ROW_HEIGHT * list.Count + ROW_HEIGHT * STATIC_ROW_COUNT);

            Widgets.BeginScrollView(scrollRect, ref _scrollPosition, viewRect);
            Vector2 cur = Vector2.zero;

            // settlement
            //int startTicks = Find.TickManager.gameStartAbsTick;
            //Quadrum settlementQuadrum = GenDate.Quadrum(startTicks, 0);
            //int settlementDay = GenDate.DayOfQuadrum(startTicks, 0) + 1;
            DrawRowWithNoDate(new Rect(0f, cur.y, viewRect.width, ROW_HEIGHT), ref cur, "DM.Tab.BuiltIn.Settlement".Translate(), /*settlementQuadrum, settlementDay,*/ _store.DMSettlement, () =>
            {
                FloatMenuUtility.MakeMenu(Enum.GetValues(typeof(Duration)).Cast<Duration>(), duration => duration.Label(), duration => delegate
                {
                    _store.DMSettlement = duration;
                });
            }, "DM.Tab.Misc.ShowAll".Translate(), () =>
            {
                Find.WindowStack.Add(new DialogList(DialogList.ListType.Settlements));
            });

            // birthdays
            DrawRowWithNoDate(new Rect(0f, cur.y, viewRect.width, ROW_HEIGHT), ref cur, "DM.Tab.BuiltIn.ChronologicalBirthday".Translate(), _store.DMBirthdays, () =>
            {
                FloatMenuUtility.MakeMenu(Enum.GetValues(typeof(Duration)).Cast<Duration>(), duration => duration.Label(), duration => delegate
                {
                    _store.DMBirthdays = duration;
                });
            }, "DM.Tab.Misc.ShowAll".Translate(), () =>
            {
                Find.WindowStack.Add(new DialogList(DialogList.ListType.Birthdays));
            });

            // lovers anniversaries
            DrawRowWithNoDate(new Rect(0f, cur.y, viewRect.width, ROW_HEIGHT), ref cur, "DM.Tab.BuiltIn.RelationshipAnniversary".Translate(), _store.DMLoversAnniversaries, () =>
            {
                FloatMenuUtility.MakeMenu(Enum.GetValues(typeof(Duration)).Cast<Duration>(), duration => duration.Label(), duration => delegate
                {
                    _store.DMLoversAnniversaries = duration;
                });
            }, "DM.Tab.Misc.ShowAll".Translate(), () =>
            {
                Find.WindowStack.Add(new DialogList(DialogList.ListType.Relationships));
            });

            // marriage anniversaries
            DrawRowWithNoDate(new Rect(0f, cur.y, viewRect.width, ROW_HEIGHT), ref cur, "DM.Tab.BuiltIn.MarriageAnniversary".Translate(), _store.DMMarriageAnniversaries, () =>
            {
                FloatMenuUtility.MakeMenu(Enum.GetValues(typeof(Duration)).Cast<Duration>(), duration => duration.Label(), duration => delegate
                {
                    _store.DMMarriageAnniversaries = duration;
                });
            }, "DM.Tab.Misc.ShowAll".Translate(), () =>
            {
                Find.WindowStack.Add(new DialogList(DialogList.ListType.Marriages));
            });

            for (int index = 0; index < list.Count; index++)
            {
                if (list.Count <= index)
                    break;

                var row = new Rect(0f, cur.y, viewRect.width, ROW_HEIGHT);

                Widgets.DrawHighlightIfMouseover(row);
                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                Widgets.DrawLineHorizontal(0f, cur.y, viewRect.width);
                GUI.color = Color.white;

                DrawRow(row, list[index], index);

                cur.y += ROW_HEIGHT;
            }
            Widgets.EndScrollView();
        }

        private void DrawRowWithNoDate(Rect rect, ref Vector2 cur, string name, Duration duration, Action action, string seeAllText, Action seeAllAction)
        {
            Widgets.DrawHighlightIfMouseover(rect);
            GUI.color = new Color(1f, 1f, 1f, 0.2f);
            Widgets.DrawLineHorizontal(0f, cur.y, rect.width);
            GUI.color = Color.white;

            GUI.BeginGroup(rect);

            Rect rectName = rect.AtZero();
            rectName.width -= COL_MARGIN * 4 + BTN_DURATION_WIDTH + BTN_DELETE_WIDTH + BTN_SEASON_WIDTH + LBL_DATE_WIDTH;
            rectName.xMin += COL_MARGIN;

            Rect rectSeeAll = new Rect(rectName.xMax + COL_MARGIN, 2f, BTN_SEASON_WIDTH + COL_MARGIN + LBL_DATE_WIDTH, rect.height - 4f);
            Rect rectDuration = new Rect(rectSeeAll.xMax + COL_MARGIN, 2f, BTN_DURATION_WIDTH, rect.height - 4f);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectName, name);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonText(rectSeeAll, seeAllText))
            {
                seeAllAction();
            }

            if (Widgets.ButtonText(rectDuration, duration.Label()))
            {
                action();
            }

            GUI.EndGroup();
            cur.y += ROW_HEIGHT;
        }

        private void DrawRowWithFixedDate(Rect rect, ref Vector2 cur, string name, Quadrum quadrum, int day, Duration duration, Action action)
        {
            Widgets.DrawHighlightIfMouseover(rect);
            GUI.color = new Color(1f, 1f, 1f, 0.2f);
            Widgets.DrawLineHorizontal(0f, cur.y, rect.width);
            GUI.color = Color.white;

            GUI.BeginGroup(rect);

            Rect rectName = rect.AtZero();
            rectName.width -= COL_MARGIN * 4 + BTN_SEASON_WIDTH + LBL_DATE_WIDTH + BTN_DURATION_WIDTH + BTN_DELETE_WIDTH;
            rectName.xMin += COL_MARGIN;

            Rect rectSeason = new Rect(rectName.xMax + COL_MARGIN, 2f, BTN_SEASON_WIDTH, rect.height - 4f);
            Rect rectDate = new Rect(rectSeason.xMax + COL_MARGIN, 0f, LBL_DATE_WIDTH, rect.height);
            Rect rectDuration = new Rect(rectDate.xMax + COL_MARGIN, 2f, BTN_DURATION_WIDTH, rect.height - 4f);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectName, name);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rectSeason, quadrum.Label());
            Widgets.Label(rectDate, day.ToString());
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonText(rectDuration, duration.Label()))
            {
                action();
            }

            GUI.EndGroup();
            cur.y += ROW_HEIGHT;
        }

        private void DrawRowAdd(Rect rect, Action postAddAction)
        {
            GUI.BeginGroup(rect);

            Rect rectName = rect.AtZero();
            rectName.width -= COL_MARGIN * 4 + BTN_SEASON_WIDTH + LBL_DATE_WIDTH + BTN_DURATION_WIDTH + BTN_ADD_WIDTH;
            rectName.xMin += COL_MARGIN;

            Rect rectSeason = new Rect(rectName.xMax + COL_MARGIN, 0f, BTN_SEASON_WIDTH, rect.height);
            Rect rectDate = new Rect(rectSeason.xMax + COL_MARGIN, 0f, LBL_DATE_WIDTH, rect.height);
            Rect rectDuration = new Rect(rectDate.xMax + COL_MARGIN, 0f, BTN_DURATION_WIDTH, rect.height);
            Rect btnAddRect = new Rect(rectDuration.xMax + COL_MARGIN, 0f, BTN_ADD_WIDTH, rect.height);

            _newEventName = Widgets.TextField(rectName, _newEventName);
            Widgets.TextFieldNumeric(rectDate, ref _newEventDay, ref _newEventDateBuffer, 1, 15);

            if (Widgets.ButtonText(rectSeason, _newEventQuadrum.Label()))
            {
                FloatMenuUtility.MakeMenu(QUADRUMS, quadrum => quadrum.Label(), quadrum => delegate
                {
                    _newEventQuadrum = quadrum;
                });
            }

            if (Widgets.ButtonText(rectDuration, _newEventDuration.Label()))
            {
                FloatMenuUtility.MakeMenu(Enum.GetValues(typeof(Duration)).Cast<Duration>(), duration => duration.Label(), duration => delegate
                {
                    _newEventDuration = duration;
                });
            }

            if (Widgets.ButtonText(btnAddRect, "DM.Tab.AddCustomDay".Translate()))
            {
                if (_newEventName == "")
                    _newEventName = "DM.Tab.CustomDayDefaultName".Translate();
                _store.DMMatteredDays.Add(new MatteredDay(_newEventName, _newEventQuadrum, _newEventDay, _newEventDuration));
                _newEventName = "";
                SoundDefOf.Click.PlayOneShotOnCamera();
                postAddAction();
            }

            GUI.EndGroup();
        }

        private void DrawHeader(Rect rect)
        {
            GUI.BeginGroup(rect);

            Rect rectName = rect.AtZero();
            rectName.width -= COL_MARGIN * 4 + BTN_SEASON_WIDTH + LBL_DATE_WIDTH + BTN_DURATION_WIDTH + BTN_DELETE_WIDTH;
            rectName.xMin += COL_MARGIN;

            Rect rectSeason = new Rect(rectName.xMax + COL_MARGIN, 0f, BTN_SEASON_WIDTH, rect.height);
            Rect rectDate = new Rect(rectSeason.xMax + COL_MARGIN, 0f, LBL_DATE_WIDTH, rect.height);
            Rect rectDuration = new Rect(rectDate.xMax + COL_MARGIN, 0f, BTN_DURATION_WIDTH, rect.height);
            
            Text.Anchor = TextAnchor.LowerCenter;
            Widgets.Label(rectName, "DM.Tab.TableHeader.Name".Translate());
            Widgets.Label(rectSeason, "DM.Tab.TableHeader.Quadrum".Translate());
            Widgets.Label(rectDate, "DM.Tab.TableHeader.Day".Translate());
            Widgets.Label(rectDuration, "DM.Tab.TableHeader.Celebration".Translate());
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.EndGroup();
        }

        private void DrawRow(Rect rect, MatteredDay matteredDay, int index)
        {
            GUI.BeginGroup(rect);

            Rect rectName = rect.AtZero();
            rectName.width -= COL_MARGIN * 4 + BTN_SEASON_WIDTH + LBL_DATE_WIDTH + BTN_DURATION_WIDTH + BTN_DELETE_WIDTH;
            rectName.xMin += COL_MARGIN;

            Rect rectSeason = new Rect(rectName.xMax + COL_MARGIN, 2f, BTN_SEASON_WIDTH, rect.height - 4f);

            Rect rectDateMinus = new Rect(rectSeason.xMax + COL_MARGIN, (rect.height - BTN_DATE_CONTROL_WIDTH) / 2f, BTN_DATE_CONTROL_WIDTH, BTN_DATE_CONTROL_WIDTH);
            Rect rectDate = new Rect(rectDateMinus.xMax, 0f, LBL_DATE_WIDTH - BTN_DATE_CONTROL_WIDTH * 2, rect.height);
            Rect rectDatePlus = new Rect(rectDate.xMax, (rect.height - BTN_DATE_CONTROL_WIDTH) / 2f, BTN_DATE_CONTROL_WIDTH, BTN_DATE_CONTROL_WIDTH);

            Rect rectDuration = new Rect(rectDatePlus.xMax + COL_MARGIN, 2f, BTN_DURATION_WIDTH, rect.height - 4f);
            Rect btnDeleteRect = new Rect(rectDuration.xMax + COL_MARGIN, Mathf.RoundToInt((ROW_HEIGHT - BTN_DELETE_HEIGHT) / 2f), BTN_DELETE_WIDTH, BTN_DELETE_HEIGHT);
            
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectName, matteredDay.Name);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rectDate, matteredDay.DayOfQuadrum.ToString());
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonText(rectSeason, matteredDay.Quadrum.Label()))
            {
                FloatMenuUtility.MakeMenu(QUADRUMS, quadrum => quadrum.Label(), quadrum => delegate
                {
                    matteredDay.Quadrum = quadrum;
                });
            }

            if (Widgets.ButtonText(rectDuration, matteredDay.Duration.Label()))
            {
                FloatMenuUtility.MakeMenu(Enum.GetValues(typeof(Duration)).Cast<Duration>(), duration => duration.Label(), duration => delegate
                {
                    matteredDay.Duration = duration;
                });
            }

            if (Widgets.ButtonImage(btnDeleteRect, Textures.ROW_DELETE))
            {
                _store.DMMatteredDays.RemoveAt(index);
                SoundDefOf.Click.PlayOneShotOnCamera();
            }

            if (Widgets.ButtonImage(rectDateMinus, Textures.DAY_MINUS))
            {
                int tmp = matteredDay.DayOfQuadrum - 1;
                if (tmp < 1) tmp = 1;
                matteredDay.DayOfQuadrum = tmp;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }

            if (Widgets.ButtonImage(rectDatePlus, Textures.DAY_PLUS))
            {
                int tmp = matteredDay.DayOfQuadrum + 1;
                if (tmp > 15) tmp = 15;
                matteredDay.DayOfQuadrum = tmp;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }

            var interactingName = Mouse.IsOver(rectName);
            if (interactingName)
            {
                if (Event.current.control && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    Find.WindowStack.Add(new DialogRename(matteredDay));
                    return;
                }
                TooltipHandler.ClearTooltipsFrom(rectName);
                TooltipHandler.TipRegion(rectName, "DM.Tab.RenameCustomDay".Translate());
            }

            GUI.EndGroup();
        }
    }
}
