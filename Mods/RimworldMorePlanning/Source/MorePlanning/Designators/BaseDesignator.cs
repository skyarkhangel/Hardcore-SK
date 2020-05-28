using System;
using MorePlanning.Plan;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MorePlanning.Designators
{
    public abstract class BaseDesignator : Designator
    {
        protected BaseDesignator(string label, string desc)
        {
            defaultLabel = label;
            defaultDesc = desc;

            soundSucceeded = SoundDefOf.Designate_PlanAdd;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;

            useMouseIcon = true;
        }

        protected virtual void DrawToolbarIcon( Rect rect)
        {
            Material material = (!disabled) ? null : TexUI.GrayscaleGUI;

            Texture2D badTex = icon;
            if (badTex == null)
            {
                badTex = BaseContent.BadTex;
            }

            Rect outerRect = rect;
            Vector2 position = outerRect.position;
            float x = iconOffset.x;
            Vector2 size = outerRect.size;
            float x2 = x * size.x;
            float y = iconOffset.y;
            Vector2 size2 = outerRect.size;
            outerRect.position = position + new Vector2(x2, y * size2.y);

            Widgets.DrawTextureFitted(outerRect, badTex, iconDrawScale * 0.85f, iconProportions, iconTexCoords, iconAngle, material);
        }

        // copy paste from Command.GizmoOnGUI
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            Text.Font = GameFont.Tiny;
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            bool flag = false;
            if (Mouse.IsOver(rect))
            {
                flag = true;
                if (!disabled)
                {
                    GUI.color = GenUI.MouseoverColor;
                }
            }
            
            Material material = (!disabled) ? null : TexUI.GrayscaleGUI;
            GenUI.DrawTextureWithMaterial(rect, BGTex, material);
            MouseoverSounds.DoRegion(rect, SoundDefOf.Mouseover_Command);
            // BEGIN EDIT
            GUI.color = IconDrawColor;
            DrawToolbarIcon(rect);
            // END EDIT
            GUI.color = UnityEngine.Color.white;
            bool flag2 = false;
            KeyCode keyCode = (hotKey != null) ? hotKey.MainKey : KeyCode.None;
            if (keyCode != 0 && !GizmoGridDrawer.drawnHotKeys.Contains(keyCode))
            {
                Rect rect2 = new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, 18f);
                Widgets.Label(rect2, keyCode.ToStringReadable());
                GizmoGridDrawer.drawnHotKeys.Add(keyCode);
                if (hotKey.KeyDownEvent)
                {
                    flag2 = true;
                    Event.current.Use();
                }
            }
            if (Widgets.ButtonInvisible(rect))
            {
                flag2 = true;
            }
            string labelCap = LabelCap;
            if (!labelCap.NullOrEmpty())
            {
                float num = Text.CalcHeight(labelCap, rect.width);
                Rect rect3 = new Rect(rect.x, rect.yMax - num + 12f, rect.width, num);
                GUI.DrawTexture(rect3, TexUI.GrayTextBG);
                GUI.color = UnityEngine.Color.white;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(rect3, labelCap);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = UnityEngine.Color.white;
            }
            GUI.color = UnityEngine.Color.white;
            if (DoTooltip)
            {
                TipSignal tip = Desc;
                if (disabled && !disabledReason.NullOrEmpty())
                {
                    string text = tip.text;
                    tip.text = text + "\n\n" + "DisabledCommand".Translate() + ": " + disabledReason;
                }
                TooltipHandler.TipRegion(rect, tip);
            }
            if (!HighlightTag.NullOrEmpty() && (Find.WindowStack.FloatMenu == null || !Find.WindowStack.FloatMenu.windowRect.Overlaps(rect)))
            {
                UIHighlighter.HighlightOpportunity(rect, HighlightTag);
            }
            Text.Font = GameFont.Small;
            if (flag2)
            {
                if (disabled)
                {
                    if (!disabledReason.NullOrEmpty())
                    {
                        Messages.Message(disabledReason, MessageTypeDefOf.RejectInput, false);
                    }
                    return new GizmoResult(GizmoState.Mouseover, null);
                }
                GizmoResult result;
                if (Event.current.button == 1)
                {
                    result = new GizmoResult(GizmoState.OpenedFloatMenu, Event.current);
                }
                else
                {
                    if (!TutorSystem.AllowAction(TutorTagSelect))
                    {
                        return new GizmoResult(GizmoState.Mouseover, null);
                    }
                    result = new GizmoResult(GizmoState.Interacted, Event.current);
                    TutorSystem.Notify_Event(TutorTagSelect);
                }
                return result;
            }
            if (flag)
            {
                return new GizmoResult(GizmoState.Mouseover, null);
            }
            return new GizmoResult(GizmoState.Clear, null);
        }
    }
}
