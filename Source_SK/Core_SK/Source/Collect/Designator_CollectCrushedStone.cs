using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace SK_collect
{
    public class Designator_CollectCrushedStone : Designator
    {
        public Designator_CollectCrushedStone()
        {
            this.defaultLabel = "DesignatorCollectCrushedStoneLabel".Translate();
            this.defaultDesc = "DesignatorCollectCrushedStoneDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designations/CrushedStone");
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
            if (Find.DesignationManager.DesignationAt(loc, DefDatabase<DesignationDef>.GetNamed("CollectCrushedStone")) != null)
                return false;
            if (Find.TerrainGrid.TerrainAt(loc) != DefDatabase<TerrainDef>.GetNamed("Gravel"))
                return "DesignatorCollectCrushedStoneReportString".Translate();
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
            Find.DesignationManager.AddDesignation(new Designation(c, DefDatabase<DesignationDef>.GetNamed("CollectCrushedStone")));
        }
    }
}