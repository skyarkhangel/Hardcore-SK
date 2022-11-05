using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RocketMan
{

    [RocketPatch(typeof(DebugWindowsOpener), methodType: MethodType.Constructor)]
    [StaticConstructorOnStartup]
    public static class DebugWindowsOpener_Patch
    {
        public static Action drawButtonsCached;

        public static FieldInfo mDrawButtonCache = AccessTools.Field(typeof(DebugWindowsOpener_Patch), nameof(DebugWindowsOpener_Patch.drawButtonsCached));

        static DebugWindowsOpener_Patch()
        {
            Finder.Harmony.Patch(AccessTools.Constructor(typeof(DebugWindowsOpener)), postfix: new HarmonyMethod(AccessTools.Method(typeof(DebugWindowsOpener_Patch), nameof(DebugWindowsOpener_Patch.Postfix))));

            Finder.Harmony.Patch(AccessTools.Method(typeof(DebugWindowsOpener), nameof(DebugWindowsOpener.DevToolStarterOnGUI)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(DebugWindowsOpener_Patch), nameof(DebugWindowsOpener_Patch.Transpiler))));
        }

        public static void Postfix(DebugWindowsOpener __instance)
        {
            drawButtonsCached = () =>
            {
                __instance.DrawButtons();

                if (__instance.widgetRow.ButtonIcon(ContentFinder<Texture2D>.Get("RocketMan/UI/rocketman_time_dilation_debug_button", true), "Simulate offscreen behavior and flash dilated/throttled pawns."))
                {
                    RocketDebugPrefs.AlwaysDilating = !RocketDebugPrefs.AlwaysDilating;
                    RocketDebugPrefs.FlashDilatedPawns = !RocketDebugPrefs.FlashDilatedPawns;
                }
                if (__instance.widgetRow.ButtonIcon(ContentFinder<Texture2D>.Get("RocketMan/UI/rocketman_debug_options_button", true), "Open " + "<color=orange>RocketMans</color> hidden debug options."))
                {
                    if (Find.WindowStack.WindowOfType<Window_HiddenDebugMenu>() != null)
                    {
                        Find.WindowStack.RemoveWindowsOfType(typeof(Window_HiddenDebugMenu));
                    }
                    else
                    {
                        Find.WindowStack.Add(new Window_HiddenDebugMenu());
                    }
                }
            };
            __instance.drawButtonsCached = drawButtonsCached;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;
            foreach (CodeInstruction code in instructions)
            {
                if (!patched && code.opcode == OpCodes.Ldc_R4 && 28f.Equals(code.operand))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 2f);
                    yield return new CodeInstruction(OpCodes.Add);
                    patched = true;
                }
                if (code.opcode == OpCodes.Ldfld && code.OperandIs(mDrawButtonCache))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, mDrawButtonCache) { labels = code.labels };
                    continue;
                }
                yield return code;
            }
        }
    }
}
