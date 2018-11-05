using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Verse;
using RimWorld;
using UnityEngine;

namespace PickUpAndHaul
{
    //ITab_Pawn_Gear
    //private void DrawThingRow(ref float y, float width, Thing thing, bool inventory = false)
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "DrawThingRow")]
    public static class DrawThingRow_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase mb)
        {
            MethodInfo WidgetsButtonImageInfo = AccessTools.Method(typeof(Widgets), "ButtonImage", new Type[] { typeof(Rect), typeof(Texture2D) });
            MethodInfo WidgetsButtonImageColorInfo = AccessTools.Method(typeof(Widgets), "ButtonImage", new Type[] { typeof(Rect), typeof(Texture2D), typeof(Color)});

            MethodInfo SelPawnForGearInfo = AccessTools.Property(typeof(ITab_Pawn_Gear), "SelPawnForGear").GetGetMethod(true);

            MethodInfo GetColorForHauledInfo = AccessTools.Method(typeof(DrawThingRow_Patch), "GetColorForHauled");

            bool done = false;
            foreach (CodeInstruction i in instructions)
            {
                //if (Widgets.ButtonImage(rect2, TexButton.Drop))
                if (!done && i.opcode == OpCodes.Call && i.operand == WidgetsButtonImageInfo)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);//this
                    yield return new CodeInstruction(OpCodes.Call, SelPawnForGearInfo);//this.SelPawnForGearInfo
                    yield return new CodeInstruction(OpCodes.Ldarg_3);//thing
                    yield return new CodeInstruction(OpCodes.Call, GetColorForHauledInfo);//GetColorForHauledInfo(Pawn, Thing)
                    yield return new CodeInstruction(OpCodes.Call, WidgetsButtonImageColorInfo);//ButtonImage(rect, texture, color)
                }
                else 
                    yield return i;
            }
        }

        public static Color GetColorForHauled(Pawn pawn, Thing thing)
        {
            CompHauledToInventory comp = pawn.GetComp<CompHauledToInventory>();
            if (comp.GetHashSet().Contains(thing))
                return Color.Lerp(Color.grey, Color.red, 0.5f);
            return Color.white;
        }
    }
}
