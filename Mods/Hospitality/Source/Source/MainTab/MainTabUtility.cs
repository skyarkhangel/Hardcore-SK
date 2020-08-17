using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality.MainTab
{
    internal static class MainTabUtility
    {
        public static void DrawLordGroups(Rect rect, PawnTable table, Pawn firstPawn)
        {
            if (table.SortingBy != null) return;
            if (firstPawn == null) return;

            var lords = firstPawn.Map.GetMapComponent().PresentLords;

            int index = -table.PawnsListForReading.IndexOf(firstPawn);
            float yTop = rect.yMin;
            float cellHeight = rect.height;

            foreach (var lord in lords)
            {
                const int verticalMargin = 6;
                const float width = 4;

                var box = rect;
                box.xMin = box.xMax - width;
                box.yMin = yTop + index * cellHeight + verticalMargin;
                box.yMax = box.yMin + lord.ownedPawns.Count * cellHeight - verticalMargin*2;
                Widgets.DrawBoxSolid(box, lord.faction.Color);
                index += lord.ownedPawns.Count;
            }
        }
    }
}
