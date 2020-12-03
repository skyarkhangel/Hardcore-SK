using System;
using System.IO;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ExperimentalOptimizations
{
    public static class Listing_Standard_Extensions
    {
        private static string _editBuf;

        public static bool CheckBoxIsChanged(this Listing_Standard l, string label, ref bool value)
        {
            bool tmp = value;
            l.CheckboxLabeled(label, ref tmp);
            bool stateChanged = value != tmp;
            value = tmp;
            return stateChanged;
        }

        public static bool TextFieldNumericChanged(this Listing_Standard l, string label, ref int value, float min, float max)
        {
            int tmp = value;
            l.TextFieldNumericLabeled(label, ref tmp, ref _editBuf, min, max);
            bool stateChanged = value != tmp;
            value = tmp;
            return stateChanged;
        }
    }

    public class Settings : ModSettings
    {
        public static bool CheckModCompatible = true;

        public static void DoSettingsWindowContents(Rect rect)
        {
            var l = new Listing_Standard();
            l.Begin(rect);

            l.CheckboxLabeled("CheckModCompatible".Translate(), ref CheckModCompatible);

            foreach (var optimization in ExperimentalOptimizations.Optimizations)
            {
                var opt = optimization.TryGetAttribute<Optimization>();
                opt.optimizationSetting.InvokeStaticMethod("DoSettingsWindowContents", l);
            }

            if (l.ButtonText("DEBUG: Dump needs, hediffs subclasses"))
            {
                var sb = new StringBuilder();
                var allTypes = GenTypes.AllTypes.ToList();

                var hediffs = allTypes
                    .Where(t => t.IsSubclassOf(typeof(Hediff)))
                    .Where(t => TypeHasDeclaredMethod(t, "PostTick") || TypeHasDeclaredMethod(t, "Tick"))
                    .ToList();
                var hediffComps = allTypes
                    .Where(t => t.IsSubclassOf(typeof(HediffComp)))
                    .Where(t => TypeHasDeclaredMethod(t, "CompPostTick"))
                    .ToList();
                var needs = allTypes
                    .Where(t => t.IsSubclassOf(typeof(Need)))
                    .ToList();

                sb.AppendLine($"needs:");
                foreach (var need in needs)
                {
                    sb.AppendLine($"  {need.FullName}");
                }

                sb.AppendLine($"hediffs:");
                foreach (var hediff in hediffs)
                {
                    sb.AppendLine($"  {hediff.FullName}");
                }

                sb.AppendLine($"hediffComps:");
                foreach (var comp in hediffComps)
                {
                    sb.AppendLine($"  {comp.FullName}");
                }

                File.WriteAllText($"{GenFilePaths.FolderUnderSaveData("EOptimizations")}\\subclasses_debug.txt", sb.ToString());

                // local functions
                bool TypeHasDeclaredMethod(Type t, string methodName) => t
                    .GetMethods(AccessTools.all).Any(m => m.IsDeclaredMember() && m.Name.Equals(methodName));
            }

            l.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref CheckModCompatible, "CheckModCompatible", true);

            foreach (var optimization in ExperimentalOptimizations.Optimizations)
            {
                var opt = optimization.TryGetAttribute<Optimization>();
                opt.optimizationSetting.InvokeStaticMethod("ExposeData");
            }
        }
    }
}