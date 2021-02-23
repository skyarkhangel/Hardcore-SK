using Analyzer;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Analyzer.Profiling
{
    public static class DynamicTypeBuilder
    {
        private static readonly TypeAttributes staticAtt = TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;
        // this is what a static class looks like in attributes

        private static AssemblyBuilder assembly = null;
        private static AssemblyBuilder Assembly => assembly ??= AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("DubsDynamicTypes"), AssemblyBuilderAccess.RunAndSave);

        private static ModuleBuilder moduleBuilder = null;
        private static ModuleBuilder ModuleBuilder => moduleBuilder ??= Assembly.DefineDynamicModule("DubsDynamicTypes", "DubsDynamicTypes.dll");

        // Do NOT clear this cache, it holds information for all dynamically created types (including persistent ones!)
        public static Dictionary<string, HashSet<MethodInfo>> methods = new Dictionary<string, HashSet<MethodInfo>>();


        /* CreateType creates a type at runtime which inherits from 'Entry', it creates a type with one field and two methods
         * Field: 'Active' signifying whether the entry is currently active or not 
         * Ctor: Sets 'Active' to false.
         * Method: 'GetPatchMethods' given a string, finds a list of methods in a dictionary which represent the methods this type is profiling
         * they are stored in the `methods` dictionary, and added to in the CreateType method. 
         */

        public static Type CreateType(string input, HashSet<MethodInfo> methods)
        {
            var name = string.Concat(input.Where(x => char.IsLetter(x) && !char.IsSymbol(x) && !char.IsWhiteSpace(x)));

#if DEBUG
            ThreadSafeLogger.Message($"[Analyzer] Converted the parameter called {input} into {name}, creating type");
#endif

            ThreadSafeLogger.Message(name);
            TypeBuilder tb = ModuleBuilder.DefineType(name, staticAtt, typeof(Entry));

            FieldBuilder active = tb.DefineField("Active", typeof(bool), FieldAttributes.Public | FieldAttributes.Static);

            DynamicTypeBuilder.methods.Add(name, methods ?? new HashSet<MethodInfo>());
            ConstructorBuilder ivCtor = tb.DefineConstructor(MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, new Type[0]);

            // default initialise active to false.
            ILGenerator ctorIL = ivCtor.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldc_I4_0);
            ctorIL.Emit(OpCodes.Stsfld, active);
            ctorIL.Emit(OpCodes.Ret);

            CreateProfilePatch(name, tb);

            return tb.CreateType();
        }

        // We already have our methods defined inside a dictionary, all we need to do is index into it at runtime
        private static void CreateProfilePatch(string name, TypeBuilder tb)
        {
            MethodInfo func = AccessTools.Method(typeof(DynamicTypeBuilder), "PatchAll");

            MethodBuilder ProfilePatch = tb.DefineMethod("GetPatchMethods", MethodAttributes.Public | MethodAttributes.Static, typeof(IEnumerable<MethodInfo>), null);

            ILGenerator generator = ProfilePatch.GetILGenerator();

            generator.Emit(OpCodes.Ldstr, name);
            generator.Emit(OpCodes.Call, func);
            generator.Emit(OpCodes.Ret);
        }

        // Does the one off work in a method, not bothered for performance so keeping it simple.
        public static IEnumerable<MethodInfo> PatchAll(string name)
        {
            if (methods.TryGetValue(name, out var meths))
            {
                foreach (MethodInfo meth in meths)
                    yield return meth;
            }
        }

    }

}
