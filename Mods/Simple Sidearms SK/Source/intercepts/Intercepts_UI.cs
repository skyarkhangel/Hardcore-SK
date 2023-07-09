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
            //Log.Message("GetGizmos PostFix Called");
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
                    List<ThingDefStuffDefPair> rememberedWeapons = pawnMemory.RememberedWeapons;

                    foreach (ThingDefStuffDefPair weapon in rememberedWeapons)
                    {
                        if (weapon.thing.IsMeleeWeapon)
                            meleeWeaponMemories.Add(weapon);
                        else if (weapon.thing.IsRangedWeapon)
                            rangedWeaponMemories.Add(weapon);
                    }

                    yield return new Gizmo_SidearmsList(__instance, carriedWeapons, rememberedWeapons);

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
                if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    return;
                if (pawn.IsQuestLodger())
                    return;

                IntVec3 c = IntVec3.FromVector3(clickPos);

                if (!pawn.CanReach(new LocalTargetInfo(c), PathEndMode.ClosestTouch, Danger.Deadly))
                    return;

                if (pawn.equipment != null)
                {
                    foreach (var thing in c.GetThingList(pawn.Map))
                    {
                        var thingWithComps = thing as ThingWithComps;
                        if (thingWithComps == null)
                            continue;
                        if (!thingWithComps.def.IsWeapon)
                            continue;
                        if (thingWithComps.IsBurning())
                            continue;

                        bool toolUse = ((pawn.CombinedDisabledWorkTags & WorkTags.Violent) != 0) || thingWithComps.toThingDefStuffDefPair().isToolNotWeapon();
                        string textPostfix = toolUse ? "AsTool".Translate() : "AsSidearm".Translate();
                        if (!StatCalculator.canUseSidearmInstance(thingWithComps, pawn, out string errStr))
                        {
                            string orderText = "CannotEquip".Translate(thingWithComps.LabelShort) + textPostfix;

                            var order = new FloatMenuOption(orderText + ": " + errStr, null, MenuOptionPriority.Default, null, null, 0f, null, null);
                            opts.Add(order);
                        }
                        else
                        {
                            string orderText = "Equip".Translate(thingWithComps.LabelShort) + textPostfix;

                            if (thingWithComps.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                            {
                                orderText = orderText + " " + "EquipWarningBrawler".Translate();
                            }

                            var order = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(orderText, delegate
                            {
                                thingWithComps.SetForbidden(false, true);
                                pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, thingWithComps), JobTag.Misc);
                                //MoteMaker.MakeStaticMote(equipment.DrawPos, equipment.Map, ThingDefOf.Mote_FeedbackEquip, 1f); //why is this gone?

                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
                            }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, thingWithComps, "ReservedBy");
                            opts.Add(order);
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
