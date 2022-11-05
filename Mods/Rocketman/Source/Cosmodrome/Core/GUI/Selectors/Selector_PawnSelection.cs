using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RocketMan
{
    // TODO update to 1.3
    // public class Selector_PawnSelection : ISelector_GenericSelection<Pawn>
    // {
    //    public Selector_PawnSelection(IEnumerable<Pawn> defs, Action<Pawn> selectionAction, bool integrated = false,
    //        Action closeAction = null) : base(defs, selectionAction, integrated, closeAction)
    //    {
    //    }
    //
    //    protected override void DoSingleItem(Rect rect, Pawn item)
    //    {
    //        Widgets.DrawHighlightIfMouseover(rect);
    //        Widgets.DrawTextureFitted(rect.LeftPartPixels(50), PortraitsCache.Get(item, new Vector2(50, 50)), 1);
    //        GUIFont.Anchor = TextAnchor.MiddleLeft;
    //        Widgets.Label(new Rect(rect.position + new Vector2(60, 0), rect.size - new Vector2(60, 0)),
    //            item.Name.ToStringFull);
    //    }
    //
    //    protected override bool ItemMatchSearchString(Pawn item)
    //    {
    //        return item.Name.ToStringFull.ToLower().Contains(searchString.ToLower());
    //    }
    // }
}