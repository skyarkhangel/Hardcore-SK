using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;
using System.Reflection.Emit;
using System.Reflection;
using Verse.AI;

namespace PickUpAndHaul
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create(id: "mehni.rimworld.pickupandhaul.main");

            harmony.Patch(original: AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(FloatMenuMakerMad_AddHumanlikeOrders_Transpiler)));

            harmony.Patch(original: AccessTools.Method(typeof(JobGiver_DropUnusedInventory), "TryGiveJob"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DropUnusedInventory_PostFix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobDriver_HaulToCell), "MakeNewToils"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(JobDriver_HaulToCell_PostFix)));

            harmony.Patch(original: AccessTools.Method(typeof(Pawn_InventoryTracker), "Notify_ItemRemoved"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_InventoryTracker_PostFix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobGiver_DropUnusedInventory), "Drop"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Drop_Prefix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobGiver_Idle), "TryGiveJob"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(IdleJoy_Postfix)));

            harmony.Patch(original: AccessTools.Method(typeof(ITab_Pawn_Gear), "DrawThingRow"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(GearTabHighlightTranspiler)));

            Verse.Log.Message("PickUpAndHaul v0.1.0.5¼ welcomes you to RimWorld with pointless logspam.", true);
        }

        private static bool Drop_Prefix(Pawn pawn, Thing thing)
        {
            CompHauledToInventory takenToInventory = pawn.TryGetComp<CompHauledToInventory>();
            if (takenToInventory == null)
            {
                return true;
            }

            HashSet<Thing> carriedThing = takenToInventory.GetHashSet();

            return !carriedThing.Contains(thing);
        }

        private static void Pawn_InventoryTracker_PostFix(Pawn_InventoryTracker __instance, Thing item)
        {
            CompHauledToInventory takenToInventory = __instance.pawn.TryGetComp<CompHauledToInventory>();
            if (takenToInventory == null)
            {
                return;
            }

            HashSet<Thing> carriedThing = takenToInventory.GetHashSet();
            if (carriedThing?.Count > 0)
            {
                if (carriedThing.Contains(item))
                {
                    carriedThing.Remove(item);
                }
            }
        }

        private static void JobDriver_HaulToCell_PostFix(JobDriver_HaulToCell __instance)
        {
            CompHauledToInventory takenToInventory = __instance.pawn.TryGetComp<CompHauledToInventory>();
            if (takenToInventory == null)
            {
                return;
            }

            HashSet<Thing> carriedThing = takenToInventory.GetHashSet();

            if (__instance.job.haulMode == HaulMode.ToCellStorage
                && __instance.pawn.Faction == Faction.OfPlayer
                && __instance.pawn.RaceProps.Humanlike
                && __instance.pawn.carryTracker.CarriedThing is Corpse == false
                && carriedThing != null
                && carriedThing.Count != 0) //deliberate hauling job. Should unload.
            {
                PawnUnloadChecker.CheckIfPawnShouldUnloadInventory(__instance.pawn, true);
            }
        }

        public static void IdleJoy_Postfix(Pawn pawn)
        {
            PawnUnloadChecker.CheckIfPawnShouldUnloadInventory(pawn, true);
        }

        public static void DropUnusedInventory_PostFix(Pawn pawn)
        {
            PawnUnloadChecker.CheckIfPawnShouldUnloadInventory(pawn);
        }

        public static IEnumerable<CodeInstruction> FloatMenuMakerMad_AddHumanlikeOrders_Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            MethodInfo playerHome = AccessTools.Property(typeof(Map), nameof(Map.IsPlayerHome)).GetGetMethod();
            List<CodeInstruction> instructionList = instructions.ToList();

            bool patched = false;

            foreach (CodeInstruction instruction in instructionList)
            {
                if (!patched && instruction.operand == playerHome && !ModCompatibilityCheck.CombatExtendedIsActive)
                {
                    instruction.opcode = OpCodes.Ldc_I4_0;
                    instruction.operand = null;
                    yield return instruction;
                    patched = true;
                }
                yield return instruction;
            }
        }

        //ITab_Pawn_Gear
        //private void DrawThingRow(ref float y, float width, Thing thing, bool inventory = false)
        public static IEnumerable<CodeInstruction> GearTabHighlightTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase mb)
        {
            MethodInfo WidgetsButtonImageInfo = AccessTools.Method(typeof(Widgets), "ButtonImage", new Type[] { typeof(Rect), typeof(Texture2D) });
            MethodInfo WidgetsButtonImageColorInfo = AccessTools.Method(typeof(Widgets), "ButtonImage", new Type[] { typeof(Rect), typeof(Texture2D), typeof(Color) });

            MethodInfo SelPawnForGearInfo = AccessTools.Property(typeof(ITab_Pawn_Gear), "SelPawnForGear").GetGetMethod(true);

            MethodInfo GetColorForHauledInfo = AccessTools.Method(typeof(HarmonyPatches), nameof(GetColorForHauled));

            bool done = false;
            foreach (CodeInstruction i in instructions)
            {
                //if (Widgets.ButtonImage(rect2, TexButton.Drop))
                if (!done && i.opcode == OpCodes.Call && i.operand == WidgetsButtonImageInfo)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);                           //this
                    yield return new CodeInstruction(OpCodes.Call, SelPawnForGearInfo);          //this.SelPawnForGearInfo
                    yield return new CodeInstruction(OpCodes.Ldarg_3);                           //thing
                    yield return new CodeInstruction(OpCodes.Call, GetColorForHauledInfo);       //GetColorForHauledInfo(Pawn, Thing)
                    yield return new CodeInstruction(OpCodes.Call, WidgetsButtonImageColorInfo); //ButtonImage(rect, texture, color)
                    done = true;
                }
                else
                {
                    yield return i;
                }
            }
        }

        private static Color GetColorForHauled(Pawn pawn, Thing thing)
        {
            if (pawn.GetComp<CompHauledToInventory>()?.GetHashSet().Contains(thing) ?? false)
            {
                return Color.Lerp(Color.grey, Color.red, 0.5f);
            }

            return Color.white;
        }
    }
}