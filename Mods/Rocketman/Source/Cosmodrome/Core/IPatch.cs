using System;
using HarmonyLib;

namespace RocketMan
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class IPatch : Attribute
    {
        public string targetMethod;
        public Type targetType;
        public Type[] parameters = null;
        public Type[] generics = null;
        public MethodType methodType;

        public readonly PatchType patchType;

        public IPatch()
        {
            this.patchType = PatchType.empty;
        }

        public IPatch(Type targetType, string targetMethod = null, MethodType methodType = MethodType.Normal, Type[] parameters = null, Type[] generics = null)
        {
            this.patchType = PatchType.normal;
            this.targetType = targetType;
            this.targetMethod = targetMethod;
            this.methodType = methodType;
            this.parameters = parameters;
            this.generics = generics;
        }
    }
}
