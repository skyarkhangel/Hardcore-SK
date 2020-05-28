using MorePlanning.Plan;
using MorePlanning.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using Resources = MorePlanning.Common.Resources;

namespace MorePlanning.Designators
{
    public class RemoveDesignator : PlanBaseDesignator
    {

        public RemoveDesignator() : base("DesignatorPlanRemove".Translate(), "DesignatorPlanRemoveDesc".Translate())
        {
            soundSucceeded = SoundDefOf.Designate_PlanRemove;
            hotKey = KeyBindingDefOf.Designator_Deconstruct;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Map))
            {
                return false;
            }

            if (MorePlanningMod.Instance.OverrideColors)
            {
                return MapUtility.HasAnyPlanDesignationAt(c, Map);
            }

            return MapUtility.HasPlanDesignationAt(c, Map, MorePlanningMod.Instance.SelectedColor);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            MapUtility.RemoveAllPlanDesignationAt(c, Map);
        }

        public override void DrawMouseAttachments()
        {
            Vector2 mousePosition = Event.current.mousePosition;
            float num = mousePosition.y + 12f;

            if (MorePlanningMod.Instance.OverrideColors)
            {
                Widgets.DrawTextureFitted(new Rect(mousePosition.x + 12f, num, 32f, 32f), Resources.PlanToolRemoveAll, iconDrawScale);
            }
            else
            {
                Graphics.DrawTexture(new Rect(mousePosition.x + 12f, num, 32f, 32f), Resources.Plan, iconTexCoords, 0, 1, 0, 1, PlanColorManager.PlanColor[MorePlanningMod.Instance.SelectedColor]);
                Widgets.DrawTextureFitted(new Rect(mousePosition.x + 12f, num, 32f, 32f), Resources.RemoveIcon, iconDrawScale);
            }
        }

        protected override void DrawToolbarIcon(Rect rect)
        {
            Rect position = new Rect(0f, 0f, iconProportions.x, iconProportions.y);
            float num;
            if (position.width / position.height < rect.width / rect.height)
            {
                num = rect.height / position.height;
            }
            else
            {
                num = rect.width / position.width;
            }
            num *= 0.85f;
            position.width *= num;
            position.height *= num;
            position.x = rect.x + rect.width / 2f - position.width / 2f;
            position.y = rect.y + rect.height / 2f - position.height / 2f;

            if (Event.current.type == EventType.Repaint)
            {
                Graphics.DrawTexture(position, Resources.Plan, iconTexCoords, 0, 1, 0, 1, PlanColorManager.PlanColor[MorePlanningMod.Instance.SelectedColor]);
                Widgets.DrawTextureFitted(new Rect(rect), Resources.RemoveIcon, iconDrawScale * 0.85f, iconProportions, iconTexCoords);
            }
        }
    }
}
