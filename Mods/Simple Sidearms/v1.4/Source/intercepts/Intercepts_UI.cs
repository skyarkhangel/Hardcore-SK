using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using SimpleSidearms.rimworld;
using PeteTimesSix.SimpleSidearms.Utilities;

namespace PeteTimesSix.SimpleSidearms.Intercepts
{
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "InterfaceDrop")]
    public static class ITab_Pawn_Gear_InterfaceDrop_Prefix
    {
        [HarmonyPrefix]
        public static void InterfaceDrop(ITab_Pawn_Gear __instance, Thing t)
        {
            if (t.def.IsMeleeWeapon || t.def.IsRangedWeapon)
            {
                ThingWithComps thingWithComps = t as ThingWithComps;
                ThingOwner thingOwner = thingWithComps.holdingOwner;
                IThingHolder actualOwner = thingOwner.Owner;
                if (actualOwner is Pawn_InventoryTracker)
                {
                    CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn((actualOwner as Pawn_InventoryTracker).pawn);
                    if (pawnMemory == null)
                        return;
                    pawnMemory.InformOfDroppedSidearm(thingWithComps, true);
                }
                else if (actualOwner is Pawn_EquipmentTracker)
                {
                    CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn((actualOwner as Pawn_EquipmentTracker).ParentHolder as Pawn);
                    if (pawnMemory == null)
                        return;
                    pawnMemory.InformOfDroppedSidearm(thingWithComps, true);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Pawn_GetGizmos_Postfix
    {
        [HarmonyPostfix]
        //public static void GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            //This postfix inserts the SimpleSidearms gizmo before all other gizmos
            if (__instance.IsValidSidearmsCarrier() && (__instance.IsColonistPlayerControlled
                || DebugSettings.godMode) && __instance.equipment != null && __instance.inventory != null
                )
            {
                IEnumerable<ThingWithComps> carriedWeapons = __instance.getCarriedWeapons(includeTools: true);

                CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(__instance);
                if (pawnMemory != null)
                {
                    List<ThingDefStuffDefPair> rangedWeaponMemories = new List<ThingDefStuffDefPair>();
                    List<ThingDefStuffDefPair> meleeWeaponMemories = new List<ThingDefStuffDefPair>();

                    foreach (ThingDefStuffDefPair weapon in pawnMemory.RememberedWeapons)
                    {
                        if (weapon.thing.IsMeleeWeapon)
                            meleeWeaponMemories.Add(weapon);
                        else if (weapon.thing.IsRangedWeapon)
                            rangedWeaponMemories.Add(weapon);
                    }

                    yield return new Gizmo_SidearmsList(__instance, carriedWeapons, pawnMemory.RememberedWeapons);

                    if (DebugSettings.godMode)
                    {
                        yield return new Gizmo_Brainscope(__instance);
                    }
                }
            }

            foreach (var aGizmo in __result)
            {
                yield return aGizmo;
            }
        }
    }


    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Pawn), typeof(List<FloatMenuOption>) })]
    public static class FloatMenuMakerMap_AddHumanLikeOrders_Postfix
    {
        [HarmonyPostfix]
        public static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            try
            {
                IntVec3 c = IntVec3.FromVector3(clickPos);
                if (pawn.equipment != null)
                {
                    ThingWithComps equipment = null;
                    List<Thing> thingList = c.GetThingList(pawn.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if (thingList[i].TryGetComp<CompEquippable>() != null)
                        {
                            equipment = (ThingWithComps)thingList[i];
                            break;
                        }
                    }
                    if (equipment != null)
                    {
                        string labelShort = equipment.LabelShort;
                        string errStr;
                        FloatMenuOption item3;
                        /*if ((!equipment.toThingStuffPair().isToolNotWeapon()) && ((pawn.CombinedDisabledWorkTags & WorkTags.Violent) != 0))
                        {
                        }*/
                        //if (!pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                        if (!pawn.CanReach(new LocalTargetInfo(equipment), PathEndMode.ClosestTouch, Danger.Deadly))
                        {
                        }
                        else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                        {
                        }
                        else if (!equipment.def.IsWeapon)
                        {
                        }
                        else if (equipment.IsBurning())
                        {
                        }
                        else if (pawn.IsQuestLodger())
                        {
                        }
                        else if (!StatCalculator.canCarrySidearmInstance(equipment, pawn, out errStr))
                        {
                            "CannotEquip".Translate();
                            item3 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + errStr, null, MenuOptionPriority.Default, null, null, 0f, null, null);
                            opts.Add(item3);
                        }
                        else
                        {
                            string text2 = "Equip".Translate(labelShort);
                            if(((pawn.CombinedDisabledWorkTags & WorkTags.Violent) != 0) || equipment.toThingDefStuffDefPair().isToolNotWeapon())
                                text2 += "AsTool".Translate();
                            else
                                text2 += "AsSidearm".Translate();


                            if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                            {
                                text2 = text2 + " " + "EquipWarningBrawler".Translate();
                            }

                            item3 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text2, delegate
                            {
                                equipment.SetForbidden(false, true);
                                pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, equipment), JobTag.Misc);
                                //MoteMaker.MakeStaticMote(equipment.DrawPos, equipment.Map, ThingDefOf.Mote_FeedbackEquip, 1f); //why is this gone?
                            
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
                            }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, equipment, "ReservedBy");
                            opts.Add(item3);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception during SimpleSidearms floatmenumaker intercept. Cancelling intercept. Exception: " + e.ToString());
            }
        }
    }
}
