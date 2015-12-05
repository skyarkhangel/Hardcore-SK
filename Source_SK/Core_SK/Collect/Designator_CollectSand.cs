using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace SK_collect
{
    public class Designator_CollectSand : Designator
    {
        public Designator_CollectSand()
        {
            this.defaultLabel = "DesignatorCollectSandLabel".Translate();
            this.defaultDesc = "DesignatorCollectSandDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designations/Sand");
            this.soundDragSustain = SoundDefOf.DesignateDragStandard;
            this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.DesignateHarvest;
        }

        public override void SelectedUpdate()
        {
            base.SelectedUpdate();
            GenUI.RenderMouseoverBracket();
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            if (!GenGrid.InBounds(loc))
                return false;
            if (GridsUtility.Fogged(loc))
                return false;
            if (Find.DesignationManager.DesignationAt(loc, DefDatabase<DesignationDef>.GetNamed("CollectSand")) != null)
                return false;
            if (Find.TerrainGrid.TerrainAt(loc) != TerrainDefOf.Sand)
                return "DesignatorCollectSandReportString".Translate();
            return AcceptanceReport.WasAccepted;
        }

        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

        public override bool DragDrawMeasurements
        {
            get
            {
                return true;
            }
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Find.DesignationManager.AddDesignation(new Designation(c, DefDatabase<DesignationDef>.GetNamed("CollectSand")));
        }
    }
}