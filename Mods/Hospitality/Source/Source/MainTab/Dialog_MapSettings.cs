using Hospitality.Utilities;
using UnityEngine;
using Verse;

namespace Hospitality.MainTab
{
    public class Dialog_MapSettings : Window
    {
        private readonly Map map;

        public Dialog_MapSettings(Map map)
        {
            this.map = map;
            forcePause = true;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(450f, 400f);

        public override void DoWindowContents(Rect inRect)
        {
            if (Multiplayer.IsRunning)
                Multiplayer.WatchBegin();

            var listingStandard = new Listing_Standard {ColumnWidth = inRect.width};
            listingStandard.Begin(inRect);

            var comp = map.GetMapComponent();

            Multiplayer.mapFields?.Watch(comp);

            string labelStay = "AreaToStay".Translate();
            string labelBuy = "AreaToBuy".Translate();
            var rectStayLabel = listingStandard.GetRect(Text.CalcHeight(labelStay, listingStandard.ColumnWidth));
            var rectStay = listingStandard.GetRect(24);
            var rectBuyLabel = listingStandard.GetRect(Text.CalcHeight(labelBuy, listingStandard.ColumnWidth));
            var rectBuy = listingStandard.GetRect(24);

            DialogUtility.LabelWithTooltip(labelStay, "AreaToStayTooltip".Translate(), rectStayLabel);
            GenericUtility.DoAreaRestriction(rectStay, comp.defaultAreaRestriction, area => comp.defaultAreaRestriction = area, AreaUtility.AreaAllowedLabel_Area, map);
            DialogUtility.LabelWithTooltip(labelBuy, "AreaToBuyTooltip".Translate(), rectBuyLabel);
            GenericUtility.DoAreaRestriction(rectBuy, comp.defaultAreaShopping, area => comp.defaultAreaShopping = area, GenericUtility.GetShoppingLabel, map);

            var rectImproveRelationship = listingStandard.GetRect(Text.LineHeight);
            DialogUtility.CheckboxLabeled(listingStandard, "ImproveRelationship".Translate(), ref comp.defaultEntertain, rectImproveRelationship, false, ITab_Pawn_Guest.txtImproveTooltip);
            var rectMakeFriends = listingStandard.GetRect(Text.LineHeight);
            DialogUtility.CheckboxLabeled(listingStandard, "MakeFriends".Translate(), ref comp.defaultMakeFriends, rectMakeFriends, false, ITab_Pawn_Guest.txtMakeFriendsTooltip);

            listingStandard.GapLine(24);

            DialogUtility.CheckboxLabeled(listingStandard, "GuestsWelcome".Translate(), ref comp.guestsAreWelcome, listingStandard.GetRect(Text.LineHeight), false, "GuestsWelcomeTooltip".Translate());
            DialogUtility.CheckboxLabeled(listingStandard, "RefuseGuestsUntilBeds".Translate(), ref comp.refuseGuestsUntilWeHaveBeds, listingStandard.GetRect(Text.LineHeight), false, "RefuseGuestsUntilBedsTooltip".Translate());
            DialogUtility.CheckboxLabeled(listingStandard, "GuestsAskForSafety".Translate(), ref comp.askForSafety, listingStandard.GetRect(Text.LineHeight), false, "GuestsAskForSafetyTooltip".Translate());
            DialogUtility.CheckboxLabeled(listingStandard, "GuestsCanTakeFoodForFree".Translate(), ref comp.guestsCanTakeFoodForFree, listingStandard.GetRect(Text.LineHeight), false, "GuestsCanTakeFoodForFreeTooltip".Translate());
            listingStandard.End();

            if (Multiplayer.IsRunning)
                Multiplayer.WatchEnd();
        }
    }
}
