using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Hospitality.MainTab
{
    // Pretty much copied from PawnColumnWorker_AllowedArea
    public abstract class PawnColumnWorker_AreaBase : PawnColumnWorker
    {
        private const int TopAreaHeight = 65;

        private const int ManageAreasButtonHeight = 32;

        protected override GameFont DefaultHeaderFont => GameFont.Tiny;

        public override int GetMinWidth(PawnTable table)
        {
            return Mathf.Max(base.GetMinWidth(table), 200);
        }

        public override int GetOptimalWidth(PawnTable table)
        {
            return Mathf.Clamp(273, GetMinWidth(table), GetMaxWidth(table));
        }

        public override int GetMinHeaderHeight(PawnTable table)
        {
            return Mathf.Max(base.GetMinHeaderHeight(table), TopAreaHeight);
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            // Changed check
            if (!pawn.IsGuest()) return;

            AreaGUI.DoAllowedAreaSelectors(rect, pawn, GetArea, SetArea);
        }

        public override void DoHeader(Rect rect, PawnTable table)
        {
            base.DoHeader(rect, table);
            Rect rect2 = new Rect(rect.x, rect.y + (rect.height - TopAreaHeight), Mathf.Min(rect.width, 360f), ManageAreasButtonHeight);
            // Made abstract
            DrawTopArea(rect2);
        }

        // Added
        protected abstract void DrawTopArea(Rect rect);

        public override int Compare(Pawn a, Pawn b)
        {
            return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
        }

        private int GetValueToCompare(Pawn pawn)
        {
            // Changed check
            if (!pawn.IsGuest())
            {
                return -2147483648;
            }

            // Using own method
            Area areaRestriction = GetArea(pawn);
            return areaRestriction?.ID ?? -2147483647;
        }

        // Added
        protected abstract Area GetArea(Pawn pawn);

        // Added
        protected abstract void SetArea(Pawn pawn, Area area);

        protected override void HeaderClicked(Rect headerRect, PawnTable table)
        {
            base.HeaderClicked(headerRect, table);
            if (Event.current.shift && Find.CurrentMap != null)
            {
                List<Pawn> pawnsListForReading = table.PawnsListForReading;
                for (int i = 0; i < pawnsListForReading.Count; i++)
                {
                    if (pawnsListForReading[i].Faction != Faction.OfPlayer)
                    {
                        return;
                    }

                    if (Event.current.button == 0)
                    {
                        // Using own method
                        SetArea(pawnsListForReading[i], Find.CurrentMap.areaManager.Home);
                    }
                    else if (Event.current.button == 1)
                    {
                        // Using own method
                        SetArea(pawnsListForReading[i], null);
                    }
                }

                if (Event.current.button == 0)
                {
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
                }
                else if (Event.current.button == 1)
                {
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
                }
            }
        }

        protected override string GetHeaderTip(PawnTable table)
        {
            return base.GetHeaderTip(table) + "\n" + "AllowedAreaShiftClickTip".Translate();
        }
    }
}
