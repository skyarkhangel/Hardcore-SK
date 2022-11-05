namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using System.Linq;
    using Verse;

    public class PawnColumnWorker_AllowedAreaWithMassAssign : PawnColumnWorker_AllowedAreaWide
    {
        public override int GetOptimalWidth(PawnTable table)
        {
            var assignableAreas = Find.CurrentMap.areaManager.AllAreas.Where(area => area.AssignableAsAllowed());
            var bla = Text.CalcSize(assignableAreas.Select(x => x.Label).ToCommaList());
            return (int) Mathf.Max(bla.x + 48, def.width);
        }

        public override int GetMinWidth(PawnTable table)
        {
            return Mathf.Min(def.width, GetOptimalWidth(table));
        }

        protected override string GetHeaderTip(PawnTable table)
        {
            return base.GetHeaderTip(table) + "\nShift + control + click: Set all pawns to zone";
        }

        protected override void HeaderClicked(Rect headerRect, PawnTable table)
        {
            base.HeaderClicked(headerRect, table);
            if (Find.CurrentMap == null)
            {
                return;
            }
            var allAreas = Find.CurrentMap.areaManager.AllAreas;
            var assignableAreas = 1 + allAreas.Count(area => area.AssignableAsAllowed());
            var rectWidth = headerRect.width / assignableAreas;
            Text.WordWrap = false;
            Text.Font = GameFont.Tiny;
            var areaIndexOffset = 1;
            foreach (var area in allAreas)
            {
                if (area.AssignableAsAllowed())
                {
                    var startPosition = areaIndexOffset * rectWidth;
                    var rect = new Rect(headerRect.x + startPosition, headerRect.y, rectWidth, headerRect.height);
                    if (Mouse.IsOver(rect) && Event.current.control && Event.current.shift && Event.current.button == 0)
                    {
                        table.PawnsListForReading.ForEach(pawn => pawn.playerSettings.AreaRestriction = area);
                    }
                    areaIndexOffset++;
                }
            }
            Text.WordWrap = true;
            Text.Font = GameFont.Small;
        }
    }
}