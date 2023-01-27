
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using HarmonyLib;
using Verse.Profile;

namespace CombatExtended.ExtendedLoadout;

[AttributeUsage(AttributeTargets.Method)]
public class ClearDataOnNewGame : Attribute {}

[HarmonyPatch(typeof(MemoryUtility), "ClearAllMapsAndWorld")]
public static class Harmony_ClearAllMapsAndWorld
{
    public static void Prefix()
    {
        DbgLog.Wrn($"ExtendedLoadout clearing initiated.");
        if (_clearDataMethods == null)
            _clearDataMethods = GetClearingMethods();
        _clearDataMethods?.ForEach(x => x.Invoke(null, null));
    }

    private static List<MethodInfo>? GetClearingMethods()
    {
        var allTypesInAsm = Assembly.GetExecutingAssembly().GetTypes();
        var methods = allTypesInAsm
            .SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(x => x.TryGetAttribute<ClearDataOnNewGame>(out _))
            .ToList();
        DbgLog.Wrn($"CollectClearingMethods: {String.Join("; ", methods.Select(x => $"{x.DeclaringType.Name}:{x.Name}"))}");
        return methods;
    }

    private static List<MethodInfo>? _clearDataMethods;
}