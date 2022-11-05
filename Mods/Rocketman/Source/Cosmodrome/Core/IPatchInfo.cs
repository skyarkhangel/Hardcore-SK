using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace RocketMan
{
    public abstract class IPatchInfo<T> where T : IPatch
    {
        private bool patchedSuccessfully;

        private T attribute;

        private MethodBase[] targets;

        private Type declaringType;

        private MethodInfo prefix;

        private MethodInfo postfix;

        private MethodInfo transpiler;

        private MethodInfo finalizer;

        private MethodBase prepare;

        private MethodBase replacement;

        private PatchType patchType;

        public abstract string PluginName { get; }

        public abstract string PatchTypeUniqueIdentifier { get; }

        public bool IsValid => attribute != null && targets.Any(t => t != null);

        public bool PatchedSuccessfully
        {
            get => patchedSuccessfully;
        }

        public MethodBase ReplacementMethod
        {
            get => replacement;
        }

        public Type DeclaringType
        {
            get => declaringType;
        }

        public IPatchInfo(Type type)
        {
            declaringType = type;
            attribute = type.TryGetAttribute<T>();
            patchType = attribute.patchType;
            try
            {
                prepare = AccessTools.Method(
                    declaringType, "Prepare");

                prefix = AccessTools.Method(
                    declaringType, "Prefix");

                postfix = AccessTools.Method(
                    declaringType, "Postfix");

                transpiler = AccessTools.Method(
                    declaringType, "Transpiler");

                finalizer = AccessTools.Method(
                    declaringType, "Finalizer");

                if (prepare == null && prefix == null && postfix == null && transpiler == null && finalizer == null)
                {
                    throw new Exception($"All non of the methods in {declaringType} match the required ones for patching!");
                }
                if (patchType == PatchType.normal)
                {
                    if (attribute.methodType == MethodType.Getter)
                    {
                        targets = new MethodBase[1]
                            {
                                AccessTools.PropertyGetter(attribute.targetType, attribute.targetMethod)
                            };
                    }
                    else if (attribute.methodType == MethodType.Setter)
                    {
                        targets = new MethodBase[1]
                            {
                                AccessTools.PropertySetter(attribute.targetType, attribute.targetMethod)
                            };
                    }
                    else if (attribute.methodType == MethodType.Normal)
                    {
                        targets = new MethodBase[1]
                        {
                            AccessTools.Method(attribute.targetType, attribute.targetMethod, attribute.parameters,
                                attribute.generics)
                        };
                    }
                    else if (attribute.methodType == MethodType.Constructor)
                    {
                        targets = new MethodBase[1]
                       {
                            AccessTools.Constructor(attribute.targetType, attribute.parameters)
                       };
                    }
                    else
                    {
                        throw new Exception("Not implemented!");
                    }

                }
                else if (patchType == PatchType.empty)
                {
                    if (AccessTools.Method(declaringType, "TargetMethods") != null)
                    {
                        targets = (AccessTools.Method(declaringType, "TargetMethods").Invoke(null, null) as IEnumerable<MethodBase>).ToArray();
                    }
                    else
                    {
                        targets = new MethodBase[] { AccessTools.Method(declaringType, "TargetMethod").Invoke(null, null) as MethodBase };
                    }
                }
            }
            catch (Exception er)
            {
                Logger.Debug($"{PluginName}: target type {type.Name}", exception: er);
                throw er;
            }
        }

        public virtual void Patch(Harmony harmony)
        {
            if (prepare != null && !((bool)prepare.Invoke(null, null)))
            {
                Logger.Debug($"{PluginName}: Prepare failed for {attribute.targetType.Name ?? null}:{attribute.targetMethod ?? null}");
                return;
            }
            foreach (var target in targets.ToHashSet())
            {
                if (target == null || !target.IsValidTarget())
                {
                    Logger.Debug($"{PluginName}:[NOTANERROR] patching {target?.DeclaringType?.Name}:{target} is not possible! Patch attempt skipped", file: "Patching.log");
                    continue;
                }
                try
                {
                    HarmonyPriority priority;

                    int prefixPriority = -1;

                    if (prefix != null && prefix.HasAttribute<HarmonyPriority>() && prefix.TryGetAttribute<HarmonyPriority>(out priority))
                        prefixPriority = priority.info.priority;

                    int postfixPriority = -1;

                    if (postfix != null && postfix.HasAttribute<HarmonyPriority>() && postfix.TryGetAttribute<HarmonyPriority>(out priority))
                        postfixPriority = priority.info.priority;

                    int transpilerPriority = -1;

                    if (transpiler != null && transpiler.HasAttribute<HarmonyPriority>() && transpiler.TryGetAttribute<HarmonyPriority>(out priority))
                        transpilerPriority = priority.info.priority;

                    int finalizerPriority = -1;

                    if (finalizer != null && finalizer.HasAttribute<HarmonyPriority>() && finalizer.TryGetAttribute<HarmonyPriority>(out priority))
                        finalizerPriority = priority.info.priority;

                    replacement = harmony.Patch(target,
                        prefix: prefix != null ? new HarmonyMethod(prefix, priority: prefixPriority) : null,
                        postfix: postfix != null ? new HarmonyMethod(postfix, priority: postfixPriority) : null,
                        transpiler: transpiler != null ? new HarmonyMethod(transpiler, priority: transpilerPriority) : null,
                        finalizer: finalizer != null ? new HarmonyMethod(finalizer, priority: finalizerPriority) : null);

                    patchedSuccessfully = true;

                    this.OnPatchingSuccessful(replacement);

                    Logger.Debug($"{PluginName}:[NOTANERROR] patching {target?.DeclaringType?.Name}:{target} finished!");
                }
                catch (Exception er)
                {
                    this.OnPatchingFailed(er);
                    Logger.Debug($"Patching failed", exception: er);
                    Log.Warning($"{PluginName}:<color=orange>[ERROR]</color> <color=red>patching {target.DeclaringType.Name}:{target} Failed!</color> {er}");
                }
            }
        }

        public virtual void OnPatchingFailed(Exception er)
        {
            Log.Warning($"ROCKETMAN: Patching failed <color=red>{er}</color>");
        }

        public virtual void OnPatchingSuccessful(MethodBase replacement)
        {
        }
    }
}
