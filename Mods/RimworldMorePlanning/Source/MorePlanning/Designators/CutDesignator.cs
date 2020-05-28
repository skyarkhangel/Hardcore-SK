using System.Collections.Generic;
using MorePlanning.Utility;
using UnityEngine;
using Verse;

namespace MorePlanning.Designators
{
    public class CutDesignator : CopyDesignator
    {

        public CutDesignator()
        {
            defaultLabel = "MorePlanning.PlanCut".Translate();
            defaultDesc = "MorePlanning.PlanCutDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/PlanCut");
        }

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            base.DesignateMultiCell(cells);

            foreach(var cell in cells)
            {
                MapUtility.RemoveAllPlanDesignationAt(cell, Map);
            }
        }

    }
}
