using MorePlanning.Plan;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MorePlanning.Designators
{
    public class PasteDesignator : BaseDesignator
    {
        protected static float MiddleMouseDownTime;

        public static PlanInfoSet CurrentPlanCopy { get; set; }

        public override int DraggableDimensions => 0;

        public override bool DragDrawMeasurements => false;

        public PasteDesignator() :
            base("MorePlanning.PlanPaste".Translate(), "MorePlanning.PlanPasteDesc".Translate(KeyBindingDefOf.Misc1.MainKeyLabel, KeyBindingDefOf.Misc2.MainKeyLabel))
        {
            icon = ContentFinder<Texture2D>.Get("UI/PlanPaste");
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            return true;
        }

        public override void Selected()
        {
            base.Selected();
            this.defaultDesc = "MorePlanning.PlanPasteDesc".Translate(KeyBindingDefOf.Misc1.MainKeyLabel, KeyBindingDefOf.Misc2.MainKeyLabel);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            CurrentPlanCopy?.DesignateFromOrigin(c, Map);
        }

        public override void SelectedUpdate()
        {
            GenDraw.DrawNoBuildEdgeLines();
            if (!ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
            {
                if (CurrentPlanCopy != null)
                {
                    IntVec3 intVec = UI.MouseCell();
                    CurrentPlanCopy.Draw(intVec, Map);
                }
            }
        }

        public override void DoExtraGuiControls(float leftX, float bottomY)
        {
            Rect winRect = new Rect(leftX, bottomY - 90f, 200f, 90f);
            HandleRotationShortcuts();
            Find.WindowStack.ImmediateWindow(73095, winRect, WindowLayer.GameUI, delegate
            {
                RotationDirection rotationDirection = RotationDirection.None;
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Medium;
                Rect rect = new Rect(winRect.width / 2f - 64f - 5f, 15f, 64f, 64f);
                if (Widgets.ButtonImage(rect, TexUI.RotLeftTex))
                {
                    // SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                    rotationDirection = RotationDirection.Counterclockwise;
                    Event.current.Use();
                }
                Widgets.Label(rect, KeyBindingDefOf.Designator_RotateLeft.MainKeyLabel);
                Rect rect2 = new Rect(winRect.width / 2f + 5f, 15f, 64f, 64f);
                if (Widgets.ButtonImage(rect2, TexUI.RotRightTex))
                {
                    // SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    rotationDirection = RotationDirection.Clockwise;
                    Event.current.Use();
                }
                Widgets.Label(rect2, KeyBindingDefOf.Designator_RotateRight.MainKeyLabel);
                if (rotationDirection != RotationDirection.None)
                {
                    CurrentPlanCopy.Rotate(rotationDirection);
                }
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            });
        }

        private void HandleRotationShortcuts()
        {
            RotationDirection rotationDirection = RotationDirection.None;
            if (Event.current.button == 2)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    Event.current.Use();
                    MiddleMouseDownTime = Time.realtimeSinceStartup;
                }
                if (Event.current.type == EventType.MouseUp && Time.realtimeSinceStartup - MiddleMouseDownTime < 0.15f)
                {
                    rotationDirection = RotationDirection.Clockwise;
                }
            }
            if (KeyBindingDefOf.Designator_RotateRight.KeyDownEvent)
            {
                rotationDirection = RotationDirection.Clockwise;
            }
            if (KeyBindingDefOf.Designator_RotateLeft.KeyDownEvent)
            {
                rotationDirection = RotationDirection.Counterclockwise;
            }
            if (rotationDirection == RotationDirection.Clockwise)
            {
                // SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                CurrentPlanCopy.Rotate(RotationDirection.Clockwise);
            }
            if (rotationDirection == RotationDirection.Counterclockwise)
            {
                // SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                CurrentPlanCopy.Rotate(RotationDirection.Counterclockwise);
            }
            if (KeyBindingDefOf.Misc1.KeyDownEvent)
            {
                CurrentPlanCopy.FlipHorizontally();
            }
            if (KeyBindingDefOf.Misc2.KeyDownEvent)
            {
                CurrentPlanCopy.FlipVertically();
            }
        }

        public override void ProcessInput(Event ev)
        {
            if (CurrentPlanCopy == null)
            {
                Messages.Message("MorePlanning.NoCutCopiedPlan".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            base.ProcessInput(ev);
        }

    }
}
