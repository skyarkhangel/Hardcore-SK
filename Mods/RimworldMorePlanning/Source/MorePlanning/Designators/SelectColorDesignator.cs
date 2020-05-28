using System.Collections.Generic;
using MorePlanning.Dialogs;
using MorePlanning.Plan;
using MorePlanning.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Resources = MorePlanning.Common.Resources;

namespace MorePlanning.Designators
{
    public class SelectColorDesignator : BaseDesignator
    {
        protected int Color;

        public override string LabelCap { get; } = "";

        public SelectColorDesignator(int color) : 
            base("" + color, "MorePlanning.PlanDesc".Translate())
        {
            Color = color;
        }

        public override void ProcessInput(Event ev)
        {
            // Show change color option
            List<FloatMenuOption> list = new List<FloatMenuOption>
            {
                new FloatMenuOption("MorePlanning.ChangeColor".Translate(),
                    delegate { Find.WindowStack.Add(new ColorSelectorDialog(Color)); })
            };


            Find.WindowStack.Add(new FloatMenu(list));

            // Select color
            MorePlanningMod.Instance.SelectedColor = Color;
            
            if (Find.DesignatorManager.SelectedDesignator == null)
            {
                var designatorPlanPaste = MenuUtility.GetPlanningDesignator<AddDesignator>();
                Find.DesignatorManager.Select(designatorPlanPaste);
            }
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return AcceptanceReport.WasRejected;
        }

        protected override void DrawToolbarIcon(Rect rect)
        {
            Rect outerRect = rect;
            Vector2 position = outerRect.position;
            float x = iconOffset.x;
            Vector2 size = outerRect.size;
            float x2 = x * size.x;
            float y = iconOffset.y;
            Vector2 size2 = outerRect.size;
            outerRect.position = position + new Vector2(x2, y * size2.y);

            Rect positionColor = new Rect(0f, 0f, iconProportions.x, iconProportions.y);
            float num;
            if (positionColor.width / positionColor.height < rect.width / rect.height)
            {
                num = rect.height / positionColor.height;
            }
            else
            {
                num = rect.width / positionColor.width;
            }
            num *= iconDrawScale * 0.85f;
            positionColor.width *= num;
            positionColor.height *= num;
            positionColor.x = rect.x + rect.width / 2f - positionColor.width / 2f;
            positionColor.y = rect.y + rect.height / 2f - positionColor.height / 2f;

            Widgets.DrawBoxSolid(positionColor, PlanColorManager.PlanColor[Color]);

            Widgets.DrawTextureFitted(outerRect,
                MorePlanningMod.Instance.SelectedColor == Color
                    ? Resources.ToolBoxColorSelected
                    : Resources.ToolBoxColor, iconDrawScale * 0.85f, iconProportions, iconTexCoords);
        }
    }

}
