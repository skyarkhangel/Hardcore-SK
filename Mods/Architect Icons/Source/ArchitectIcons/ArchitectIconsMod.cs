using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchitectIcons
{
    public class ArchitectIconsMod : Mod
    {
        public const float ADD_WIDTH = 32;
        private static readonly Dictionary<string, string> defNamesFromLabels = new Dictionary<string, string>();

        public ArchitectIconsMod(ModContentPack content) : base(content)
        {
            var harm = new Harmony("com.bymarcin.architecticons");
            harm.Patch(AccessTools.PropertyGetter(typeof(MainTabWindow_Architect), nameof(MainTabWindow_Architect.RequestedTabSize)),
                postfix: new HarmonyMethod(GetType(), nameof(AddWidth)));
            harm.Patch(AccessTools.Method(typeof(MainTabWindow_Architect), AccessTools.Method(typeof(MainTabWindow_Architect), "DoCategoryButton").Name),
                transpiler: new HarmonyMethod(GetType(), nameof(ReplaceArchitectButton)));
            harm.Patch(AccessTools.Method(typeof(ArchitectCategoryTab), nameof(ArchitectCategoryTab.DesignationTabOnGUI)),
                transpiler: new HarmonyMethod(GetType(), nameof(OffsetGizmos)));
        }

        public static void AddWidth(ref Vector2 __result) => __result.x += ADD_WIDTH;

        public static bool DoArchitectButton(Rect rect, string label, float barPercent = 0f, float textLeftMargin = -1f, SoundDef mouseoverSound = null,
            Vector2 functionalSizeOffset = default, Color? labelColor = null, bool highlight = false)
        {
            var result = Widgets.ButtonTextSubtle(rect, label, barPercent, textLeftMargin + 16f, mouseoverSound,
                functionalSizeOffset, labelColor, highlight);
            GUI.DrawTexture(new Rect(rect.position + new Vector2(4, 8), new Vector2(16, 16)),
                Resources.FindArchitectTabCategoryIcon(DefNameFromLabel(label)));
            return result;
        }

        public static string DefNameFromLabel(string label)
        {
            if (defNamesFromLabels.TryGetValue(label, out var defName)) return defName;
            defName = DefDatabase<DesignationCategoryDef>.AllDefs.First(def => def.LabelCap == label).defName;
            defNamesFromLabels[label] = defName;
            Logger.Debug(defName + "=>" + label);
            return defName;
        }

        public static IEnumerable<CodeInstruction> ReplaceArchitectButton(IEnumerable<CodeInstruction> instructions) =>
            instructions.MethodReplacer(AccessTools.Method(typeof(Widgets), nameof(Widgets.ButtonTextSubtle)),
                AccessTools.Method(typeof(ArchitectIconsMod), nameof(DoArchitectButton)));

        public static IEnumerable<CodeInstruction> OffsetGizmos(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == 210f)
                {
                    instruction.operand = (float) instruction.operand + ADD_WIDTH;
                }

                yield return instruction;
            }
        }
    }
}