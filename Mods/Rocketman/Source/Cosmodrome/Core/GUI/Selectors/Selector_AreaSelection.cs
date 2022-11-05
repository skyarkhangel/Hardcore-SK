using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public class Selector_AreaSelection : ISelector_GenericSelection<Area>
    {
        public Selector_AreaSelection(IEnumerable<Area> areas, Action<Area> selectionAction, bool integrated = false,
            Action closeAction = null) : base(areas, selectionAction, integrated, closeAction)
        {
        }

        public override float RowHeight => 25f;

        protected override void DoSingleItem(Rect rect, Area item)
        {
            var color = item.Color;
            color.a = 0.5f;
            GUIFont.Anchor = TextAnchor.MiddleLeft;
            Widgets.DrawHighlightIfMouseover(rect);
            Widgets.Label(rect.RightPart(0.8f), item.Label);
            Widgets.DrawBoxSolid(rect.LeftPartPixels(10f), color);
        }

        protected override bool ItemMatchSearchString(Area item)
        {
            return item.Label.ToLower().Contains(searchString.ToLower());
        }
    }
}