using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AutoEquip
{
    public class MainTabWindow_AssignAutoEquip : MainTabWindow_PawnList
    {
        private const float TopAreaHeight = 45f;
        private const int HostilityResponseColumnWidth = 30;

        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(1010f, 45f + (float)base.PawnsCount * 30f + 65f);
            }
        }

        public override void DoWindowContents(Rect fillRect)
        {
            base.DoWindowContents(fillRect);
            Rect position = new Rect(0f, 0f, fillRect.width, 45f);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect = new Rect(5f, 5f, 160f, 35f);
            if (Widgets.TextButton(rect, "ManageOutfits".Translate(), true, false))
            {
                Find.WindowStack.Add(new Dialog_ManageOutfitsAutoEquip(null));
            }
            Text.Anchor = TextAnchor.LowerCenter;
            Rect rect2 = new Rect(175f, 0f, position.width - 175f, position.height);
            Rect rect3 = new Rect(rect2.x, rect2.y, rect2.width / 2f, rect2.height);
            Widgets.Label(rect3, "CurrentOutfit".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 45f, fillRect.width, fillRect.height - 45f);
            base.DrawRows(outRect);
        }

        protected override void DrawPawnRow(Rect rect, Pawn p)
        {
			Rect rect2 = new Rect(rect.x + 175f, rect.y + 3f, 30f, rect.height);
			HostilityResponseModeUtility.DrawResponseButton(rect2.position, p);
            Rect rect3 = new Rect(rect.x + 175f + 30f, rect.y, rect.width - 175f - 30f, rect.height);
            Rect rect4 = new Rect(rect3.x, rect3.y + 2f, rect3.width * 0.333f, rect3.height - 4f);
            if (Widgets.TextButton(rect4, p.outfits.CurrentOutfit.label, true, false))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (Outfit current in Find.Map.outfitDatabase.AllOutfits)
                {
                    Outfit localOut = current;
                    list.Add(new FloatMenuOption(localOut.label, delegate
                    {
                        p.outfits.CurrentOutfit = localOut;
                    }, MenuOptionPriority.Medium, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list, false));
            }
            Rect rect5 = new Rect(rect4.xMax + 4f, rect.y + 2f, 100f, rect.height - 4f);
            if (Widgets.TextButton(rect5, "OutfitEdit".Translate(), true, false))
            {
                Find.WindowStack.Add(new Dialog_ManageOutfitsAutoEquip(p.outfits.CurrentOutfit));
            }
            Rect rect6 = new Rect(rect5.xMax + 4f, rect.y + 2f, 100f, rect.height - 4f);
            if (p.outfits.forcedHandler.SomethingIsForced)
            {
                if (Widgets.TextButton(rect6, "ClearForcedApparel".Translate(), true, false))
                {
                    p.outfits.forcedHandler.Reset();
                }
                TooltipHandler.TipRegion(rect6, new TipSignal(delegate
                {
                    string text = "ForcedApparel".Translate() + ":\n";
                    foreach (Apparel current2 in p.outfits.forcedHandler.ForcedApparel)
                    {
                        text = text + "\n   " + current2.LabelCap;
                    }
                    return text;
                }, p.GetHashCode() * 612));
            }
        }
    }
}
