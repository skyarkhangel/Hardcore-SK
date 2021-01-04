using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using Verse;

namespace Analyzer.Profiling
{
    public static class MethodTransplanting
    {
        public static HashSet<MethodInfo> patchedMeths = new HashSet<MethodInfo>();
        public static ConcurrentDictionary<MethodBase, Type> typeInfo = new ConcurrentDictionary<MethodBase, Type>();

        private static readonly HarmonyMethod transpiler = new HarmonyMethod(typeof(MethodTransplanting), nameof(MethodTransplanting.Transpiler));

        // profile controller
        private static readonly MethodInfo ProfileController_Start = AccessTools.Method(typeof(ProfileController), nameof(ProfileController.Start));
        private static readonly FieldInfo ProfilerController_Profiles = AccessTools.Field(typeof(ProfileController), "profiles");

        // profiler
        private static readonly MethodInfo Profiler_Start = AccessTools.Method(typeof(Profiler), nameof(Profiler.Start));
        private static readonly MethodInfo Profiler_Stop = AccessTools.Method(typeof(Profiler), nameof(Profiler.Stop));
        private static readonly ConstructorInfo ProfilerCtor = AccessTools.Constructor(typeof(Profiler), new Type[] { typeof(string), typeof(string), typeof(Type), typeof(Def), typeof(Thing), typeof(MethodBase) });

        // analyzer
        private static readonly MethodInfo Analyzer_Get_CurrentlyProfiling = AccessTools.Method(typeof(Analyzer), "get_CurrentlyProfiling");
        private static readonly FieldInfo Analyzer_CurrentlyProfiling = AccessTools.Field(typeof(Analyzer), "currentlyProfiling");
        private static readonly FieldInfo Analyzer_CurrentyPaused = AccessTools.Field(typeof(Analyzer), "currentlyPaused"); 

        // dictionary
        private static readonly MethodInfo Dict_Get_Value = AccessTools.Method(typeof(Dictionary<string, MethodInfo>), "get_Item");
        private static readonly MethodInfo Dict_TryGetValue = AccessTools.Method(typeof(Dictionary<string, Profiler>), "TryGetValue");
        private static readonly MethodInfo Dict_Add = AccessTools.Method(typeof(Dictionary<string, Profiler>), "Add");


        public static void ClearCaches()
        {
            patchedMeths.Clear();
            typeInfo.Clear();
        }

        public static void PatchMethods(Type type)
        {
            // get the methods
            var meths = (IEnumerable<MethodInfo>)type.GetMethod("GetPatchMethods", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);

            if (meths != null)
                UpdateMethods(type, meths);
        }

        public static void UpdateMethods(Type type, IEnumerable<MethodInfo> meths)
        {
            foreach (var meth in meths) UpdateMethod(type, meth);
        }
            
        internal static void UpdateMethod(Type type, MethodInfo meth)
        {
            if (patchedMeths.Contains(meth))
            {
#if DEBUG
                ThreadSafeLogger.Warning($"[Analyzer] Already patched method {meth.DeclaringType.FullName + ":" + meth.Name}");
#else
                if (Settings.verboseLogging)
                    ThreadSafeLogger.Warning($"[Analyzer] Already patched method {meth.DeclaringType.FullName + ":" + meth.Name}");
#endif
                return;
            }

            patchedMeths.Add(meth);
            typeInfo.TryAdd(meth, type);

            Task.Factory.StartNew(delegate
            {
                try
                {
                    Modbase.Harmony.Patch(meth, transpiler: transpiler);
                }
                catch (Exception e)
                {
#if DEBUG
                    ThreadSafeLogger.Error($"[Analyzer] Failed to patch method {meth.DeclaringType.FullName + ":" + meth.Name} failed with the error {e.Message}");
#else
                    if (Settings.verboseLogging)
                        ThreadSafeLogger.Warning($"[Analyzer] Failed to patch method {meth.DeclaringType.FullName}:{meth.Name} failed with the error {e.Message}");
#endif
                }
            });
        }

        // This transpiler basically replicates ProfileController.Start, but in IL, and inside the method it is patching, to reduce as much overhead as
        // possible, its quite simple, just long and hard to read.
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase __originalMethod, IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            var profLocal = ilGen.DeclareLocal(typeof(Profiler));
            var keyLocal = ilGen.DeclareLocal(typeof(string));
            var beginLabel = ilGen.DefineLabel();
            var noProfLabel = ilGen.DefineLabel();  

            var curType = typeInfo[__originalMethod];
            var curLabelMeth = curType.GetMethod("GetLabel", BindingFlags.Public | BindingFlags.Static);
            var curNamerMeth = curType.GetMethod("GetName", BindingFlags.Public | BindingFlags.Static);
            var curTypeMeth = curType.GetMethod("GetType", BindingFlags.Public | BindingFlags.Static);


            var key = Utility.GetMethodKey(__originalMethod as MethodInfo); // This translates our method into a human-legible key, I.e. Namespace.Type<Generic>:Method
            var methodKey = MethodInfoCache.AddMethod(key, __originalMethod as MethodInfo);


            // Active Check
            {
                // if(active && (Analyzer.CurrentlyProfiling))
                yield return new CodeInstruction(OpCodes.Ldsfld, curType.GetField("Active", BindingFlags.Public | BindingFlags.Static));

                yield return new CodeInstruction(OpCodes.Ldsfld, Analyzer_CurrentlyProfiling); // profiling
                yield return new CodeInstruction(OpCodes.Ldsfld, Analyzer_CurrentyPaused); // !paused
                yield return new CodeInstruction(OpCodes.Not);
                yield return new CodeInstruction(OpCodes.And);

                yield return new CodeInstruction(OpCodes.And);
                yield return new CodeInstruction(OpCodes.Brfalse_S, beginLabel);
            }


            { // Custom Namer
                if (curNamerMeth != null)
                {
                    foreach (var codeInst in GetLoadArgsForMethodParams(__originalMethod as MethodInfo, curNamerMeth.GetParameters()))
                        yield return codeInst;

                    yield return new CodeInstruction(OpCodes.Call, curNamerMeth);
                }
                else
                {
                    yield return new CodeInstruction(OpCodes.Ldstr, key);
                }
                yield return new CodeInstruction(OpCodes.Stloc, keyLocal);
            }

            { // if(Profilers.TryGetValue(key, out var prof))
                yield return new CodeInstruction(OpCodes.Ldsfld, ProfilerController_Profiles);
                yield return new CodeInstruction(OpCodes.Ldloc, keyLocal);
                yield return new CodeInstruction(OpCodes.Ldloca_S, profLocal);
                yield return new CodeInstruction(OpCodes.Callvirt, Dict_TryGetValue);
                yield return new CodeInstruction(OpCodes.Brfalse_S, noProfLabel);
            }

            { // If we found a profiler - Start it, and skip to the start of execution of the method
                yield return new CodeInstruction(OpCodes.Ldloc, profLocal);
                yield return new CodeInstruction(OpCodes.Call, Profiler_Start);
                yield return new CodeInstruction(OpCodes.Pop); // Profiler.Start returns itself so we pop it off the stack
                yield return new CodeInstruction(OpCodes.Br, beginLabel);
            }

            { // if not, we need to make one
                yield return new CodeInstruction(OpCodes.Ldloc, keyLocal).WithLabels(noProfLabel);

                { // Custom Labelling
                    if (curLabelMeth != null)
                    {
                        foreach (var codeInst in GetLoadArgsForMethodParams(__originalMethod as MethodInfo, curLabelMeth.GetParameters()))
                            yield return codeInst;

                        yield return new CodeInstruction(OpCodes.Call, curLabelMeth);
                    }
                    else
                    {
                        yield return new CodeInstruction(OpCodes.Dup); // duplicate the key on the stack so the key is both the key and the label in ProfileController.Start
                    }
                }
                { // Custom Typing
                    if (curTypeMeth != null)
                    {
                        foreach (var codeInst in GetLoadArgsForMethodParams(__originalMethod as MethodInfo, curTypeMeth.GetParameters()))
                            yield return codeInst;

                        yield return new CodeInstruction(OpCodes.Call, curTypeMeth);
                    }
                    else
                    {
                        yield return new CodeInstruction(OpCodes.Ldnull); // duplicate the key on the stack so the key is both the key and the label in ProfileController.Start
                    }
                }

                yield return new CodeInstruction(OpCodes.Ldnull);
                yield return new CodeInstruction(OpCodes.Ldnull);

                { // get our methodinfo from the metadata
                    foreach (var inst in MethodInfoCache.GetInlineIL(methodKey))
                        yield return inst;
                }

                yield return new CodeInstruction(OpCodes.Newobj, ProfilerCtor); // new Profiler();
                yield return new CodeInstruction(OpCodes.Dup);
                yield return new CodeInstruction(OpCodes.Stloc, profLocal);
            }

            yield return new CodeInstruction(OpCodes.Call, Profiler_Start);
            yield return new CodeInstruction(OpCodes.Pop); // profiler.Start returns itself

            { // Add to the Profilers dictionary, so we cache creation.
                yield return new CodeInstruction(OpCodes.Ldsfld, ProfilerController_Profiles);
                yield return new CodeInstruction(OpCodes.Ldloc, keyLocal);
                yield return new CodeInstruction(OpCodes.Ldloc, profLocal);
                yield return new CodeInstruction(OpCodes.Callvirt, Dict_Add);
            }

            instructions.ElementAt(0).WithLabels(beginLabel);

            // For each instruction which exits this function, append our finishing touches (I.e.)
            // if(profiler != null)
            // {
            //      profiler.Stop();
            // }
            // return; // any labels here are moved to the start of the `if`
            foreach (var inst in instructions)
            {
                if (inst.opcode == OpCodes.Ret)
                {
                    Label endLabel = ilGen.DefineLabel();

                    // localProf?.Stop();
                    yield return new CodeInstruction(OpCodes.Ldloc, profLocal).MoveLabelsFrom(inst);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, endLabel);

                    yield return new CodeInstruction(OpCodes.Ldloc, profLocal);
                    yield return new CodeInstruction(OpCodes.Call, Profiler_Stop);

                    yield return inst.WithLabels(endLabel);
                }
                else
                {
                    yield return inst;
                }
            }
        }

        // Emulates Harmonys '__instance' & '___fieldName' & parameter sniping.
        private static List<CodeInstruction> GetLoadArgsForMethodParams(MethodInfo originalMethod, ParameterInfo[] methodparams)
        {
            var origParams = originalMethod.GetParameters();
            var origType = originalMethod.GetType();
            var insts = new List<CodeInstruction>();

            foreach (var param in methodparams)
            {
                if (param.Name == "__instance") // Trying to get the instance of the object (assumed to be non static)
                {
                    insts.Add(new CodeInstruction(OpCodes.Ldarg_0)); // Push the instance of the object on the stack (up to the user to get this right)
                }
                else if (param.Name.StartsWith("___")) // Trying to get a field from the object (static or non static)
                {
                    var fieldName = param.Name.Remove(0, 3);

                    var fieldInfo = AccessTools.Field(origType, fieldName);

                    if (fieldInfo.IsStatic)
                    {
                        insts.Add(new CodeInstruction(OpCodes.Ldsfld, fieldInfo));
                    }
                    else
                    {
                        insts.Add(new CodeInstruction(OpCodes.Ldarg_0)); // push the instance onto the stack, then grab the field from the instance.
                        insts.Add(new CodeInstruction(OpCodes.Ldfld, fieldInfo));
                    }
                }
                else // Trying to intercept a param from the original method
                {
                    insts.Add(new CodeInstruction(OpCodes.Ldarg, origParams.FirstIndexOf(p => p.Name == param.Name && p.ParameterType == param.ParameterType)));
                }
            }
            return insts;
        }


        // Utility for internal && transpiler profiling.
        // This method takes a codeinstruction (of type Call or CallVirt), a key, a type, and a fieldinfo of a dictionary
        // and will return a new codeinstruction, which the same opcode as the instruction passed to it, and the method
        // will be a new dynamic method, which duplicates the functionality of the original method, while adding proiling to it.
        // 
        // The key is used for keying into the dictionary field you give, which will be expected to return a MethodInfo
        // this will be then used in the call to ProfileController.Start
        public static CodeInstruction ReplaceMethodInstruction(CodeInstruction inst, string key, Type type, int index)
        {
            MethodInfo method = null;
            try { method = (MethodInfo)inst.operand; } catch (Exception) { return inst; }

            Type[] parameters = null;


            if (method.Attributes.HasFlag(MethodAttributes.Static)) // If we have a static method, we don't need to grab the instance
                parameters = method.GetParameters().Select(param => param.ParameterType).ToArray();
            else if (method.DeclaringType.IsValueType) // if we have a struct, we need to make the struct a ref, otherwise you resort to black magic
                parameters = method.GetParameters().Select(param => param.ParameterType).Prepend(method.DeclaringType.MakeByRefType()).ToArray();
            else // otherwise, we have an instance-nonstruct class, lets all our instance, and our parameter types
                parameters = method.GetParameters().Select(param => param.ParameterType).Prepend(method.DeclaringType).ToArray();

            DynamicMethod meth = new DynamicMethod(
                method.Name + "_runtimeReplacement",
                MethodAttributes.Public,
                method.CallingConvention,
                method.ReturnType,
                parameters,
                method.DeclaringType.IsInterface ? typeof(void) : method.DeclaringType,
                true
                );

            ILGenerator gen = meth.GetILGenerator(512);

            // local variable for profiler
            LocalBuilder localProfiler = gen.DeclareLocal(typeof(Profiler));

            InsertStartIL(type, gen, key, localProfiler, index);

            // dynamically add our parameters, as many as they are, onto the stack for our original method
            for (int i = 0; i < parameters.Length; i++)
                gen.Emit(OpCodes.Ldarg, i);

            gen.Emit(inst.opcode, method); // call our original method, (all parameters are on the stack)

            InsertRetIL(type, gen, localProfiler); // wrap our function up, return a value if required

            return new CodeInstruction(inst)
            {
                opcode = OpCodes.Call,
                operand = meth // our created dynamic method
            };
        }

        // Utility for IL insertion
        public static void InsertStartIL(Type type, ILGenerator ilGen, string key, LocalBuilder profiler, int index)
        {
            // if(Active && AnalyzerState.CurrentlyRunning)
            // { 
            Label skipLabel = ilGen.DefineLabel();

            ilGen.Emit(OpCodes.Ldsfld, type.GetField("Active", BindingFlags.Public | BindingFlags.Static));
            ilGen.Emit(OpCodes.Brfalse_S, skipLabel);

            ilGen.Emit(OpCodes.Call, Analyzer_Get_CurrentlyProfiling);
            ilGen.Emit(OpCodes.Brfalse_S, skipLabel);

            ilGen.Emit(OpCodes.Ldstr, key);
            // load our string to stack

            ilGen.Emit(OpCodes.Ldnull);
            ilGen.Emit(OpCodes.Ldnull);
            ilGen.Emit(OpCodes.Ldnull);
            ilGen.Emit(OpCodes.Ldnull);
            // load our null variables

            ilGen.Emit(OpCodes.Ldsfld, MethodInfoCache.internalArray);
            ilGen.Emit(OpCodes.Ldc_I4, index);
            ilGen.Emit(OpCodes.Ldc_I4_7);
            ilGen.Emit(OpCodes.Shr);
            ilGen.Emit(OpCodes.Callvirt, MethodInfoCache.accessList);
            ilGen.Emit(OpCodes.Ldc_I4, index);
            ilGen.Emit(OpCodes.Ldc_I4, 127);
            ilGen.Emit(OpCodes.And);
            ilGen.Emit(OpCodes.Ldelem_Ref);

            ilGen.Emit(OpCodes.Call, ProfileController_Start);
            ilGen.Emit(OpCodes.Stloc, profiler.LocalIndex);
            // localProfiler = ProfileController.Start(key, null, null, null, null, KeyMethods[key]);

            ilGen.MarkLabel(skipLabel);
        }

        public static void InsertRetIL(Type type, ILGenerator ilGen, LocalBuilder profiler)
        {
            Label skipLabel = ilGen.DefineLabel();
            // if(profiler != null)
            // {
            ilGen.Emit(OpCodes.Ldloc, profiler);
            ilGen.Emit(OpCodes.Brfalse_S, skipLabel);
            // profiler.Stop();
            ilGen.Emit(OpCodes.Ldloc, profiler.LocalIndex);
            ilGen.Emit(OpCodes.Call, Profiler_Stop);
            // }
            ilGen.MarkLabel(skipLabel);

            ilGen.Emit(OpCodes.Ret);
        }
    }
}
