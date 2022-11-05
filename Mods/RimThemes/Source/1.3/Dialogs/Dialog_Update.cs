using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Verse;
using RimWorld;
using System.Text.RegularExpressions;

namespace aRandomKiwi.RimThemes
{
    public class Dialog_Update : Window
    {
        protected float bottomAreaHeight;
        protected Vector2 scrollPosition = Vector2.zero;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(620f, 700f);
            }
        }

        public Dialog_Update()
        {
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            //inRect.yMin += 15f;
            //inRect.yMax -= 15f;

            var defaultColumnWidth = (inRect.width - 50);
            Listing_Standard list = new Listing_Standard() { ColumnWidth = defaultColumnWidth };

            //Image logo
            Widgets.ButtonImage(new Rect((inRect.width / 2) - 90, inRect.y, 180, 144), Loader.rtUpdateIconTex, Color.white, Color.white);

            var outRect = new Rect(inRect.x, inRect.y + 150, inRect.width, inRect.height-150);
            var scrollRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height*1.5f);

            outRect.height -= (this.bottomAreaHeight + 50);
            Widgets.BeginScrollView(outRect, ref scrollPosition, scrollRect, true);

            list.Begin(scrollRect);

            list.GapLine();
            Text.Anchor = TextAnchor.MiddleCenter;
            list.Label(Utils.releaseInfo);
            Text.Anchor = TextAnchor.UpperLeft;
            list.GapLine();

            list.Label(Utils.releaseDesc);

            list.End();
            Widgets.EndScrollView();
        }
    }
}