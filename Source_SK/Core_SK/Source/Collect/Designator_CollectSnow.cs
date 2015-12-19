using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace SK_collect
{
    public class Designator_CollectSnow : Designator
    {
        public Designator_CollectSnow()
        {
            this.defaultDesc = "DesignatorCollectSnowDesc".Translate();
            this.defaultLabel = "DesignatorCollectSnowLabel".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designations/Snow");
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
            if (Find.DesignationManager.DesignationAt(loc, DefDatabase<DesignationDef>.GetNamed("CollectSnow")) != null)
                return false;
            if (Find.TerrainGrid.TerrainAt(loc) != DefDatabase<TerrainDef>.GetNamed("Ice"))
            {
                //Log.Message(Find.TerrainGrid.TerrainAt(loc).defName);
                return "DesignatorCollectSnowReportString".Translate();
            }
            return AcceptanceReport.WasAccepted;
        }

        public override bool DragDrawMeasurements
        {
            get
            {
                return true;
            }
        }

        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Find.DesignationManager.AddDesignation(new Designation(c, DefDatabase<DesignationDef>.GetNamed("CollectSnow")));
        }
    }
}