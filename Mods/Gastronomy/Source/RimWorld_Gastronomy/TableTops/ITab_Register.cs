using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Gastronomy.TableTops
{
    public class ITab_Register : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(800f, 480f);
        private bool showSettings = true;
        private bool showRadius = false;
        private bool showStats = true;
        private Vector2 menuScrollPosition;

        public ITab_Register()
        {
            size = WinSize;
            labelKey = "TabRegister";
        }

        protected Building_CashRegister Register => (Building_CashRegister) SelThing;

        protected override void FillTab()
        {
            var rectTop = new Rect(0, 16, WinSize.x, 40).ContractedBy(10);
            var rectLeft = new Rect(0f, 40+20+16, WinSize.x/2, WinSize.y-40).ContractedBy(10f);
            var rectRight = new Rect(WinSize.x/2, 40+20+16, WinSize.x/2, WinSize.y-40).ContractedBy(10f);

            DrawTop(rectTop);
            DrawLeft(rectLeft);
            DrawRight(rectRight);
        }

        private void DrawTop(Rect rect)
        {
            Timetable.TimetableUtility.DoHeader(new Rect(rect) {height = 24});
            rect.yMin += 24;
            Timetable.TimetableUtility.DoCell(new Rect(rect) {height = 30}, Register.restaurant.timetableOpen);
        }

        private void DrawRight(Rect rect)
        {
            // Menu
            {
                var menuRect = new Rect(rect);
                menuRect.yMax -= 36;

                Register.restaurant.Menu.GetMenuFilters(out var filter, out var parentFilter);
                ThingFilterUI.DoThingFilterConfigWindow(menuRect, ref menuScrollPosition, filter, parentFilter, 
                    1, null, HiddenSpecialThingFilters(), true);
            }
        }

        private void DrawLeft(Rect rect)
        {
            if (showSettings)
            {
                var smallRect = new Rect(rect) {height = 30*3};
                rect.yMin += smallRect.height + 10;

                DrawSettings(smallRect);
            }

            if (showRadius)
            {
                var smallRect = new Rect(rect) {height = 50};
                rect.yMin += smallRect.height + 10;

                DrawRadius(smallRect);
            }

            if (showStats)
            {
                var smallRect = new Rect(rect) {height = 9 * 24 + 20};
                rect.yMin += smallRect.height + 10;

                DrawStats(smallRect);
            }
        }

        private void DrawSettings(Rect rect)
        {
            var listing = new Listing_Standard();
            listing.Begin(rect);
            {
                listing.CheckboxLabeled("TabRegisterOpened".Translate(), ref Register.restaurant.openForBusiness, "TabRegisterOpenedTooltip".Translate());
                listing.CheckboxLabeled("TabRegisterGuests".Translate(), ref Register.restaurant.allowGuests, "TabRegisterGuestsTooltip".Translate());
                listing.CheckboxLabeled("TabRegisterColonists".Translate(), ref Register.restaurant.allowColonists, "TabRegisterColonistsTooltip".Translate());

                // TODO: Make adjustable (guests will have to react somehow)
                bool guestsPay = Register.restaurant.guestPricePercentage > 0;
                listing.CheckboxLabeled("TabRegisterGuestsHaveToPay".Translate(), ref guestsPay, "TabRegisterGuestHaveToPayTooltip".Translate(0.5f.ToStringPercent()));
                Register.restaurant.guestPricePercentage = guestsPay ? 0.5f : 0;
            }
            listing.End();
        }

        private void DrawRadius(Rect rect)
        {
            var listing = new Listing_Standard();
            listing.Begin(rect);
            {
                string strRadius = "TabRegisterRadius".Translate().Truncate(rect.width * 0.6f);
                string strRadiusValue = Register.radius >= 999f ? "Unlimited".TranslateSimple().Truncate(rect.width * 0.3f) : Register.radius.ToString("F0");
                listing.Label(strRadius + ": " + strRadiusValue);
                float oldValue = Register.radius;
                Register.radius = listing.Slider(Register.radius, 3f, 100f);
                if (Register.radius >= 100f)
                {
                    Register.radius = 999f;
                }

                if (Register.radius != oldValue)
                {
                    Register.ScanDiningSpots();
                }
            }
            listing.End();
        }

        private void DrawStats(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, new Color(1f, 1f, 1f, 0.08f));
            rect = rect.ContractedBy(10);
            var listing = new Listing_Standard();
            listing.Begin(rect);
            {
                var patrons = Register.restaurant.Patrons;
                var orders = Register.restaurant.Orders.AllOrders;
                var debts = Register.restaurant.Debts.AllDebts;
                var stock = Register.restaurant.Stock.AllStock;
                var ordersForServing = Register.restaurant.Orders.AvailableOrdersForServing.ToArray();
                var ordersForCooking = Register.restaurant.Orders.AvailableOrdersForCooking.ToArray();

                listing.LabelDouble("TabRegisterSeats".Translate(), Register.restaurant.Seats.ToString());
                listing.LabelDouble("TabRegisterPatrons".Translate(), patrons.Count.ToString(), patrons.Select(p=>p.LabelShort).ToCommaList());
                DrawOrders(listing, "TabRegisterTotalOrders".Translate(), orders);
                DrawOrders(listing, "TabRegisterNeedsServing".Translate(), ordersForServing);
                DrawOrders(listing, "TabRegisterNeedsCooking".Translate(), ordersForCooking);
                DrawStock(listing, "TabRegisterStocked".Translate(), stock);
                DrawDebts(listing, "TabRegisterDebts".Translate(), debts);

                //listing.LabelDouble("TabRegisterStocked".Translate(), stock.Sum(s=>s.stackCount).ToString(), stock.Select(s=>s.def).Distinct().Select(s=>s.label).ToCommaList());
            }
            listing.End();
        }

        private static void DrawOrders(Listing listing, TaggedString label, [NotNull] IReadOnlyCollection<Order> orders)
        {
            // Label
            var rect = CustomLabelDouble(listing, label, $"{orders.Count}: ", out var countSize);

            var grouped = orders.GroupBy(o => o.consumableDef);

            var rectImage  = rect.RightHalf();
            rectImage.xMin += countSize.x;
            rectImage.width = rectImage.height = countSize.y;

            // Icons for each type of order
            foreach (var group in grouped)
            {
                if (group.Key == null) continue;
                // A list of the patrons for the order
                DrawDefIcon(rectImage, group.Key, $"{group.Key.LabelCap}: {group.Select(o => o.patron.Name.ToStringShort).ToCommaList()}");
                rectImage.x += 2 + rectImage.width;
               
                // Will the next one fit?
                if (rectImage.xMax > rect.xMax) break;
            }
            listing.Gap(listing.verticalSpacing);
        }

        private static void DrawStockExpanded(Listing listing, TaggedString label, [NotNull] IReadOnlyCollection<Thing> stock)
        {
            // Label
            var rect = CustomLabelDouble(listing, label, $"{stock.Count}:", out var countSize);

            var grouped = stock.GroupBy(s => s.def);

            var rectImage  = rect.RightHalf();
            rectImage.xMin += countSize.x;
            rectImage.height = countSize.y;

            // Icons for each type of stock
            foreach (var group in grouped)
            {
                if (group.Key == null) continue;
                // Amount label
                string amountText = $" {group.Count()}x";
                var amountSize = Text.CalcSize(amountText);
                rectImage.width = amountSize.x;

                // Will it fit?
                if (rectImage.xMax + rectImage.height > rect.xMax) break;

                // Draw label
                Widgets.Label(rectImage, amountText);
                rectImage.x += rectImage.width;
                // Icon
                rectImage.width = rectImage.height;
                DrawDefIcon(rectImage, group.Key, group.Key.LabelCap);
                rectImage.x += rectImage.width;

                // Will the next one fit?
                if (rectImage.xMax > rect.xMax) break;
            }
            listing.Gap(listing.verticalSpacing);
        }

        private static void DrawStock(Listing listing, TaggedString label, [NotNull] IReadOnlyDictionary<ThingDef, RestaurantStock.Stock> stock)
        {
            // Label
            var rect = CustomLabelDouble(listing, label, $"{stock.Values.Sum(pair => pair.items.Sum(item=>item.stackCount))}", out var countSize);

            var rectIcon  = rect.RightHalf();
            //rectIcon.xMin += countSize.x;
            var iconSize = rectIcon.width = rectIcon.height = countSize.y;
            
            var iconCols = Mathf.FloorToInt(rect.width / iconSize);
            var iconRows = Mathf.CeilToInt((float) stock.Count / iconCols);
            var height = iconRows * iconSize;

            var rectIcons = listing.GetRect(height);
            rectIcon.x = rectIcons.x;
            rectIcon.y = rectIcons.y;

            // Icons for each type of stock
            int col = 0;
            foreach (var group in stock.Values)
            {
                if (group.def == null) continue;
                if (group.items.Count == 0) continue;

                // Icon
                DrawDefIcon(rectIcon, group.def, $"{group.items.Sum(item => item.stackCount)}x {group.def.LabelCap}");
                rectIcon.x += iconSize;

                col++;
                if (col == iconCols)
                {
                    col = 0;
                    rectIcon.x = rectIcons.x;
                }
            }
            listing.Gap(listing.verticalSpacing);
        }  
        
        private static void DrawDebts(Listing listing, TaggedString label, [NotNull] ReadOnlyCollection<Debt> debts)
        {
            // Label
            var rect = CustomLabelDouble(listing, label, $"{debts.Sum(debt => debt.amount).ToStringMoney()}", out var countSize);

            var rectIcon  = rect.RightHalf();
            //rectIcon.xMin += countSize.x;
            var iconSize = rectIcon.width = rectIcon.height = countSize.y;
            
            var iconCols = Mathf.FloorToInt(rect.width / iconSize);
            var iconRows = Mathf.CeilToInt((float) debts.Count / iconCols);
            var height = iconRows * iconSize;

            var rectIcons = listing.GetRect(height);
            rectIcon.x = rectIcons.x;
            rectIcon.y = rectIcons.y;

            // Icons for each type of stock
            int col = 0;
            foreach (var debt in debts)
            {
                if (debt.patron == null) continue;
                if (debt.amount == 0) continue;

                // Icon
                DrawDefIcon(rectIcon, ThingDefOf.Silver, $"{debt.patron.Name.ToStringFull}: {debt.amount.ToStringMoney()}");
                rectIcon.x += iconSize;

                col++;
                if (col == iconCols)
                {
                    col = 0;
                    rectIcon.x = rectIcons.x;
                }
            }
            listing.Gap(listing.verticalSpacing);
        }

        private static Rect CustomLabelDouble(Listing listing, TaggedString labelLeft, TaggedString stringRight, out Vector2 sizeRight)
        {
            sizeRight = Text.CalcSize(stringRight);
            Rect rect = listing.GetRect(Mathf.Max(Text.CalcHeight(labelLeft, listing.ColumnWidth / 2f), sizeRight.y));
            Widgets.Label(rect.LeftHalf(), labelLeft);
            Widgets.Label(rect.RightHalf(), stringRight);
            return rect;
        }

        private static void DrawDefIcon(Rect rect, ThingDef def, string tooltip = null)
        {
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rect, tooltip);
                Widgets.DrawHighlightIfMouseover(rect);
            }

            GUI.DrawTexture(rect, def.uiIcon);
        }

        public override void TabUpdate()
        {
            Register.DrawGizmos();
        }

        private static IEnumerable<SpecialThingFilterDef> HiddenSpecialThingFilters()
        {
            yield return SpecialThingFilterDefOf.AllowFresh;
        }
    }
}
