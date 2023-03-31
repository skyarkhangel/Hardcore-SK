using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RealisticTrade
{
    class RealisticTradeMod : Mod
    {
        public const string ModName = "Realistic Trade";
        public static RealisticTradeSettings settings;
        public RealisticTradeMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<RealisticTradeSettings>();
            settings.ReInitValues();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return ModName;
        }
    }
}