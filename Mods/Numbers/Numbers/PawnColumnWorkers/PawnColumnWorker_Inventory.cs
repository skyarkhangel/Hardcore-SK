namespace Numbers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_Inventory : PawnColumnWorker
    {
        private int width;
        private static readonly int baseWidth = 3 * 28; //3 boxes, 28 wide each.

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn.inventory?.innerContainer == null)
                return;

            GUI.BeginGroup(rect);
            float x = 0;
            float gWidth = 28f;
            float gHeight = 28f;

            foreach (Thing thing in pawn.inventory.innerContainer)
            {
                Rect rect2 = new Rect(x, 0, gWidth, gHeight);
                DrawThing(rect2, thing, pawn);
                x += gWidth;
                if (x > width)
                    width = (int)x;
            }
            GUI.EndGroup();
        }

        public override int GetMinWidth(PawnTable table) => Mathf.Max(width, baseWidth, base.GetMinWidth(table));

        private void DrawThing(Rect rect, Thing thing, Pawn selPawn)
        {
            if (Widgets.ButtonInvisible(rect) && Event.current.button == 1)
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>
                {
                    new FloatMenuOption("ThingInfo".Translate(), () => Find.WindowStack.Add(new Dialog_InfoCard(thing)))
                };

                if (selPawn.IsColonistPlayerControlled)
                {
                    Action action = null;

                    if (!thing.def.destroyOnDrop)
                    {
                        action = delegate
                        {
                            selPawn.inventory.innerContainer.TryDrop(thing, selPawn.Position, selPawn.Map, ThingPlaceMode.Near, out Thing unused);
                        };
                    }
                    list.Add(new FloatMenuOption("DropThing".Translate(), action));
                }
                FloatMenu window = new FloatMenu(list, thing.LabelCap);
                Find.WindowStack.Add(window);
            }

            GUI.BeginGroup(rect);
            if (thing.def.DrawMatSingle?.mainTexture != null)
            {
                Widgets.ThingIcon(new Rect(3f, 3f, 27f, 27f), thing);
            }
            GUI.EndGroup();
            TooltipHandler.TipRegion(rect, new TipSignal(thing.LabelCap));
        }

        public override int Compare(Pawn a, Pawn b)
            => (a.inventory?.innerContainer?.Count() ?? -1)
                .CompareTo(b.inventory?.innerContainer?.Count() ?? -1);
    }
}

