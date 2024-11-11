using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using EnhancedDefSelector.Core;

namespace EnhancedDefSelector.Windows
{
    public class DefSelectionWindow : Window
    {
        private Vector2 scrollPosition;
        private string searchText = "";
        private readonly Action<Def> onSelect;
        private readonly List<Def> availableDefs;
        private List<Def> filteredDefs;

        private const float WINDOW_WIDTH = 620f;
        private const float WINDOW_HEIGHT = 500f;
        private const float SEARCH_HEIGHT = 30f;
        private const float ITEM_HEIGHT = 30f;
        private const float SCROLLBAR_WIDTH = 16f;
        private const float PADDING = 5f;

        public override Vector2 InitialSize => new(WINDOW_WIDTH, WINDOW_HEIGHT);

        public DefSelectionWindow(IScenarioDefPatch patch)
        {
            onSelect = patch.OnDefSelected;
            availableDefs = patch.GetAvailableDefs().OrderBy(d => d.label).ToList();
            filteredDefs = availableDefs;

            doCloseX = true;
            doCloseButton = false;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            Text.CurFontStyle.richText = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawSearchBar(inRect, out Rect listingRect);
            DrawDefList(listingRect);
        }

        private void DrawSearchBar(Rect inRect, out Rect listingRect)
        {
            var searchRect = new Rect(0f, 0f, inRect.width, SEARCH_HEIGHT);
            string newText = Widgets.TextField(searchRect, searchText);
            if (searchText != newText)
            {
                searchText = newText;
                UpdateFilteredList();
            }

            listingRect = new Rect(0f, SEARCH_HEIGHT + PADDING, inRect.width, inRect.height - SEARCH_HEIGHT - PADDING);
        }

        private void UpdateFilteredList()
        {
            if (searchText.NullOrEmpty())
            {
                filteredDefs = availableDefs;
                return;
            }

            string searchLower = searchText.ToLower();
            filteredDefs = availableDefs
                .Where(d => d.label.ToLower().Contains(searchLower))
                .ToList();
        }

        private void DrawDefList(Rect listingRect)
        {
            Text.Font = GameFont.Small;

            const float BUTTON_SPACING = 2f;
            float actualItemHeight = ITEM_HEIGHT + BUTTON_SPACING;

            float viewHeight = filteredDefs.Count * actualItemHeight;

            Widgets.BeginScrollView(listingRect, ref scrollPosition,
                new Rect(0f, 0f, listingRect.width - SCROLLBAR_WIDTH, viewHeight));

            int startIndex = (int)(scrollPosition.y / actualItemHeight);
            int visibleItems = Mathf.CeilToInt(listingRect.height / actualItemHeight) + 1;
            int endIndex = Mathf.Min(startIndex + visibleItems, filteredDefs.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                var def = filteredDefs[i];
                float itemY = i * actualItemHeight;

                Rect itemRect = new(
                    PADDING,
                    itemY + BUTTON_SPACING/2,
                    listingRect.width - SCROLLBAR_WIDTH - PADDING * 2,
                    ITEM_HEIGHT
                );

                TextAnchor oldAnchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleCenter;

                bool clicked = Widgets.ButtonText(itemRect, "", true, true, true);

                GUI.color = Color.white;
                Widgets.Label(itemRect, def.LabelCap);

                Text.Anchor = oldAnchor;

                if (clicked)
                {
                    onSelect(def);
                    Close();
                }
            }

            Widgets.EndScrollView();
        }

    }
}