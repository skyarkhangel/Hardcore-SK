using HarmonyLib;
using RimWorld;
using SimpleSidearms.rimworld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;

namespace PeteTimesSix.SimpleSidearms
{
    public static class Extensions
    {
        public static void DoModSettings(this Dialog_ModSettings instance, Mod mod) 
        {
            AccessTools.Field(typeof(Dialog_ModSettings), "selMod").SetValue(instance, mod);
        }

        public static string getLabelCap(this ThingDefStuffDefPair pair)
        {
            return pair.getLabel().CapitalizeFirst();
        }

        public static string getLabel(this ThingDefStuffDefPair pair)
        {
            if (pair.stuff != null)
                return pair.stuff.LabelAsStuff + " " + pair.thing.label;
            else
                return pair.thing.label;
        }

        public static Color getDrawColor(this ThingDefStuffDefPair pair)
        {
            if (pair.stuff != null)
            {
                return pair.stuff.stuffProps.color;
            }
            if (pair.thing.graphicData != null)
            {
                return pair.thing.graphicData.color;
            }
            return Color.white;
        }

        public static Color getDrawColorTwo(this ThingDefStuffDefPair pair)
        {
            if (pair.thing.graphicData != null)
            {
                return pair.thing.graphicData.colorTwo;
            }
            return Color.white;
        }

        public static ThingDefStuffDefPair toThingDefStuffDefPair(this Thing thing)
        {
            if (thing == null)
                throw new ArgumentException("cannot turn null to ThingDefStuffDef pair!");
                //return default(ThingDefStuffDefPair);
            return new ThingDefStuffDefPair(thing.def, thing.Stuff);
        }

        public static float getBestStatBoost(this ThingDefStuffDefPair tool, List<StatDef> stats, out bool found)
        {
            if (tool.thing.equippedStatOffsets == null || tool.thing.equippedStatOffsets.Count == 0)
            {
                found = false;
                return 0;
            }

            //this is not great because not all stats are boosted equally
            //but its something

            float best = 0;
            found = false;
            foreach (StatModifier modifier in tool.thing.equippedStatOffsets)
            {
                if (stats.Contains(modifier.stat)) 
                {
                    found = true;
                    if(best < modifier.value)
                        best = modifier.value;
                }
            }
            return best;
        }


        public static bool isTool(this ThingDefStuffDefPair possibleTool)
        {
            if (isToolNotWeapon(possibleTool))
                return true;

            if (possibleTool.thing.equippedStatOffsets != null && possibleTool.thing.equippedStatOffsets.Count > 0)
                return true;

            return false;
        }

        public static bool isToolNotWeapon(this ThingDefStuffDefPair possibleTool)
        {
            if (
                possibleTool.thing.defName == "Gun_Fire_Ext" ||
                possibleTool.thing.defName == "VWE_Gun_FireExtinguisher"
                )
            {
                return true;
            }
            return false;
        }

        public static bool matchesThingDefStuffDefPair(this Thing thing, ThingDefStuffDefPair pair, bool allowPartialMatch = false)
        {
            bool retVal = false;
            if (thing == null)
                return false;
            var thisPair = thing.toThingDefStuffDefPair();
            if(thisPair.thing == pair.thing && thisPair.stuff == pair.stuff)
                retVal = true;
            else if(allowPartialMatch && thisPair.thing == pair.thing)
                retVal = true;

            return retVal;
        }

        public static bool IsValidSidearmsCarrier(this Pawn pawn) 
        {
            return pawn != null && !pawn.Dead && pawn.equipment != null && pawn.inventory != null && pawn.RaceProps.Humanlike;
        }

        public static PrimaryWeaponMode getSkillWeaponPreference(this Pawn pawn)
        {
            if (pawn.skills == null)
                return PrimaryWeaponMode.Ranged;

            SkillRecord rangedSkill = pawn.skills.GetSkill(SkillDefOf.Shooting);
            SkillRecord meleeSkill = pawn.skills.GetSkill(SkillDefOf.Melee);

            if (rangedSkill.passion > meleeSkill.passion)
                return PrimaryWeaponMode.Ranged;
            else if (meleeSkill.passion > rangedSkill.passion)
                return PrimaryWeaponMode.Melee;
            else if (meleeSkill.Level > rangedSkill.Level)
                return PrimaryWeaponMode.Melee;
            else
                return PrimaryWeaponMode.Ranged; //slight bias towards ranged but *shrug*
        }

        public static IEnumerable<ThingWithComps> getCarriedWeapons(this Pawn pawn, bool includeEquipped = true, bool includeTools = false)
        {
            List<ThingWithComps> weapons = new List<ThingWithComps>();

            if (pawn == null || pawn.equipment == null || pawn.inventory == null)
                return weapons;

            if (includeEquipped)
            {
                if (pawn.equipment.Primary != null && (!pawn.equipment.Primary.toThingDefStuffDefPair().isToolNotWeapon() || includeTools))
                    weapons.Add(pawn.equipment.Primary);
            }

            foreach (Thing item in pawn.inventory.innerContainer)
            {
                if (
                    item is ThingWithComps &&
                    (!item.toThingDefStuffDefPair().isToolNotWeapon() || includeTools) &&
                    (item.def.IsRangedWeapon || item.def.IsMeleeWeapon)
                    )
                {
                    var equippable = item.TryGetComp<CompEquippable>();
                    if(equippable != null)
                        weapons.Add(item as ThingWithComps);
                }
            }
            return weapons;
        }

        public static bool hasWeaponType(this Pawn pawn, ThingDefStuffDefPair weapon, int dupesToSkip = 0) 
        {
            return pawn.missingCountWeaponsOfType(weapon, 1, dupesToSkip) == 0;
        }

        public static int missingCountWeaponsOfType(this Pawn pawn, ThingDefStuffDefPair weapon, int countToSatisfy, int dupesToSkip = 0)
        {
            if (pawn == null)
            {
                Log.Warning("hasWeaponSomewhere got handed null pawn");
                return 0;
            }

            int dupesSoFar = 0;

            if (pawn.equipment != null)
                if (pawn.equipment.Primary != null)
                    if (pawn.equipment.Primary.matchesThingDefStuffDefPair(weapon))
                        dupesSoFar++;

            if (dupesSoFar - dupesToSkip >= countToSatisfy)
            {
                return 0;
            }

            if (pawn.inventory != null)
            {
                if (pawn.inventory.innerContainer != null)
                {
                    foreach (Thing thing in pawn.inventory.innerContainer)
                    {
                        if (thing.matchesThingDefStuffDefPair(weapon))
                        {
                            dupesSoFar += thing.stackCount;
                            if (dupesSoFar - dupesToSkip >= countToSatisfy)
                            {
                                return 0;
                            }
                        }
                    }
                }
            }
            return countToSatisfy - (dupesSoFar - dupesToSkip);
        }

        public static bool Contains(this Rect rect, Rect otherRect)
        {
            if (!rect.Contains(new Vector2(otherRect.xMin, otherRect.yMin)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMin, otherRect.yMax)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMax, otherRect.yMax)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMax, otherRect.yMin)))
                return false;
            return true;
        }

        public static bool ButtonText(Rect rect, string label, Color color, bool drawBackground = true, bool doMouseoverSound = false, bool active = true)
        {
            return Widgets.ButtonText(rect, label, drawBackground, doMouseoverSound, color, active);
        } 
    }
}
