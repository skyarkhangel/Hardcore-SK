using System;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Verse;
using Mono.Cecil.Cil;

namespace RocketMan.Patches
{
    //public class ListerHaulables_Patch
    //{
    //    static MethodInfo mAdd = AccessTools.Method(typeof(List<Thing>), nameof(List<Thing>.Add));
    //    static MethodInfo mRemove = AccessTools.Method(typeof(List<Thing>), nameof(List<Thing>.Remove));
    //    static MethodInfo mContains = AccessTools.Method(typeof(List<Thing>), nameof(List<Thing>.Contains));

    //    static List<Thing> _thingsList;
    //    static Map _thingsMap;

    //    static List<Pair<System.WeakReference<Map>, HashSet<Thing>>> _stores = new List<Pair<System.WeakReference<Map>, HashSet<Thing>>>();
    //    static HashSet<Thing> _cachedStore;
    //    static Map _cachedMap;

    //    static HashSet<Thing> GetIndices(Map map)
    //    {
    //        if (_cachedMap == map)
    //        {                
    //            return _cachedStore;
    //        }
    //        _cachedStore = null;
    //        for (int i = 0;i < _stores.Count; i++)
    //        {
    //            Pair<System.WeakReference<Map>, HashSet<Thing>> store = _stores[i];
    //            if (!store.First.TryGetTarget(out Map m))
    //            {
    //                _stores.RemoveAt(i--);                    
    //                continue;
    //            }
    //            else if (m == map)
    //            {
    //                _cachedStore = store.Second;
    //                break;
    //            }
    //        }
    //        if (_cachedStore == null)
    //        {
    //            _cachedStore = new HashSet<Thing>();
    //            _stores.Add(new Pair<System.WeakReference<Map>, HashSet<Thing>>(new System.WeakReference<Map>(map), _cachedStore));
    //        }
    //        _cachedMap = map;
    //        return _cachedStore;
    //    }        

    //    static void Add(ListerHaulables lister, Thing thing)
    //    {
    //        _thingsMap = null;
    //        _thingsList = null;
    //        GetIndices(lister.map).Add(thing);            
    //    }

    //    static void Remove(ListerHaulables lister, Thing thing)
    //    {
    //        _thingsMap = null;
    //        _thingsList = null;
    //        GetIndices(lister.map).Remove(thing);            
    //    }

    //    static bool Contains(ListerHaulables lister, Thing thing)
    //    {
    //        return GetIndices(lister.map).Contains(thing);
    //    }        

    //    static IEnumerable<CodeInstruction> ApplyAddPatch(IEnumerable<CodeInstruction> instructions)
    //    {
    //        foreach(CodeInstruction instruction in instructions)
    //        {
    //            yield return instruction;
    //            if (instruction.opcode == OpCodes.Callvirt && instruction.OperandIs(mAdd))
    //            {
    //                yield return new CodeInstruction(OpCodes.Ldarg_0);
    //                yield return new CodeInstruction(OpCodes.Ldarg_1);
    //                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ListerHaulables_Patch), nameof(ListerHaulables_Patch.Add)));
    //            }
    //        }
    //    }

    //    static IEnumerable<CodeInstruction> ApplyRemovePatch(IEnumerable<CodeInstruction> instructions)
    //    {
    //        foreach (CodeInstruction instruction in instructions)
    //        {
    //            yield return instruction;
    //            if (instruction.opcode == OpCodes.Callvirt && instruction.OperandIs(mRemove))
    //            {
    //                yield return new CodeInstruction(OpCodes.Ldarg_0);
    //                yield return new CodeInstruction(OpCodes.Ldarg_1);
    //                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ListerHaulables_Patch), nameof(ListerHaulables_Patch.Remove)));
    //            }
    //        }
    //    }

    //    static IEnumerable<CodeInstruction> ApplyContainsPatch(IEnumerable<CodeInstruction> instructions)
    //    {
    //        List<CodeInstruction> codes = instructions.ToList();
    //        bool finished = false;
    //        for(int i = 0;i < codes.Count; i++)
    //        {
    //            if (!finished)
    //            {
    //                if (codes[i + 2].opcode == OpCodes.Callvirt && codes[i + 2].OperandIs(mContains))
    //                {
    //                    finished = true;
    //                    i += 2;
    //                    yield return new CodeInstruction(OpCodes.Ldarg_1);
    //                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ListerHaulables_Patch), nameof(ListerHaulables_Patch.Contains)));
    //                    continue;
    //                }
    //            }
    //            yield return codes[i];
    //        }            
    //    }

    //    [RocketPatch(typeof(ListerHaulables), nameof(ListerHaulables.ThingsPotentiallyNeedingHauling))]
    //    public class ListerHaulables_ThingsPotentiallyNeedingHauling_Patch
    //    {
    //        public static bool Prefix(ListerHaulables __instance, out List<Thing> __result)
    //        {
    //            if(_thingsMap != __instance.map)
    //            {
    //                _thingsMap = __instance.map;
    //                _thingsList = GetIndices(__instance.map).ToList();
    //            }
    //            __result = _thingsList;
    //            return false;
    //        }
    //    }

    //    [RocketPatch(typeof(ListerHaulables), nameof(ListerHaulables.Check))]
    //    public class ListerHaulables_Check_Patch
    //    {
    //        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //        {
    //            return ApplyRemovePatch(ApplyContainsPatch(ApplyAddPatch(instructions)));
    //        }
    //    }

    //    [HarmonyPatch(typeof(ListerHaulables), nameof(ListerHaulables.CheckAdd))]
    //    public class ListerHaulables_CheckAdd_Patch
    //    {
    //        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //        {
    //            return ApplyContainsPatch(ApplyAddPatch(instructions));
    //        }
    //    }

    //    [HarmonyPatch(typeof(ListerHaulables), nameof(ListerHaulables.TryRemove))]
    //    public class ListerHaulables_TryRemove_Patch
    //    {
    //        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //        {
    //            return ApplyRemovePatch(ApplyContainsPatch(instructions));
    //        }
    //    }
    //}
}

