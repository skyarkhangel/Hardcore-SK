namespace Numbers
{
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    public class PawnColumnWorker_Bandwidth : PawnColumnWorker
    {
        private static readonly Color EmptyBlockColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        private static readonly Color FilledBlockColor = ColorLibrary.Yellow;

        private static readonly Color ExcessBlockColor = ColorLibrary.Red;

        public override int Compare(Pawn a, Pawn b)
        {
            return (a.mechanitor?.TotalBandwidth ?? 0).CompareTo(b.mechanitor?.TotalBandwidth ?? 0);
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            var tracker = pawn.mechanitor;
            if (tracker == null)
            {
                return;
            }
            //shamelessly ripped from MechanitorBandwithGizo.
            int totalBandwidth = tracker.TotalBandwidth;
            int usedBandwidth = tracker.UsedBandwidth;
            string text = usedBandwidth.ToString("F0") + " / " + totalBandwidth.ToString("F0");
            TaggedString taggedString = "Bandwidth".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + text + "\n\n" + "BandwidthGizmoTip".Translate();
            int usedBandwidthFromSubjects = tracker.UsedBandwidthFromSubjects;
            if (usedBandwidthFromSubjects > 0)
            {
                taggedString += (string)("\n\n" + ("BandwidthUsage".Translate() + ": ")) + usedBandwidthFromSubjects;
                IEnumerable<string> entries = from p in tracker.OverseenPawns
                                              where !p.IsGestating()
                                              group p by p.kindDef into p
                                              select (string)(p.Key.LabelCap + " x") + p.Count() + " (+" + p.Sum((Pawn mech) => mech.GetStatValue(StatDefOf.BandwidthCost)) + ")";
                taggedString += "\n\n" + entries.ToLineList(" - ");
            }
            int usedBandwidthFromGestation = tracker.UsedBandwidthFromGestation;
            if (usedBandwidthFromGestation > 0)
            {
                taggedString += (string)("\n\n" + "MechGestationBandwidthUsed".Translate() + ": ") + usedBandwidthFromGestation;
                IEnumerable<string> entries2 = from p in tracker.OverseenPawns
                                               where p.IsGestating()
                                               group p by p.kindDef into p
                                               select (string)(p.Key.LabelCap + " x") + p.Count() + " (+" + p.Sum((Pawn mech) => mech.GetStatValue(StatDefOf.BandwidthCost)) + ")";
                taggedString += "\n\n" + entries2.ToLineList(" - ");
            }
            TooltipHandler.TipRegion(rect, taggedString);

            int num = Mathf.Max(usedBandwidth, totalBandwidth);

            int num2 = 2;
            int num3 = Mathf.FloorToInt(rect.height / (float)num2);
            int num4 = Mathf.FloorToInt(rect.width / (float)num3);
            int num5 = 0;
            while (num2 * num4 < num)
            {
                num2++;
                num3 = Mathf.FloorToInt(rect.height / (float)num2);
                num4 = Mathf.FloorToInt(rect.width / (float)num3);
                num5++;
                if (num5 >= 1000)
                {
                    Debug.LogError("Failed to fit bandwidth cells into rect.");
                    return;
                }
            }
            int num6 = Mathf.FloorToInt(rect.width / (float)num3);
            int num7 = num2;
            float num8 = (rect.width - (float)(num6 * num3)) / 2f;
            int num9 = 0;
            int usedBandwidthFromGestation2 = tracker.UsedBandwidthFromGestation;
            int num10 = ((num7 <= 2) ? 4 : 2);
            for (int i = 0; i < num7; i++)
            {
                for (int j = 0; j < num6; j++)
                {
                    num9++;
                    Rect rect5 = new Rect(rect.x + (float)(j * num3) + num8, rect.y + (float)(i * num3), num3, num3).ContractedBy(2f);
                    if (num9 <= num)
                    {
                        if (num9 <= usedBandwidthFromGestation2)
                        {
                            Widgets.DrawRectFast(rect5, EmptyBlockColor);
                            Widgets.DrawRectFast(rect5.ContractedBy(num10), FilledBlockColor);
                        }
                        else if (num9 <= usedBandwidth)
                        {
                            Widgets.DrawRectFast(rect5, (num9 <= totalBandwidth) ? FilledBlockColor : ExcessBlockColor);
                        }
                        else
                        {
                            Widgets.DrawRectFast(rect5, EmptyBlockColor);
                        }
                    }
                }
            }
        }
    }
}
