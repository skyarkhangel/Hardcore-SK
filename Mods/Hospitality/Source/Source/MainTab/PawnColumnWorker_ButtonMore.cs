using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality.MainTab {
    public class PawnColumnWorker_ButtonMore : PawnColumnWorker
    {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (Widgets.ButtonText(rect, "..."))
            {
                CameraJumper.TryJumpAndSelect(pawn);
                if (Find.Selector.IsSelected(pawn))
                {
                    InspectPaneUtility.OpenTab(typeof (ITab_Pawn_Guest));
                }
            }
        }

        public override int GetMinWidth(PawnTable table)
        {
            return Mathf.Max(base.GetMinWidth(table), 36);
        }

        public override int GetMaxWidth(PawnTable table)
        {
            return Mathf.Min(base.GetMaxWidth(table), this.GetMinWidth(table));
        }
    }
}