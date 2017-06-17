using HugsLib.Settings;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace QOLTweaksPack.hugsLibSettings
{
    class SettingUIs
    {
        private const float ContentPadding = 5f;
        private const float MinGizmoSize = 75f;
        private const float IconSize = 32f;
        private const float IconGap = 1f;
        private const float TextMargin = 20f;
        private const float BottomMargin = 2f;

        private static readonly Color iconBaseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color iconMouseOverColor = new Color(0.6f, 0.6f, 0.4f, 1f);

        private static readonly Color SelectedOptionColor = new Color(0.5f, 1f, 0.5f, 1f);
        private static readonly Color constGrey = new Color(0.8f, 0.8f, 0.8f, 1f);

        private static List<ThingDef> meals;
        internal static List<ThingDef> Meals { get
            {
                if (meals != null)
                    return meals;

                meals = new List<ThingDef>();                
                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (def.IsWithinCategory(ThingCategoryDefOf.FoodMeals))
                        meals.Add(def);
                }

                return meals;
            }
        }

        public static bool HugsDrawerRebuild_Checkbox(SettingHandle<bool> handle, Rect controlRect, Color background)
        {
            drawBackground(controlRect, background);
            const float defaultCheckboxHeight = 24f;
            var checkOn = handle.Value;
            Widgets.Checkbox(controlRect.x, controlRect.y + (controlRect.height - defaultCheckboxHeight) / 2, ref checkOn);
            if (checkOn != handle.Value)
            {
                handle.Value = checkOn;
                return true;
            }
            return false;
        }

        internal enum ExpansionMode { None, Vertical, Horizontal };

        internal static bool CustomDrawer_Enumlist(SettingHandle handle, Rect controlRect, string[] enumNames, float[] forcedWidths, bool alsoMouseovers, ExpansionMode expansionMode, Color background)
        {
            drawBackground(controlRect, background);
            if (enumNames == null) return false;
            if (enumNames.Length != forcedWidths.Length) return false;

            if (expansionMode == ExpansionMode.Horizontal)
                throw new NotImplementedException("Horizontal scrolling not yet implemented.");

            float buttonWidth = controlRect.width;
            int forcedButtons = 0;
            for (int i = 0; i < forcedWidths.Length; i++)
            {
                if (forcedWidths[i] != 0f)
                {
                    forcedButtons++;
                    buttonWidth -= forcedWidths[i];
                }
            }
            if (forcedButtons != enumNames.Length)
                buttonWidth /= (float)(enumNames.Length - forcedButtons);

            float position = controlRect.position.x;

            bool changed = false;

            for (int i = 0; i < enumNames.Length; i++)
            {
                float width = (forcedWidths[i] == 0f) ? buttonWidth : forcedWidths[i];

                Rect buttonRect = new Rect(controlRect);
                buttonRect.position = new Vector2(position, buttonRect.position.y);
                buttonRect.width = width;
                //buttonRect = buttonRect.ContractedBy(2f);
                bool interacted = false;

                bool selected = handle.StringValue.Equals(enumNames[i]);

                string label = (handle.EnumStringPrefix + enumNames[i]).Translate();
                string mouseOver = (handle.EnumStringPrefix + enumNames[i]+"_mouseover").Translate();

                if (expansionMode == ExpansionMode.Vertical)
                {
                    float height = Text.CalcHeight(label, width);
                    if (handle.CustomDrawerHeight < height) handle.CustomDrawerHeight = height;
                }

                Color activeColor = GUI.color;
                if (selected)
                    GUI.color = SelectedOptionColor;

                if(alsoMouseovers)
                    TooltipHandler.TipRegion(buttonRect, mouseOver);

                interacted = Widgets.ButtonText(buttonRect, label);

                if (selected)
                    GUI.color = activeColor;

                if (interacted)
                {
                    handle.StringValue = enumNames[i];
                    changed = true;
                }

                position += width;
            }
            return changed;
        }

        internal static bool CustomDrawer_MatchingMeals_active(Rect wholeRect, SettingHandle<MealSetHandler> setting, Color background, string yesText = "Selected meals", string noText = "Other meals")
        {
            drawBackground(wholeRect, background);

            if (setting.Value == null)
                setting.Value = new MealSetHandler();

            GUI.color = Color.white;

            Rect leftRect = new Rect(wholeRect);
            leftRect.width = leftRect.width / 2;
            leftRect.height = wholeRect.height - TextMargin + BottomMargin;
            leftRect.position = new Vector2(leftRect.position.x, leftRect.position.y);
            Rect rightRect = new Rect(wholeRect);
            rightRect.width = rightRect.width / 2;
            leftRect.height = wholeRect.height - TextMargin + BottomMargin;
            rightRect.position = new Vector2(rightRect.position.x + leftRect.width, rightRect.position.y);

            DrawLabel(yesText, leftRect, TextMargin);
            DrawLabel(noText, rightRect, TextMargin);

            leftRect.position = new Vector2(leftRect.position.x, leftRect.position.y + TextMargin);
            rightRect.position = new Vector2(rightRect.position.x, rightRect.position.y + TextMargin);

            int iconsPerRow = (int)(leftRect.width / (IconGap + IconSize));

            HashSet<string> selection = setting.Value.InnerList;

            List<ThingDef> allMeals = Meals.ListFullCopy();
            List<ThingDef> selectedMeals = new List<ThingDef>();
            List<ThingDef> unselectedMeals = new List<ThingDef>();

            foreach(ThingDef thing in allMeals)
            {
                if (selection.Contains(thing.defName))
                    selectedMeals.Add(thing);
                else
                    unselectedMeals.Add(thing);
            }

            bool change = false;

            int biggerRows = Math.Max((selectedMeals.Count - 1) / iconsPerRow, (unselectedMeals.Count - 1) / iconsPerRow) + 1;
            setting.CustomDrawerHeight = (biggerRows * IconSize) + ((biggerRows) * IconGap) + TextMargin;

            for (int i = 0; i < selectedMeals.Count; i++)
            {
                int collum = (i % iconsPerRow);
                int row = (i / iconsPerRow);
                bool interacted = DrawIconForItem(selectedMeals[i], leftRect, new Vector2(IconSize * collum + collum * IconGap, IconSize * row + row * IconGap), i);
                if (interacted)
                {
                    change = true;
                    selection.Remove(selectedMeals[i].defName);
                    i--; //to prevent skipping
                }
            }

            for (int i = 0; i < unselectedMeals.Count; i++)
            {
                int collum = (i % iconsPerRow);
                int row = (i / iconsPerRow);
                bool interacted = DrawIconForItem(unselectedMeals[i], rightRect, new Vector2(IconSize * collum + collum * IconGap, IconSize * row + row * IconGap), i);
                if (interacted)
                {
                    change = true;
                    selection.Add(unselectedMeals[i].defName);
                }
            }
            if (change)
            {
                setting.Value.InnerList = selection;
                //Log.Message("selected list change");
            }
            return change;
        }

        private static void DrawLabel(string labelText, Rect textRect, float offset)
        {
            var labelHeight = Text.CalcHeight(labelText, textRect.width);
            labelHeight -= 2f;
            var labelRect = new Rect(textRect.x, textRect.yMin - labelHeight + offset, textRect.width, labelHeight);
            GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(labelRect, labelText);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        private static bool DrawIconForItem(ThingDef item, Rect contentRect, Vector2 iconOffset, int buttonID)
        {
            var iconTex = item.uiIcon;
            Graphic g = item.graphicData.Graphic;
            Color color = getColor(item);
            Color colorTwo = getColor(item);
            Graphic g2 = item.graphicData.Graphic.GetColoredVersion(g.Shader, color, colorTwo);

            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);

            string label = item.label;

            TooltipHandler.TipRegion(iconRect, label);
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.MouseoverCommand);
            if (Mouse.IsOver(iconRect))
            {
                GUI.color = iconMouseOverColor;
                GUI.DrawTexture(iconRect, TextureResources.drawPocket);
                //Graphics.DrawTexture(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, iconMouseOverColor);
            }
            else
            {
                GUI.color = iconBaseColor;
                GUI.DrawTexture(iconRect, TextureResources.drawPocket);
                //Graphics.DrawTexture(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, iconBaseColor);
            }

            Texture resolvedIcon;
            if (!item.uiIconPath.NullOrEmpty())
            {
                resolvedIcon = item.uiIcon;
            }
            else
            {
                resolvedIcon = g2.MatSingle.mainTexture;
            }
            GUI.color = color;
            GUI.DrawTexture(iconRect, resolvedIcon);
            GUI.color = Color.white;

            if (Widgets.ButtonInvisible(iconRect, true))
            {
                Event.current.button = buttonID;
                return true;
            }
            else
                return false;
        }

        private static Color getColor(ThingDef item)
        {
            if (item.graphicData != null)
            {
                return item.graphicData.color;
            }
            return Color.white;
        }

        private static void drawBackground(Rect rect, Color background)
        {
            Color save = GUI.color;
            GUI.color = background;
            GUI.DrawTexture(rect, TexUI.FastFillTex);
            GUI.color = save;
        }
    }

}
