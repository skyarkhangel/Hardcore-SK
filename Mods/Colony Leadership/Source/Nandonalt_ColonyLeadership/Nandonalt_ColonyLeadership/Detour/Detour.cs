using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Nandonalt_ColonyLeadership.Detour
{

    // Detour code by Jecrell
    public static class Detours
    {
        private static List<string> detoured = new List<string>();

        private static List<string> destinations = new List<string>();

        public unsafe static bool TryDetourFromTo(MethodInfo source, MethodInfo destination)
        {
            bool flag = source == null;
            bool result;
            if (flag)
            {
                Log.Error("Source MethodInfo is null: Detours");
                result = false;
            }
            else
            {
                bool flag2 = destination == null;
                if (flag2)
                {
                    Log.Error("Destination MethodInfo is null: Detours");
                    result = false;
                }
                else
                {
                    string item = string.Concat(new string[]
                    {
                        source.DeclaringType.FullName,
                        ".",
                        source.Name,
                        " @ 0x",
                        source.MethodHandle.GetFunctionPointer().ToString("X" + (IntPtr.Size * 2).ToString())
                    });
                    string item2 = string.Concat(new string[]
                    {
                        destination.DeclaringType.FullName,
                        ".",
                        destination.Name,
                        " @ 0x",
                        destination.MethodHandle.GetFunctionPointer().ToString("X" + (IntPtr.Size * 2).ToString())
                    });
                    Detours.detoured.Add(item);
                    Detours.destinations.Add(item2);
                    bool flag3 = IntPtr.Size == 8;
                    if (flag3)
                    {
                        long num = source.MethodHandle.GetFunctionPointer().ToInt64();
                        long num2 = destination.MethodHandle.GetFunctionPointer().ToInt64();
                        byte* ptr = (byte*)num;
                        long* ptr2 = (long*)(ptr + 2);
                        *ptr = 72;
                        ptr[1] = 184;
                        *ptr2 = num2;
                        ptr[10] = 255;
                        ptr[11] = 224;
                    }
                    else
                    {
                        int num3 = source.MethodHandle.GetFunctionPointer().ToInt32();
                        int num4 = destination.MethodHandle.GetFunctionPointer().ToInt32();
                        byte* ptr3 = (byte*)num3;
                        int* ptr4 = (int*)(ptr3 + 1);
                        int num5 = num4 - num3 - 5;
                        *ptr3 = 233;
                        *ptr4 = num5;
                    }
                    result = true;
                }
            }
            return result;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    internal class DetourAttribute : Attribute
    {
        public Type source;

        public BindingFlags bindingFlags;

        public DetourAttribute(Type source)
        {
            this.source = source;
        }
    }
}
