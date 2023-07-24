using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ModIndicator
{
    [StaticConstructorOnStartup]
    public static class Start
    {
        static Start()
        {
            var harmony = new Harmony("DimonSever000.ModIndicator");
            harmony.PatchAll();
            Log.Message("Mod Indicator patches loaded successfully");
        }
    }
}
