using PeteTimesSix.SimpleSidearms;
using PeteTimesSix.SimpleSidearms.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;
using static PeteTimesSix.SimpleSidearms.SimpleSidearms;
using HarmonyLib;

namespace SimpleSidearms.rimworld
{
    public class Gizmo_SidearmsList : Command
    {
        public const float ContentPadding = 2f;
        public const float MinGizmoSize = 75f;
        public const float IconSize = 32f;
        public const float IconGap = 1f;
        public const float SelectorPanelWidth = 32f + ContentPadding * 2;
        public const float PreferenceIconHeight = 21f;
        public const float PreferenceIconWidth = 32f;

        public const float FirstTimeSettingsWarningWidth = 16f;


        public static readonly Color iconBaseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public static readonly Color iconMouseOverColor = new Color(0.6f, 0.6f, 0.4f, 1f);

        public static readonly Color preferenceBase = new Color(0.5f, 0.5f, 0.5f, 1f);
        public static readonly Color preferenceSet = new Color(0.5f, 1.0f, 0.5f, 1f);
        public static readonly Color preferenceOfSkill = new Color(1.0f, 0.75f, 0.5f, 1f);
        public static readonly Color preferenceHighlight = new Color(0.7f, 0.7f, 0.4f, 1f);
        public static readonly Color preferenceHighlightSet = new Color(0.7f, 1.0f, 0.4f, 1f);

        //public Texture2D[] iconTextures;
        public Action hotkeyAction;

        public static Pawn parent;
        public IEnumerable<ThingWithComps> carriedWeapons;
        public static Dictionary<ThingWithComps,bool> carriedWeaponsTemp = new Dictionary<ThingWithComps, bool>();
        public IEnumerable<ThingWithComps> carriedRangedWeapons { get { return carriedWeapons.Where(w => w.def.IsRangedWeapon); } }
        public IEnumerable<ThingWithComps> carriedMeleeWeapons { get { return carriedWeapons.Where(w => w.def.IsMeleeWeapon); } }

        public IEnumerable<ThingDefStuffDefPair> weaponMemories;
        public static Dictionary<ThingDefStuffDefPair, Dictionary<bool, int>> weaponMemoriesTemp = new Dictionary<ThingDefStuffDefPair, Dictionary<bool, int>>();
        public IEnumerable<ThingDefStuffDefPair> rangedWeaponMemories { get { return weaponMemories.Where(w => w.thing.IsRangedWeapon); } }
        public IEnumerable<ThingDefStuffDefPair> meleeWeaponMemories { get { return weaponMemories.Where(w => w.thing.IsMeleeWeapon); } }

        public enum SidearmsListInteraction
        {
            None,
            SelectorRanged,
            SelectorSkill,
            SelectorMelee,
            Weapon,
            WeaponMemory,
            Unarmed
        }
        public SidearmsListInteraction interactedWith = SidearmsListInteraction.None;
        public ThingWithComps interactionWeapon;
        public ThingDefStuffDefPair? interactionWeaponType;
        public bool interactionWeaponIsDuplicate;
        public CompSidearmMemory pawnMemory;
        public static Rect gizmoRect;
        public static Rect contentRect;
        public bool update = false;
        public bool mouseOver = false;

        public override float GetWidth(float maxWidth)
        {
            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(parent);
            if (pawnMemory == null)
                return 75;
            int biggerCount = Math.Max(
                carriedRangedWeapons.Count() + countMissingRangedWeapons(pawnMemory, parent),
                carriedMeleeWeapons.Count() + countMissingMeleeWeapons(pawnMemory, parent) + 1
                );
            float width = SelectorPanelWidth + ContentPadding + (IconSize * biggerCount) + IconGap * (biggerCount - 1) + ContentPadding;
            if (!Settings.SettingsEverOpened)
                width += (FirstTimeSettingsWarningWidth + 2);
            return Math.Min(Math.Max(width, MinGizmoSize), maxWidth);
        }

        public Gizmo_SidearmsList(Pawn pawn, IEnumerable<ThingWithComps> carriedWeapons, IEnumerable<ThingDefStuffDefPair> weaponMemories)
        {
            if (!carriedWeaponsTemp.NullOrEmpty())
            {
                if (parent != null && parent.thingIDNumber != pawn.thingIDNumber || carriedWeaponsTemp.Keys.Count != carriedWeapons.Count())
                {
                   update = true;
                }
                else
                {
                    foreach (var item in carriedWeapons)
                    {
                        if (!carriedWeaponsTemp.ContainsKey(item))
                            update = true;
                    }
                }
            }

            if (SelectionDrawer.SelectTimes.Count < 2)
            {
                parent = pawn;
            }
            else
            {
                foreach (var item in SelectionDrawer.SelectTimes)
                {
                    Pawn firstPawn = SelectionDrawer.SelectTimes.Where(p => p.Key is Pawn).First().Key as Pawn;
                    if (pawn != null)
                    parent = firstPawn;
                }

            }

            this.carriedWeapons = carriedWeapons;
            this.weaponMemories = weaponMemories;
            tutorTag = "SidearmsList";
            this.defaultLabel = "DrawSidearm_gizmoTitle".Translate();
            this.defaultDesc = "DrawSidearm_gizmoTooltip".Translate();
            pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(parent);

        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {

            if (Mouse.IsOver(gizmoRect) || Mouse.IsOver(contentRect) || carriedWeaponsTemp.NullOrEmpty() || update)
            {
                //if (!Mouse.IsOver(gizmoRect) && !Mouse.IsOver(contentRect))
                //{
                //    ticksPast += Find.TickManager.TicksGame;
                //}
                if (Mouse.IsOver(gizmoRect) || Mouse.IsOver(contentRect))
                {
                    mouseOver = true;
                }
                else
                {
                    mouseOver = false;
                }
                update = false;
                //Log.Message("GizmoOnGUI_old has been run.");
                return GizmoOnGUI_old(topLeft, maxWidth);
            }

            // Minimized
            //ticksPast = 0f;
            interactionWeaponIsDuplicate = false;
            interactionWeapon = null;
            interactionWeaponType = null;

            gizmoRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), MinGizmoSize);
            contentRect = gizmoRect.ContractedBy(ContentPadding);
            Widgets.DrawWindowBackground(gizmoRect);

            if (pawnMemory == null)
            {
                return new GizmoResult(GizmoState.Clear);
            }

            int total = 0;

            if (!Settings.GizmoPerformanceMode) 
            { 
                {
                    var rangedWeapons = carriedWeaponsTemp.Where(w => w.Key.def.IsRangedWeapon).ToList();
                    rangedWeapons.SortStable((a, b) => { return (int)((b.Key.MarketValue - a.Key.MarketValue) * 1000); });

                    int i = 0;
                    foreach (var weapon in rangedWeapons)
                    {
                        //Log.Message("Adding: " + weapon.Key.def.defName + ". isDuplicate: " + weapon.Value);
                        var iconOffset = new Vector2((IconSize * i) + IconGap * (i - 1) + SelectorPanelWidth, 0);
                        DrawIconForWeapon(parent, pawnMemory, weapon.Key, weapon.Value, contentRect, iconOffset);

                        i++;
                    }
                    total += i;
                }

                if (pawnMemory != null)
                {
                    var rangedWeaponMemoriesSorted = weaponMemoriesTemp.Where(w => w.Key.thing.IsRangedWeapon).ToList();
                    rangedWeaponMemoriesSorted.SortStable((a, b) => { return (int)((b.Key.Price - a.Key.Price) * 1000); });
                    var grouped = rangedWeaponMemoriesSorted.GroupBy(m => m);

                    int j = 0;
                    foreach (var group in grouped)
                    {
                        var weaponMemory = group.Key;
                        var stackCount = group.Count();

                        //Log.Message("Adding Memory: " + weaponMemory.Key.thing.defName + ". isDuplicate: " + weaponMemory.Value.First().Key + ". missingCount: " + weaponMemory.Value.First().Value);
                        var iconOffset = new Vector2((IconSize * (total + j)) + IconGap * ((total + j) - 1) + SelectorPanelWidth, 0);
                        DrawIconForWeaponMemory(parent, pawnMemory, weaponMemory.Key, weaponMemory.Value.First().Value, weaponMemory.Value.First().Key, contentRect, iconOffset);
                        j++;
                    }
                }

                total = 0;
                {
                    var rangedWeapons = carriedWeaponsTemp.Where(w => w.Key.def.IsMeleeWeapon).ToList();
                    rangedWeapons.SortStable((a, b) => { return (int)((b.Key.MarketValue - a.Key.MarketValue) * 1000); });

                    int i = 0;
                    foreach (var weapon in rangedWeapons)
                    {
                        //Log.Message("Adding: " + weapon.Key.def.defName + ". isDuplicate: " + weapon.Value);
                        var iconOffset = new Vector2((IconSize * i) + IconGap * (i - 1) + SelectorPanelWidth, IconSize + IconGap);
                        DrawIconForWeapon(parent, pawnMemory, weapon.Key, weapon.Value, contentRect, iconOffset);

                        i++;
                    }
                    total += i;
                }

                if (pawnMemory != null)
                {
                    var rangedWeaponMemoriesSorted = weaponMemoriesTemp.Where(w => w.Key.thing.IsMeleeWeapon).ToList();
                    rangedWeaponMemoriesSorted.SortStable((a, b) => { return (int)((b.Key.Price - a.Key.Price) * 1000); });
                    var grouped = rangedWeaponMemoriesSorted.GroupBy(m => m);

                    int j = 0;
                    foreach (var group in grouped)
                    {
                        var weaponMemory = group.Key;
                        var stackCount = group.Count();

                        //Log.Message("Adding Memory: " + weaponMemory.Key.thing.defName + ". isDuplicate: " + weaponMemory.Value.First().Key + ". missingCount: " + weaponMemory.Value.First().Value);
                        var iconOffset = new Vector2((IconSize * (total + j)) + IconGap * ((total + j) - 1) + SelectorPanelWidth, IconSize + IconGap);
                        DrawIconForWeaponMemory(parent, pawnMemory, weaponMemory.Key, weaponMemory.Value.First().Value, weaponMemory.Value.First().Key, contentRect, iconOffset);
                        j++;
                    }
                }
            }
            var unarmedIconOffset = new Vector2((IconSize * total) + IconGap * (total - 1) + SelectorPanelWidth, IconSize + IconGap);
            DrawIconForUnarmed(parent, pawnMemory, contentRect, unarmedIconOffset);

            Rect selectorPanel = new Rect(gizmoRect.x + ContentPadding, gizmoRect.y + ContentPadding, SelectorPanelWidth - ContentPadding * 2, MinGizmoSize - ContentPadding * 2);
            DrawPreferenceSelector(parent, pawnMemory, selectorPanel);

            UIHighlighter.HighlightOpportunity(gizmoRect, "SidearmList");

            if (!Settings.SettingsEverOpened)
            {
                Rect position = new Rect((gizmoRect.x + gizmoRect.width - (FirstTimeSettingsWarningWidth + 2)), gizmoRect.y + 4, FirstTimeSettingsWarningWidth, FirstTimeSettingsWarningWidth);
                float brightness = Pulser.PulseBrightness(1f, 0.5f);
                GUI.color = new Color(brightness, brightness, 0f);
                GUI.DrawTexture(position, TextureResources.FirstTimeSettingsWarningIcon);
                if (Widgets.ButtonInvisible(position))
                {
                    var dialog = new Dialog_ModSettings(ModSingleton);
                    //Find.WindowStack.Add(new Dialog_ModSettings(mod));
                    Find.WindowStack.Add(dialog);
                }
                TooltipHandler.TipRegion(position, "FirstTimeSettingsWarning".Translate());
            }

            GUI.color = Color.white;

            if (parent.IsColonistPlayerControlled)
                DrawGizmoLabel(defaultLabel, gizmoRect);
            else
                DrawGizmoLabel(defaultLabel + " (godmode)", gizmoRect);
            return interactedWith != SidearmsListInteraction.None ? new GizmoResult(GizmoState.Interacted, Event.current) : new GizmoResult(GizmoState.Clear);
        }

        private GizmoResult GizmoOnGUI_old(Vector2 topLeft, float maxWidth)
        {
            interactionWeaponIsDuplicate = false;
            interactionWeapon = null;
            interactionWeaponType = null;
            //Log.Message("GizmoOnGUI_old Run");
            gizmoRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), MinGizmoSize);

            if (Mouse.IsOver(gizmoRect))
            {
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsBasic, OpportunityType.Important);
            }

            contentRect = gizmoRect.ContractedBy(ContentPadding);
            Widgets.DrawWindowBackground(gizmoRect);

            if (pawnMemory == null)
            {
                return new GizmoResult(GizmoState.Clear);
            }

            carriedWeaponsTemp.Clear();
            weaponMemoriesTemp.Clear();
            int total = 0;
            Dictionary<ThingDefStuffDefPair, int> dupeCounters = new Dictionary<ThingDefStuffDefPair, int>();
            {
                var rangedWeapons = carriedRangedWeapons.ToList();
                rangedWeapons.SortStable((a, b) => { return (int)((b.MarketValue - a.MarketValue) * 1000); });

                int i = 0;
                foreach (var weapon in rangedWeapons)
                {
                    ThingDefStuffDefPair weaponMemory = weapon.toThingDefStuffDefPair();
                    if (!dupeCounters.ContainsKey(weaponMemory))
                        dupeCounters[weaponMemory] = 0;

                    bool isDupe = dupeCounters[weaponMemory] > 0;
                    carriedWeaponsTemp.Add(weapon,isDupe);
                    if (!Settings.GizmoPerformanceMode || mouseOver)
                    {
                        var iconOffset = new Vector2((IconSize * i) + IconGap * (i - 1) + SelectorPanelWidth, 0);
                        DrawIconForWeapon(parent, pawnMemory, weapon, isDupe, contentRect, iconOffset);
                    }

                    i++;
                    dupeCounters[weaponMemory] += weapon.stackCount;
                }
                total += i;
            }

            dupeCounters.Clear();

            if (pawnMemory != null)
            {
                var rangedWeaponMemoriesSorted = rangedWeaponMemories.ToList();
                rangedWeaponMemoriesSorted.SortStable((a, b) => { return (int)((b.Price - a.Price) * 1000); });
                var grouped = rangedWeaponMemoriesSorted.GroupBy(m => m);

                int j = 0;
                foreach (var group in grouped)
                {
                    var weaponMemory = group.Key;
                    var stackCount = group.Count();

                    if (!dupeCounters.ContainsKey(weaponMemory))
                        dupeCounters[weaponMemory] = 0;

                    int missingCount = parent.missingCountWeaponsOfType(weaponMemory, stackCount, dupeCounters[weaponMemory]);
                    if(missingCount > 0)
                    {
                        bool isDupe = dupeCounters[weaponMemory] > 0;
                        Dictionary<bool, int> info = new Dictionary<bool, int>();
                        info.Add(isDupe, missingCount);
                        weaponMemoriesTemp.Add(weaponMemory, info);
                        if (!Settings.GizmoPerformanceMode || mouseOver)
                        {
                            var iconOffset = new Vector2((IconSize * (total + j)) + IconGap * ((total + j) - 1) + SelectorPanelWidth, 0);
                            DrawIconForWeaponMemory(parent, pawnMemory, weaponMemory, missingCount, isDupe, contentRect, iconOffset);
                        }
                        j++;
                    }
                    dupeCounters[weaponMemory] += stackCount;
                }
            }

            dupeCounters.Clear();
            total = 0;

            {
                var meleeWeapons = carriedMeleeWeapons.ToList();
                meleeWeapons.SortStable((a, b) => { return (int)((b.MarketValue - a.MarketValue) * 1000); });
                int i = 0;
                foreach(var weapon in meleeWeapons)
                {
                    ThingDefStuffDefPair weaponMemory = weapon.toThingDefStuffDefPair();
                    if (!dupeCounters.ContainsKey(weaponMemory))
                        dupeCounters[weaponMemory] = 0;

                    bool isDupe = dupeCounters[weaponMemory] > 0;
                    carriedWeaponsTemp.Add(weapon, isDupe);
                    if (!Settings.GizmoPerformanceMode || mouseOver)
                    {
                        var iconOffset = new Vector2((IconSize * i) + IconGap * (i - 1) + SelectorPanelWidth, IconSize + IconGap);
                        DrawIconForWeapon(parent, pawnMemory, weapon, isDupe, contentRect, iconOffset);
                    }

                    i++;
                    dupeCounters[weaponMemory] += weapon.stackCount;
                }
                total += i;
            }

            dupeCounters.Clear();

            if (pawnMemory != null)
            {
                var meleeWeaponMemoriesSorted = meleeWeaponMemories.ToList();
                meleeWeaponMemoriesSorted.SortStable((a, b) => { return (int)((b.Price - a.Price) * 1000); });
                var grouped = meleeWeaponMemoriesSorted.GroupBy(m => m);

                int j = 0;
                foreach (var group in grouped)
                {
                    var weaponMemory = group.Key;
                    var stackCount = group.Count();
                    if (!dupeCounters.ContainsKey(weaponMemory))
                        dupeCounters[weaponMemory] = 0;

                    int missingCount = parent.missingCountWeaponsOfType(weaponMemory, stackCount, dupeCounters[weaponMemory]);
                    if (missingCount > 0)
                    {
                        bool isDupe = dupeCounters[weaponMemory] > 0;
                        Dictionary<bool, int> info = new Dictionary<bool, int>();
                        info.Add(isDupe, missingCount);
                        weaponMemoriesTemp.Add(weaponMemory, info);
                        if (!Settings.GizmoPerformanceMode || mouseOver)
                        {
                            var iconOffset = new Vector2((IconSize * (total + j)) + IconGap * ((total + j) - 1) + SelectorPanelWidth, IconSize + IconGap);
                            DrawIconForWeaponMemory(parent, pawnMemory, weaponMemory, missingCount, isDupe, contentRect, iconOffset);
                        }
                        j++;
                    }
                    dupeCounters[weaponMemory] += stackCount;
                }
                total += j;
            }
            if (Settings.GizmoPerformanceMode && !mouseOver)
            {
                total = 0;
            }

            var unarmedIconOffset = new Vector2((IconSize * total) + IconGap * (total - 1) + SelectorPanelWidth, IconSize + IconGap);
            DrawIconForUnarmed(parent, pawnMemory, contentRect, unarmedIconOffset);

            Rect selectorPanel = new Rect(gizmoRect.x + ContentPadding, gizmoRect.y + ContentPadding, SelectorPanelWidth - ContentPadding * 2, MinGizmoSize - ContentPadding * 2);

            DrawPreferenceSelector(parent, pawnMemory, selectorPanel);

            UIHighlighter.HighlightOpportunity(gizmoRect, "SidearmList");

            if (!Settings.SettingsEverOpened)
            {
                Rect position = new Rect((gizmoRect.x + gizmoRect.width - (FirstTimeSettingsWarningWidth + 2)), gizmoRect.y + 4, FirstTimeSettingsWarningWidth, FirstTimeSettingsWarningWidth);
                float brightness = Pulser.PulseBrightness(1f, 0.5f);
                GUI.color = new Color(brightness, brightness, 0f);
                GUI.DrawTexture(position, TextureResources.FirstTimeSettingsWarningIcon);
                if (Widgets.ButtonInvisible(position))
                {
                    var dialog = new Dialog_ModSettings(ModSingleton);
                    //Find.WindowStack.Add(new Dialog_ModSettings(mod));
                    Find.WindowStack.Add(dialog);
                }
                TooltipHandler.TipRegion(position, "FirstTimeSettingsWarning".Translate());
            }

            GUI.color = Color.white;

            if (parent.IsColonistPlayerControlled)
                DrawGizmoLabel(defaultLabel, gizmoRect);
            else
                DrawGizmoLabel(defaultLabel+" (godmode)", gizmoRect);
            return interactedWith != SidearmsListInteraction.None ? new GizmoResult(GizmoState.Interacted, Event.current) : new GizmoResult(GizmoState.Clear);
        }


        public void DrawPreferenceSelector(Pawn pawn, CompSidearmMemory pawnMemory, Rect contentRect)
        {
            var rangedIconRect = new Rect(contentRect.x, contentRect.y, PreferenceIconWidth, PreferenceIconHeight);
            var skillIconRect = new Rect(contentRect.x, contentRect.y+ PreferenceIconHeight + IconGap, PreferenceIconWidth, PreferenceIconHeight);
            var meleeIconRect = new Rect(contentRect.x, contentRect.y+ (PreferenceIconHeight + IconGap) * 2, PreferenceIconWidth, PreferenceIconHeight);

            var skillPref = pawn.getSkillWeaponPreference();

            if (Mouse.IsOver(contentRect))
            {
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsBasic, OpportunityType.Important);
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsPreference, OpportunityType.Important);
            }

            if (Mouse.IsOver(rangedIconRect))
            {
                if (pawnMemory.primaryWeaponMode == PrimaryWeaponMode.Ranged)
                    GUI.color = preferenceHighlightSet;
                else
                    GUI.color = preferenceHighlight;
                GUI.DrawTexture(rangedIconRect, TextureResources.preferRanged);
                TooltipHandler.TipRegion(rangedIconRect, string.Format("SidearmPreference_Ranged".Translate()));
                MouseoverSounds.DoRegion(rangedIconRect, SoundDefOf.Mouseover_Command);
            }
            else
            {
                if (pawnMemory.primaryWeaponMode == PrimaryWeaponMode.Ranged)
                    GUI.color = preferenceSet;
                else if (pawnMemory.primaryWeaponMode == PrimaryWeaponMode.BySkill && skillPref == PrimaryWeaponMode.Ranged)
                    GUI.color = preferenceOfSkill;
                else
                    GUI.color = preferenceBase;
                GUI.DrawTexture(rangedIconRect, TextureResources.preferRanged);
            }

            if (Mouse.IsOver(skillIconRect))
            {
                if (pawnMemory.primaryWeaponMode == PrimaryWeaponMode.BySkill)
                    GUI.color = preferenceHighlightSet;
                else
                    GUI.color = preferenceHighlight;
                GUI.DrawTexture(skillIconRect, TextureResources.preferSkilled);
                TooltipHandler.TipRegion(skillIconRect, string.Format("SidearmPreference_Skill".Translate()));
                MouseoverSounds.DoRegion(skillIconRect, SoundDefOf.Mouseover_Command);
            }
            else
            {
                if (pawn.skills != null)
                {
                    if (pawnMemory.primaryWeaponMode == PrimaryWeaponMode.BySkill)
                        GUI.color = preferenceSet;
                    else
                        GUI.color = preferenceBase;
                    GUI.DrawTexture(skillIconRect, TextureResources.preferSkilled);
                }
            }

            if (Mouse.IsOver(meleeIconRect))
            {
                if (pawnMemory.primaryWeaponMode == PrimaryWeaponMode.Melee)
                    GUI.color = preferenceHighlightSet;
                else
                    GUI.color = preferenceHighlight;
                GUI.DrawTexture(meleeIconRect, TextureResources.preferMelee);
                TooltipHandler.TipRegion(meleeIconRect, string.Format("SidearmPreference_Melee".Translate()));
                MouseoverSounds.DoRegion(meleeIconRect, SoundDefOf.Mouseover_Command);
            }
            else
            {
                if (pawnMemory.primaryWeaponMode == PrimaryWeaponMode.Melee)
                    GUI.color = preferenceSet;
                else if (pawnMemory.primaryWeaponMode == PrimaryWeaponMode.BySkill && skillPref == PrimaryWeaponMode.Melee)
                    GUI.color = preferenceOfSkill;
                else
                    GUI.color = preferenceBase;
                GUI.DrawTexture(meleeIconRect, TextureResources.preferMelee);
            }

            UIHighlighter.HighlightOpportunity(rangedIconRect, "SidearmPreferenceButton");
            UIHighlighter.HighlightOpportunity(skillIconRect, "SidearmPreferenceButton");
            UIHighlighter.HighlightOpportunity(meleeIconRect, "SidearmPreferenceButton");

            if (Widgets.ButtonInvisible(rangedIconRect, true))
            {
                interactedWith = SidearmsListInteraction.SelectorRanged;
            }
            if (Widgets.ButtonInvisible(skillIconRect, true))
            {
                interactedWith = SidearmsListInteraction.SelectorSkill;
            }
            if (Widgets.ButtonInvisible(meleeIconRect, true))
            {
                interactedWith = SidearmsListInteraction.SelectorMelee;
            }
        }

        public void DrawIconForWeaponMemory(Pawn pawn, CompSidearmMemory pawnMemory, ThingDefStuffDefPair weaponType, int stackCount, bool isDuplicate, Rect contentRect, Vector2 iconOffset)
        {
            Graphic g = weaponType.thing.graphicData.Graphic;

            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);

            Texture2D drawPocket;
            drawPocket = TextureResources.drawPocketMemory;

            if (pawn.Drafted)
            {
                TooltipHandler.TipRegion(iconRect, string.Format("DrawSidearm_gizmoTooltipMemoryWhileDrafted".Translate(), weaponType.getLabel()));
            }
            else
            {
                TooltipHandler.TipRegion(iconRect, string.Format("DrawSidearm_gizmoTooltipMemory".Translate(), weaponType.getLabel()));
            }
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);
            if (Mouse.IsOver(iconRect))
            {
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SidearmsMissing, OpportunityType.GoodToKnow);

                GUI.color = iconMouseOverColor;
                GUI.DrawTexture(iconRect, drawPocket);
            }
            else
            {
                GUI.color = iconBaseColor;
                GUI.DrawTexture(iconRect, drawPocket);
            }

            Graphic outerGraphic = weaponType.thing.graphic;
            if (outerGraphic is Graphic_StackCount)
                outerGraphic = (outerGraphic as Graphic_StackCount).SubGraphicForStackCount(stackCount, weaponType.thing);

            Material material = outerGraphic.ExtractInnerGraphicFor(null).MatAt(weaponType.thing.defaultPlacingRot, null);
            Texture resolvedIcon = (Texture2D)material.mainTexture;
            GUI.color = weaponType.getDrawColor();
            GUI.DrawTexture(iconRect, resolvedIcon);
            GUI.color = Color.white;

            if (stackCount > 1)
            {
                var store = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(iconRect, stackCount.ToString());
                Text.Anchor = store;
            }

            if (!isDuplicate)
            {
                GUI.color = Color.white;

                if (pawnMemory.ForcedWeapon == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.forcedAlways);
                
                if(weaponType.thing.IsRangedWeapon & pawnMemory.DefaultRangedWeapon == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.defaultRanged);
                else if (pawnMemory.PreferredMeleeWeapon == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.preferredMelee);

                GUI.color = Color.white;
            }

            UIHighlighter.HighlightOpportunity(iconRect, "SidearmMissing");

            if (Widgets.ButtonInvisible(iconRect, true))
            {
                interactedWith = SidearmsListInteraction.WeaponMemory;
                interactionWeaponType = weaponType;
                interactionWeaponIsDuplicate = isDuplicate;
            }
        }

        public void DrawIconForWeapon(Pawn pawn, CompSidearmMemory pawnMemory, ThingWithComps weapon, bool isDuplicate, Rect contentRect, Vector2 iconOffset)
        {
            if (weapon is null || weapon.def is null || weapon.def.uiIcon is null)
                return;

            ThingDefStuffDefPair weaponType = weapon.toThingDefStuffDefPair();

            bool allowInteraction = StatCalculator.canUseSidearmInstance(weapon, pawn, out string interactionBlockedReason) || Settings.AllowBlockedWeaponUse;

            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);
            //var iconColor = iconBaseColor;

            string hoverText;
            if (allowInteraction)
            {
                if (pawn.Drafted)
                {
                    if (pawnMemory.ForcedWeaponWhileDrafted == weapon.toThingDefStuffDefPair())
                        hoverText = "DrawSidearm_gizmoTooltipForcedWhileDrafted".Translate();
                    else
                        hoverText = "DrawSidearm_gizmoTooltipWhileDrafted".Translate();
                }
                else
                {
                    if (pawnMemory.ForcedWeapon == weapon.toThingDefStuffDefPair())
                        hoverText = "DrawSidearm_gizmoTooltipForced".Translate();
                    else
                    {
                        if (weapon.def.IsRangedWeapon)
                        {
                            if (pawnMemory.DefaultRangedWeapon == weaponType)
                                hoverText = "DrawSidearm_gizmoTooltipRangedDefault".Translate();
                            else
                                hoverText = "DrawSidearm_gizmoTooltipRanged".Translate();
                        }
                        else
                        {
                            if (pawnMemory.PreferredMeleeWeapon == weaponType)
                                hoverText = "DrawSidearm_gizmoTooltipMeleePreferred".Translate();
                            else
                                hoverText = "DrawSidearm_gizmoTooltipMelee".Translate();
                        }
                    }
                }
            }
            else 
            {
                hoverText = "DrawSidearm_blocked".Translate() + ": " + interactionBlockedReason;
            }

            TooltipHandler.TipRegion(iconRect, string.Format(hoverText, weapon.toThingDefStuffDefPair().getLabel()));
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);

            Texture2D drawPocket;
            if (weaponMemories.Contains(weapon.toThingDefStuffDefPair()))
            {
                drawPocket = TextureResources.drawPocket;
            }
            else
            {
                drawPocket = TextureResources.drawPocketTemp;
            }

            if (Mouse.IsOver(iconRect))
            {
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SidearmsDropping, OpportunityType.GoodToKnow);

                if (pawn.Drafted)
                {
                    LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsAdvancedDrafted, OpportunityType.GoodToKnow);
                }
                else 
                {
                    if (weapon.def.IsRangedWeapon)
                        LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, OpportunityType.GoodToKnow);
                    else
                        LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, OpportunityType.GoodToKnow);
                }

                GUI.color = iconMouseOverColor;
                GUI.DrawTexture(iconRect, drawPocket);
                //Graphics.DrawTexture(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, iconMouseOverColor);
            }
            else
            {
                GUI.color = iconBaseColor;
                GUI.DrawTexture(iconRect, drawPocket);
                //Graphics.DrawTexture(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, iconBaseColor);
            }

            Graphic outerGraphic = weaponType.thing.graphic;
            if (outerGraphic is Graphic_StackCount)
                outerGraphic = (outerGraphic as Graphic_StackCount).SubGraphicForStackCount(weapon.stackCount, weaponType.thing);

            Material material = outerGraphic.ExtractInnerGraphicFor(null).MatAt(weaponType.thing.defaultPlacingRot, null);
            Texture resolvedIcon = (Texture2D)material.mainTexture;
            GUI.color = weapon.DrawColor;
            GUI.DrawTexture(iconRect, resolvedIcon);
            GUI.color = Color.white;

            if (GettersFilters.isManualUse(weapon))
            {
                GUI.DrawTexture(iconRect, TextureResources.weaponTypeManual);
            }
            if (GettersFilters.isDangerousWeapon(weapon))
            {
                GUI.DrawTexture(iconRect, TextureResources.weaponTypeDangerous);
            }
            if (GettersFilters.isEMPWeapon(weapon))
            {
                GUI.DrawTexture(iconRect, TextureResources.weaponTypeEMP);
            }

            if (!allowInteraction) 
            {
                GUI.DrawTexture(iconRect, TextureResources.blockedWeapon);
            }

            if (weapon.stackCount > 1)
            {
                var store = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(iconRect, weapon.stackCount.ToString());
                Text.Anchor = store;
            }

            if (!isDuplicate)
            {
                GUI.color = Color.white;


                if (pawnMemory.ForcedWeaponWhileDrafted == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.forcedDrafted);

                if (pawnMemory.ForcedWeapon == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.forcedAlways);

                if (weaponType.thing.IsRangedWeapon & pawnMemory.DefaultRangedWeapon == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.defaultRanged);
                else if (pawnMemory.PreferredMeleeWeapon == weaponType)
                    GUI.DrawTexture(iconRect, TextureResources.preferredMelee);

                GUI.color = Color.white;
            }


            if (allowInteraction)
            {
                UIHighlighter.HighlightOpportunity(iconRect, "SidearmInInventory");
                if (weapon.def.IsRangedWeapon)
                    UIHighlighter.HighlightOpportunity(iconRect, "SidearmInInventoryRanged");
                else
                    UIHighlighter.HighlightOpportunity(iconRect, "SidearmInInventoryMelee");

                if (Widgets.ButtonInvisible(iconRect, true))
                {
                    interactedWith = SidearmsListInteraction.Weapon;
                    interactionWeapon = weapon;
                    interactionWeaponIsDuplicate = isDuplicate;
                }
            }
        }

        public void DrawIconForUnarmed(Pawn pawn, CompSidearmMemory pawnMemory, Rect contentRect, Vector2 iconOffset)
        {
            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);
            //var iconColor = iconBaseColor;

            string hoverText;
            if (pawn.Drafted)
            {
                if (pawnMemory.ForcedUnarmedWhileDrafted)
                    hoverText = "DrawSidearm_gizmoTooltipUnarmedForcedWhileDrafted";
                else
                    hoverText = "DrawSidearm_gizmoTooltipUnarmedWhileDrafted";
            }
            else
            {
                if (pawnMemory.ForcedUnarmed)
                    hoverText = "DrawSidearm_gizmoTooltipUnarmedForced";
                else if (pawnMemory.PreferredUnarmed)
                    hoverText = "DrawSidearm_gizmoTooltipUnarmedPreferred";
                else
                    hoverText = "DrawSidearm_gizmoTooltipUnarmed";

            }

            TooltipHandler.TipRegion(iconRect, hoverText.Translate());
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);

            Texture2D drawPocket = TextureResources.drawPocket;

            if (Mouse.IsOver(iconRect))
            {
                GUI.color = iconMouseOverColor;
                GUI.DrawTexture(iconRect, drawPocket);
                //Graphics.DrawTexture(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, iconMouseOverColor);
            }
            else
            {
                GUI.color = iconBaseColor;
                GUI.DrawTexture(iconRect, drawPocket);
                //Graphics.DrawTexture(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, iconBaseColor);
            }

            Texture resolvedIcon = TexCommand.AttackMelee;
            GUI.color = Color.white;
            GUI.DrawTexture(iconRect, resolvedIcon);
            GUI.color = Color.white;

            GUI.color = Color.white; 
            
            if (pawnMemory.ForcedUnarmedWhileDrafted)
                GUI.DrawTexture(iconRect, TextureResources.forcedDrafted);
            
            if (pawnMemory.ForcedUnarmed)
                GUI.DrawTexture(iconRect, TextureResources.forcedAlways);
            
            if (pawnMemory.PreferredUnarmed)
                GUI.DrawTexture(iconRect, TextureResources.preferredMelee);
            else 
            GUI.color = Color.white;

            if (Widgets.ButtonInvisible(iconRect, true))
            {
                interactedWith = SidearmsListInteraction.Unarmed;
            }
        }

        public override void ProcessInput(Event ev)
        {
            if (activateSound != null)
            {
                activateSound.PlayOneShotOnCamera();
            }
            if (ev.button < 0)
            {
                if (hotkeyAction != null)
                    hotkeyAction();
            }
            else {
                handleInteraction(interactedWith, ev);
                interactedWith = SidearmsListInteraction.None;
                //iconClickAction(ev.button);
            }

        }

        //Ive rewritten this twice now and its still an ugly monster.
        public const int LEFT_CLICK = 0;
        public const int RIGHT_CLICK = 1;
        public void handleInteraction(SidearmsListInteraction interaction, Event ev)
        {
            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(parent);
            if (pawnMemory == null)
                return;

            var dropMode = parent.Drafted ? DroppingModeEnum.Combat : DroppingModeEnum.Calm;


            if (ev.button == LEFT_CLICK)
            {
                switch (interaction)
                {
                    case SidearmsListInteraction.SelectorRanged:
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsPreference, KnowledgeAmount.SpecificInteraction);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                        pawnMemory.primaryWeaponMode = PrimaryWeaponMode.Ranged;
                        break;
                    case SidearmsListInteraction.SelectorSkill:
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsPreference, KnowledgeAmount.SpecificInteraction);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                        pawnMemory.primaryWeaponMode = PrimaryWeaponMode.BySkill;
                        break;
                    case SidearmsListInteraction.SelectorMelee:
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsPreference, KnowledgeAmount.SpecificInteraction);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                        pawnMemory.primaryWeaponMode = PrimaryWeaponMode.Melee;
                        break;
                    case SidearmsListInteraction.Weapon:
                        Thing weapon = interactionWeapon;
                        ThingDefStuffDefPair weaponType = weapon.toThingDefStuffDefPair();
                        if (parent.Drafted)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedDrafted, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.SetWeaponAsForced(weaponType, true);
                            if (parent.equipment.Primary != weapon && weapon is ThingWithComps)
                                WeaponAssingment.equipSpecificWeaponTypeFromInventory(parent, weaponType, MiscUtils.shouldDrop(parent, dropMode, false), false);
                        }
                        else if (pawnMemory.DefaultRangedWeapon == weaponType || pawnMemory.PreferredMeleeWeapon == weaponType || weaponType.isToolNotWeapon())
                        {
                            if(weaponType.thing.IsRangedWeapon)
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                            else
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.SetWeaponAsForced(weaponType, false);
                            if (parent.equipment.Primary != weapon && weapon is ThingWithComps)
                                WeaponAssingment.equipSpecificWeaponTypeFromInventory(parent, weaponType, MiscUtils.shouldDrop(parent, dropMode, false), false);
                        }
                        else
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
                            if (weaponType.thing.IsRangedWeapon)
                            {
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                pawnMemory.SetRangedWeaponTypeAsDefault(weaponType);
                            }
                            else
                            {
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                pawnMemory.SetMeleeWeaponTypeAsPreferred(weaponType);
                            }
                        }
                        break;
                    case SidearmsListInteraction.WeaponMemory:

                        ThingDefStuffDefPair weaponMemory = interactionWeaponType.Value;
                        if (parent.Drafted)
                        {
                            //allow nothing
                        }
                        else
                        {
                            if (weaponMemory.thing.IsRangedWeapon)
                            {
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                pawnMemory.SetRangedWeaponTypeAsDefault(weaponMemory);
                            }
                            else
                            {
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                pawnMemory.SetMeleeWeaponTypeAsPreferred(weaponMemory);
                            }
                        }
                        break;
                    case SidearmsListInteraction.Unarmed:
                        if (parent.Drafted)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedDrafted, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.SetUnarmedAsForced(true);
                            if (parent.equipment.Primary != null)
                                WeaponAssingment.equipSpecificWeapon(parent, null, MiscUtils.shouldDrop(parent, dropMode, false), false);
                        }
                        else if (pawnMemory.PreferredUnarmed)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.SetUnarmedAsForced(false);
                            if (parent.equipment.Primary != null)
                                WeaponAssingment.equipSpecificWeapon(parent, null, MiscUtils.shouldDrop(parent, dropMode, false), false);
                        }
                        else 
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.SetUnarmedAsPreferredMelee();
                        }
                        break;
                    case SidearmsListInteraction.None:
                    default:
                        return;
                }
            }
            else if(ev.button == RIGHT_CLICK)
            {
                switch (interaction)
                {
                    case SidearmsListInteraction.SelectorRanged:
                    case SidearmsListInteraction.SelectorSkill:
                    case SidearmsListInteraction.SelectorMelee:
                        break;
                    case SidearmsListInteraction.Weapon:
                        Thing weapon = interactionWeapon;
                        ThingDefStuffDefPair weaponType = weapon.toThingDefStuffDefPair();

                        if (interactionWeaponIsDuplicate)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsDropping, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            WeaponAssingment.dropSidearm(parent, weapon, true);
                        }
                        else
                        {
                            if (parent.Drafted)
                            {
                                if (pawnMemory.ForcedWeaponWhileDrafted == weaponType)
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedDrafted, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetForcedWeapon(true);
                                }
                            }
                            else
                            {
                                if (pawnMemory.ForcedWeapon == weaponType)
                                {
                                    if (weaponType.thing.IsRangedWeapon)
                                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                    else
                                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetForcedWeapon(false);
                                }
                                else if (weaponType.thing.IsRangedWeapon & pawnMemory.DefaultRangedWeapon == weaponType)
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetRangedWeaponDefault();
                                }
                                else if (pawnMemory.PreferredMeleeWeapon == weaponType)
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetMeleeWeaponPreference();
                                }
                                else
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsDropping, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    WeaponAssingment.dropSidearm(parent, weapon, true);
                                }
                            }
                        }

                        break;
                    case SidearmsListInteraction.WeaponMemory:
                        ThingDefStuffDefPair weaponMemory = interactionWeaponType.Value;

                        if (interactionWeaponIsDuplicate)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsMissing, KnowledgeAmount.SmallInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.ForgetSidearmMemory(weaponMemory);
                        }
                        else
                        {
                            if (parent.Drafted)
                            {
                                if (pawnMemory.ForcedWeaponWhileDrafted == weaponMemory)
                                {
                                    if (weaponMemory.thing.IsRangedWeapon)
                                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                    else
                                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetForcedWeapon(true);
                                }
                            }
                            else
                            {
                                if (pawnMemory.ForcedWeapon == weaponMemory)
                                {
                                    if (weaponMemory.thing.IsRangedWeapon)
                                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                    else
                                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetForcedWeapon(false);
                                }
                                else if (weaponMemory.thing.IsRangedWeapon & pawnMemory.DefaultRangedWeapon == weaponMemory)
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedRanged, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetRangedWeaponDefault();
                                }
                                else if (pawnMemory.PreferredMeleeWeapon == weaponMemory)
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.UnsetMeleeWeaponPreference();
                                }
                                else
                                {
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SidearmsMissing, KnowledgeAmount.SpecificInteraction);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                                    pawnMemory.ForgetSidearmMemory(weaponMemory);
                                }
                            }
                        }

                        break;
                    case SidearmsListInteraction.Unarmed:
                        if (parent.Drafted && pawnMemory.ForcedUnarmedWhileDrafted)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedDrafted, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.UnsetUnarmedAsForced(true);
                        }
                        else if (pawnMemory.ForcedUnarmed)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.UnsetUnarmedAsForced(false);
                        }
                        else if (pawnMemory.PreferredUnarmed)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsAdvancedMelee, KnowledgeAmount.SpecificInteraction);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);

                            pawnMemory.UnsetMeleeWeaponPreference();
                        }
                        break;
                    case SidearmsListInteraction.None:
                    default:
                        return;
                }
            }
        }


        public void DrawGizmoLabel(string labelText, Rect gizmoRect)
        {
            var labelHeight = Text.CalcHeight(labelText, gizmoRect.width);
            labelHeight -= 2f;
            var labelRect = new Rect(gizmoRect.x, gizmoRect.yMax - labelHeight + 12f, gizmoRect.width, labelHeight);
            GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(labelRect, labelText);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }


        public int countMissingMeleeWeapons(CompSidearmMemory pawnMemory, Pawn pawn)
        {
            if (pawnMemory == null)
                return 0;

            int count = 0;

            Dictionary<ThingDefStuffDefPair, int> dupeCounters = new Dictionary<ThingDefStuffDefPair, int>();

            var weaponMemories = meleeWeaponMemories.ToList();
            var grouped = weaponMemories.GroupBy(m => m);

            foreach (var group in grouped)
            {
                var weapon = group.Key;
                var stackCount = group.Count();

                if (!dupeCounters.ContainsKey(weapon))
                    dupeCounters[weapon] = 0;

                var missingWeapons = pawn.missingCountWeaponsOfType(weapon, stackCount, dupeCounters[weapon]);
                if (missingWeapons > 0)
                    count++;

                dupeCounters[weapon] += stackCount;
            }

            return count;
        }

        public int countMissingRangedWeapons(CompSidearmMemory pawnMemory, Pawn pawn)
        {
            if (pawnMemory == null)
                return 0;

            int count = 0;

            Dictionary<ThingDefStuffDefPair, int> dupeCounters = new Dictionary<ThingDefStuffDefPair, int>();
            var weaponMemories = rangedWeaponMemories.ToList();
            var grouped = weaponMemories.GroupBy(m => m);

            foreach (var group in grouped)
            {
                var weapon = group.Key;
                var stackCount = group.Count();

                if (!dupeCounters.ContainsKey(weapon))
                    dupeCounters[weapon] = 0;

                var missingWeapons = pawn.missingCountWeaponsOfType(weapon, stackCount, dupeCounters[weapon]);
                if (missingWeapons > 0)
                    count++;

                dupeCounters[weapon] += stackCount;
            }

            return count;
        }

    }
}
