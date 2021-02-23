using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.tick.stats", Category.Tick)]
    internal class H_GetStatValue
    {
        public static bool Active = false;

        [Setting("By Def")]
        public static bool ByDef = false;

        [Setting("Get Value Detour")]
        public static bool GetValDetour = false;

        public static void ProfilePatch()
        {
            MethodInfo jiff = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue));
            HarmonyMethod pre = new HarmonyMethod(typeof(H_GetStatValue), nameof(Prefix));
            HarmonyMethod post = new HarmonyMethod(typeof(H_GetStatValue), nameof(Postfix));
            Modbase.Harmony.Patch(jiff, pre, post);


            jiff = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValueAbstract), new[] { typeof(BuildableDef), typeof(StatDef), typeof(ThingDef) });
            pre = new HarmonyMethod(typeof(H_GetStatValue), nameof(PrefixAb));
            Modbase.Harmony.Patch(jiff, pre, post);

            jiff = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValueAbstract), new[] { typeof(AbilityDef), typeof(StatDef) });
            pre = new HarmonyMethod(typeof(H_GetStatValue), nameof(PrefixAbility));
            Modbase.Harmony.Patch(jiff, pre, post);

            jiff = AccessTools.Method(typeof(StatWorker), nameof(StatWorker.GetValue), new[] { typeof(StatRequest), typeof(bool) });
            pre = new HarmonyMethod(typeof(H_GetStatValue), nameof(GetValueDetour));
            Modbase.Harmony.Patch(jiff, pre);


            HarmonyMethod go = new HarmonyMethod(typeof(H_GetStatValue), nameof(PartPrefix));
            HarmonyMethod biff = new HarmonyMethod(typeof(H_GetStatValue), nameof(PartPostfix));

            foreach (Type allLeafSubclass in typeof(StatPart).AllSubclassesNonAbstract())
            {
                try
                {
                    MethodInfo mef = AccessTools.Method(allLeafSubclass, nameof(StatPart.TransformValue), new Type[] { typeof(StatRequest), typeof(float).MakeByRefType() });
                    if (mef.DeclaringType == allLeafSubclass)
                    {
                        Patches info = Harmony.GetPatchInfo(mef);
                        bool F = true;
                        if (info != null)
                        {
                            foreach (Patch infoPrefix in info.Prefixes)
                            {
                                if (infoPrefix.PatchMethod == go.method)
                                {
                                    F = false;
                                }
                            }
                        }

                        if (F)
                        {
                            Modbase.Harmony.Patch(mef, go, biff);
                        }
                    }
                }
                catch (Exception)
                {
                    ThreadSafeLogger.Error($"Failed to patch {allLeafSubclass} from {allLeafSubclass.Assembly.FullName} for profiling");
                }

            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void PartPrefix(object __instance, MethodBase __originalMethod, ref Profiler __state)
        {
            if (Active)
            {
                string state = string.Empty;
                if (__originalMethod.ReflectedType != null)
                {
                    state = __originalMethod.ReflectedType.ToString();
                }
                else
                {
                    state = __originalMethod.GetType().ToString();
                }

                __state = ProfileController.Start(state, null, null, null, null, __originalMethod);
            }
        }

        [HarmonyPriority(Priority.First)]
        public static void PartPostfix(Profiler __state)
        {
            if (Active)
            {
                __state.Stop();
            }
        }

        public static bool GetValueDetour(MethodBase __originalMethod, StatWorker __instance, StatRequest req, ref float __result, bool applyPostProcess = true)
        {
            if (Active && GetValDetour && __instance is StatWorker sw)
            {
                if (sw.stat.minifiedThingInherits)
                {
                    if (req.Thing is MinifiedThing minifiedThing)
                    {
                        if (minifiedThing.InnerThing == null)
                        {
                            Log.Error("MinifiedThing's inner thing is null.");
                        }
                        __result = minifiedThing.InnerThing.GetStatValue(sw.stat, applyPostProcess);
                        return false;
                    }
                }

                string slag = "";
                if (ByDef)
                {
                    slag = $"{__instance.stat.defName} GetValueUnfinalized for {req.Def.defName}";
                }
                else
                {
                    slag = $"{__instance.stat.defName} GetValueUnfinalized";
                }

                Profiler prof = ProfileController.Start(slag, null, null, null, null, __originalMethod);
                float valueUnfinalized = sw.GetValueUnfinalized(req, applyPostProcess);
                prof.Stop();

                if (ByDef)
                {
                    slag = $"{__instance.stat.defName} FinalizeValue for {req.Def.defName}";
                }
                else
                {
                    slag = $"{__instance.stat.defName} FinalizeValue";
                }

                prof = ProfileController.Start(slag, null, null, null, null, __originalMethod);
                sw.FinalizeValue(req, ref valueUnfinalized, applyPostProcess);
                prof.Stop();

                __result = valueUnfinalized;
                return false;
            }
            return true;
        }

        [HarmonyPriority(Priority.Last)]
        public static void Prefix(MethodBase __originalMethod, Thing thing, StatDef stat, ref Profiler __state)
        {
            if (Active && !GetValDetour)
            {
                string state = string.Empty;
                if (ByDef)
                {
                    state = $"{stat.defName} for {thing.def.defName}";
                }
                else
                {
                    state = stat.defName;
                }

                __state = ProfileController.Start(state, null, null, null, null, __originalMethod);
            }
        }

        [HarmonyPriority(Priority.First)]
        public static void Postfix(Profiler __state)
        {
            if (Active && !GetValDetour)
            {
                __state.Stop();
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void PrefixAb(MethodBase __originalMethod, BuildableDef def, StatDef stat, ref Profiler __state)
        {

            if (Active && !GetValDetour)
            {
                string state = string.Empty;
                if (ByDef)
                {
                    state = $"{stat.defName} abstract for {def.defName}";
                }
                else
                {
                    state = $"{stat.defName} abstract";
                }

                __state = ProfileController.Start(state, null, null, null, null, __originalMethod);
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void PrefixAbility(MethodBase __originalMethod, AbilityDef def, StatDef stat, ref Profiler __state)
        {

            if (Active && !GetValDetour)
            {
                string state = string.Empty;
                if (ByDef)
                {
                    state = $"{stat.defName} abstract for {def.defName}";
                }
                else
                {
                    state = $"{stat.defName} abstract";
                }

                __state = ProfileController.Start(state, null, null, null, null, __originalMethod);
            }
        }

    }
}