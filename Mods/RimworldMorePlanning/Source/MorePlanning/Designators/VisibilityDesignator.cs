using MorePlanning.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Resources = MorePlanning.Common.Resources;

namespace MorePlanning.Designators
{
    public class VisibilityDesignator : BaseDesignator
    {
        private static bool _planningVisibility = true;

        public static bool PlanningVisibility
        {
            get => _planningVisibility;
            set
            {
                _planningVisibility = value;
                UpdateIconTool();
                MorePlanningMod.Instance.WorldSettings.PlanningVisibility = value;
            }
        }

        public VisibilityDesignator() : base("MorePlanning.PlanVisibility".Translate(), "MorePlanning.PlanVisibilityDesc".Translate())
        {
            soundSucceeded = SoundDefOf.Designate_PlanAdd;
            hotKey = KeyBindingDefOf.Misc12;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            MorePlanningMod.LogError(GetType().Name + " can't designate cells");
            return AcceptanceReport.WasRejected;
        }

        public static void UpdateIconTool()
        {
            var desPlanningVisibility = MenuUtility.GetPlanningDesignator<VisibilityDesignator>();

            desPlanningVisibility.icon = _planningVisibility ? Resources.IconVisible : Resources.IconInvisible;
        }

        public override void ProcessInput(Event ev)
        {
            CurActivateSound?.PlayOneShotOnCamera();
            Find.DesignatorManager.Deselect();
            PlanningVisibility = !_planningVisibility;
        }
    }
}
