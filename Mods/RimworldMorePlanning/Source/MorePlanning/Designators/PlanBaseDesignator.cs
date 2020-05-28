using System.Collections.Generic;
using MorePlanning.Plan;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MorePlanning.Designators
{
    public abstract class PlanBaseDesignator : BaseDesignator
    {
        public override int DraggableDimensions => 2;

        public override bool DragDrawMeasurements => true;

        protected PlanBaseDesignator(string label, string desc) : base(label, desc)
        {
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
            GenDraw.DrawNoBuildEdgeLines();
            VisibilityDesignator.PlanningVisibility = true;
        }

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
        }

    }
}
