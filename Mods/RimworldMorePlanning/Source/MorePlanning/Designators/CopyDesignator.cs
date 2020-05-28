using System;
using System.Collections.Generic;
using System.Linq;
using MorePlanning.Plan;
using MorePlanning.Utility;
using RimWorld;
using UnityEngine;
using Verse;

namespace MorePlanning.Designators
{
    public class CopyDesignator : BaseDesignator
    {

        public override int DraggableDimensions => 2;

        public override bool DragDrawMeasurements => true;

        public CopyDesignator() : 
            base("MorePlanning.PlanCopy".Translate(), "MorePlanning.PlanCopyDesc".Translate())
        {
            icon = ContentFinder<Texture2D>.Get("UI/PlanCopy");
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Map))
            {
                return false;
            }
            if (c.InNoBuildEdgeArea(Map))
            {
                return "TooCloseToMapEdge".Translate();
            }
            return MapUtility.HasAnyPlanDesignationAt(c, Map);
        }

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            var planDesignations = cells.Select(cell => MapUtility.GetPlanDesignationAt(cell, Map)).Where(designation => designation != null).ToList();
            cells = planDesignations.Select(plan => plan.target.Cell);

            if (planDesignations.Count == 0)
            {
                Messages.Message("MorePlanning.MissingPlanningDesignations".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            int left = cells.Min(cell => cell.x);
            int top = cells.Max(cell => cell.z);
            int right = cells.Max(cell => cell.x);
            int bottom = cells.Min(cell => cell.z);

            IntVec2 mousePos = new IntVec2((int) Math.Floor(UI.MouseMapPosition().x), (int) Math.Floor(UI.MouseMapPosition().z));

            // Adjust mouse position to nearest planning designation
            if (mousePos.x < left)
            {
                mousePos.x = left;
            }
            else if (mousePos.x > left)
            {
                mousePos.x = right;
            }

            if (mousePos.z > top)
            {
                mousePos.z = top;
            }
            else if (mousePos.z < bottom)
            {
                mousePos.z = bottom;
            }

            int sizeCompX = mousePos.x;
            int sizeCompZ = mousePos.z;

            List<PlanInfo> planDesignationInfo = new List<PlanInfo>();

            // Copy all data from designations
            foreach (var planDesignation in planDesignations)
            {
                var moddedPlanDesignation = planDesignation as PlanDesignation;

                var planInfo = new PlanInfo
                {
                    Color = moddedPlanDesignation != null ? moddedPlanDesignation.Color : 0,
                    Pos = new IntVec3(planDesignation.target.Cell.x - sizeCompX, planDesignation.target.Cell.y, planDesignation.target.Cell.z - sizeCompZ)
                };
                planDesignationInfo.Add(planInfo);
            }

            var planCopy = new PlanInfoSet(planDesignationInfo);

            PasteDesignator.CurrentPlanCopy = planCopy;
            Finalize(true);

            var designatorPlanPaste = MenuUtility.GetPlanningDesignator<PasteDesignator>();
            Find.DesignatorManager.Select(designatorPlanPaste);
        }

    }
}
